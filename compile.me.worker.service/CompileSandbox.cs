﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Events;
using Compile.Me.Worker.Service.Types.Compile;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using ContainerStatus = Compile.Me.Shared.Types.ContainerStatus;

namespace Compile.Me.Worker.Service
{
    public class CompileSandbox
    {
        #region Fields

        /// <summary>
        /// The current status of the sandbox.
        /// </summary>
        private SandboxStatus _sandboxStatus = new SandboxStatus();

        /// <summary>
        /// The client that will be used to communicate with the docker demon and perform the code execution.
        /// </summary>
        private readonly DockerClient _dockerClient;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<CompilerService> _logger;

        /// <summary>
        /// The related docker system event messages that are related to this sandbox/container.
        /// </summary>
        private readonly List<JSONMessage> _containerEventmessages = new List<JSONMessage>();

        /// <summary>
        /// The time out timer, this will be triggered when the container starts.
        /// If the given container is still executing when the timer is fired,
        /// then the container will be killed with no waiting period.
        /// </summary>
        private readonly System.Timers.Timer _timeoutTimer;

        /// <summary>
        /// The request that will be processed for the given sandbox. Including all the required code and compiler
        /// information that will be used.
        /// </summary>
        protected readonly SandboxCompileCreationRequest SandboxCompileRequest;

        /// <summary>
        /// The finalized response of the sandbox.
        /// </summary>
        protected readonly SandboxCompileResponse SandboxCompileResponse = new SandboxCompileResponse();

        #endregion

        #region Properties

        /// <summary>
        /// The current status of the docker container/sanbox is currently in.
        /// </summary>
        private ContainerStatus Status
        {
            get => this._sandboxStatus.ContainerStatus;
            set
            {
                // Update the local status event and trigger the update event.
                // if and only if the updated status is new and does not match
                // the already existing status.
                if (this._sandboxStatus.ContainerStatus != value) this.RaiseStatusChangeEvent(value);
                this._sandboxStatus.ContainerStatus = value;
            }
        }

        /// <summary>
        /// The executing containers id.
        /// </summary>
        public string ContainerId { get; private set; }

        /// <summary>
        /// The id of the request, used to perform related correlation between the request and the response.
        /// </summary>
        public Guid RequestId => this.SandboxCompileRequest.Id;

        #endregion

        /// <summary>
        /// Creates a new instance of the sandbox.
        /// </summary>
        /// <param name="dockerClient">The workers docker client that will be used to create the underlining container</param>
        /// <param name="sandboxCompileRequest">The requesting information for the given sandbox.</param>
        /// <param name="logger">The workers logger that will be used to log information</param>
        public CompileSandbox(ILogger<CompilerService> logger, DockerClient dockerClient,
            SandboxCompileCreationRequest sandboxCompileRequest)
        {
            this._logger = logger;
            this._dockerClient = dockerClient;
            this.SandboxCompileRequest = sandboxCompileRequest;

            // Setup the timeout timer for removing and stopping the container if its still executing after this
            // time limit.
            this._timeoutTimer = new System.Timers.Timer(this.SandboxCompileRequest.TimeoutSeconds * 1000)
            {
                AutoReset = false,
                Enabled = false
            };

            this._timeoutTimer.Elapsed += this.SandboxTimeoutLimitExceeded;
        }


        /// <summary>
        /// Run the sandbox container with the given configuration options. 
        /// </summary>
        /// <returns>The details of the response, including status, and both standard/error outputs.</returns>
        public async Task Run()
        {
            try
            {
                // Ensure to go through the preparing process, setting up volumes, files.
                await this.Prepare();

                // Execute and run the sandbox container.
                await this.Execute();
            }
            catch (Exception error)
            {
                this._logger.LogError($"error attempting to execute container, error={error.Message}");

                // Stop the container in the same method the timeout would.
                this.StopContainer().FireAndForgetSafeAsync(this.HandleTimeoutStopContainerException);

                // If something went wrong, lets just clean up and exist now. Ensuring
                // that we clean up on the way out.
                this.Cleanup();
            }
        }

        /// <summary>
        /// Execute the sandbox environment, building up the arguments, creating the container and starting
        /// it. Everything after this point will be based on the stream of data being produced by the
        /// docker stream.
        /// </summary>
        private async Task Execute()
        {
            var language = this.SandboxCompileRequest.Compiler.Language;
            var compiler = this.SandboxCompileRequest.Compiler.CompilerEntry;

            var commandLine = new List<string>
            {
                "sh", "./script.sh", compiler, $"{language}.source", $"{language}.input",
                this.SandboxCompileRequest.Compiler.Interpreter ? string.Empty : $"{language}.out.o",
                this.SandboxCompileRequest.Compiler.AdditionalArguments,
                this.SandboxCompileRequest.Compiler.StandardOutputFile,
                this.SandboxCompileRequest.Compiler.StandardErrorFile
            };

            // The working directory just be in a unix based absolute format otherwise its not
            // going to work as expected and thus needs to be converted to ensure that it is in
            // that format.
            var workingDirectory = ConvertPathToUnix(this.SandboxCompileRequest.Path);

            var container = await this._dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                WorkingDir = "/input",
                Image = this.SandboxCompileRequest.Compiler.VirtualMachineName,
                NetworkDisabled = true,
                Entrypoint = commandLine,
                HostConfig = new HostConfig
                {
                    Binds = new List<string> {$"{workingDirectory}:/input"},
                    Memory = this.SandboxCompileRequest.MemoryConstraint * 1000000,
                    AutoRemove = true
                }
            });

            // Bind the container id that will be later used to ensure that the container is removed and cleaned up
            // while additionally used to start the container.
            this.ContainerId = container.ID;

            await this._dockerClient.Containers.StartContainerAsync(this.ContainerId,
                new ContainerStartParameters());
        }

        /// <summary>
        /// Prepare the instance for executing the code. Including setting up the volume folder for
        /// mounting the code, writing down the source code and the input data that will be given to
        /// the executing compiler.
        /// </summary>
        private async Task Prepare()
        {
            // Create the temporary directory that will be used for storing the source code, standard input and then
            // the location in which the compiler will write the standard output and the standard error output.
            // After the data is written and returned, the location will be deleted.
            Directory.CreateDirectory(this.SandboxCompileRequest.Path);

            await using (var sourceFile = new StreamWriter(Path.Join(this.SandboxCompileRequest.Path,
                $"{this.SandboxCompileRequest.Compiler.Language}.source")))
            {
                for (var i = 0; i < this.SandboxCompileRequest.SourceCode.Count; i++)
                {
                    await sourceFile.WriteAsync(this.SandboxCompileRequest.SourceCode[i]);

                    if (i != this.SandboxCompileRequest.SourceCode.Count - 1)
                        await sourceFile.WriteAsync(Environment.NewLine);
                }
            }

            await using (var stdinDataFile = new StreamWriter(Path.Join(this.SandboxCompileRequest.Path,
                $"{this.SandboxCompileRequest.Compiler.Language}.input")))
            {
                for (var i = 0; i < this.SandboxCompileRequest.StdinData.Count; i++)
                {
                    await stdinDataFile.WriteAsync(this.SandboxCompileRequest.StdinData[i]);

                    if (i != this.SandboxCompileRequest.StdinData.Count - 1)
                        await stdinDataFile.WriteAsync(Environment.NewLine);
                }
            }

            // Create the empty files for standard output and standard error output for the request.
            // This just ensures that we are always going to expect it to exist. Resolving file related
            // cases later on.
            foreach (var filePath in new List<string>
            {
                Path.Join(this.SandboxCompileRequest.Path, this.SandboxCompileRequest.Compiler.StandardOutputFile),
                Path.Join(this.SandboxCompileRequest.Path, this.SandboxCompileRequest.Compiler.StandardErrorFile)
            })
            {
                await File.Create(filePath).DisposeAsync();
            }

            // Finally copy in the script file that will be executed to execute the program.
            File.Copy(Path.Join(Directory.GetCurrentDirectory(), "/source/script.sh"),
                Path.Join(this.SandboxCompileRequest.Path, "script.sh"));
        }

        /// <summary>
        /// Cleans up  the instance after executing the code. Including the volume folder for
        /// mounting the code, source source code and the input data that will be given to
        /// the executing compiler.
        ///
        /// Destroying the container if its still executing for any reason.
        /// </summary>
        private void Cleanup()
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(this.SandboxCompileRequest?.Path));

            // Remove the temporary directory and all its contents since at this point they should all be fully
            // read and ready to return from the completed request.
            Directory.Delete(this.SandboxCompileRequest?.Path, true);
        }

        /// <summary>
        /// Loads the response of the sandbox execution, the standard output.
        /// </summary>
        /// <returns>The standard output of the sandbox.</returns>
        private async Task<IReadOnlyList<string>> GetSandboxStandardOutput()
        {
            var path = Path.Join(this.SandboxCompileRequest.Path,
                this.SandboxCompileRequest.Compiler.StandardOutputFile);

            // If the file does not exist, no output is returned.
            if (!File.Exists(path)) return new string[] { };

            // Read up to 50 lines.
            var maxStandardOutRead = 50;

            var lines = new List<string>();

            using var openFile = new StreamReader(path);

            for (var i = 0; i < maxStandardOutRead; i++)
            {
                var line = await openFile.ReadLineAsync();
                if (line == null) break;

                lines.Add(line);
            }

            return lines;
        }


        /// <summary>
        /// Loads the response of the sandbox error execution, the standard error output.
        /// </summary>
        /// <returns>The standard error output of the sandbox.</returns>
        private async Task<IReadOnlyList<string>> GetLoadSandboxStandardErrorOutput()
        {
            var path = Path.Join(this.SandboxCompileRequest.Path,
                this.SandboxCompileRequest.Compiler.StandardErrorFile);


            // If the file does not exist, no output is returned.
            if (!File.Exists(path)) return new string[] { };

            // Read up to 50 lines.
            const int maxStandardOutRead = 50;

            var lines = new List<string>();

            using var openFile = new StreamReader(path);

            for (var i = 0; i < maxStandardOutRead; i++)
            {
                var line = await openFile.ReadLineAsync();

                // if we have met the end of the file before we have actually
                // hit our max limit, break;
                if (line == null) break;

                lines.Add(line);
            }

            return lines;
        }


        /// <summary>
        /// Get the response of the sandbox, can only be called once in removed state.
        /// </summary>
        /// <returns></returns>
        public async Task<SandboxCompileResponse> GetResponse()
        {
            Trace.Assert(this.Status != ContainerStatus.Removed, "Sandbox response cannot be returned if not removed.");

            try
            {
                // Start  in a failed state, and update as and when it changes.
                this.SandboxCompileResponse.Result = CompilerResult.Failed;
                this.SandboxCompileResponse.Status = SandboxResponseStatus.Finished;

                if (this._sandboxStatus.ExceededMemoryLimit || this._sandboxStatus.ExceededTimeoutLimit)
                {
                    this.SandboxCompileResponse.Status = this._sandboxStatus.ExceededMemoryLimit
                        ? SandboxResponseStatus.MemoryConstraintExceeded
                        : SandboxResponseStatus.TimeLimitExceeded;

                    return this.SandboxCompileResponse;
                }

                this.SandboxCompileResponse.StandardErrorOutput = await this.GetLoadSandboxStandardErrorOutput();

                // If error exists, then don't bother loading the standard output.
                if (this.SandboxCompileResponse.StandardErrorOutput != null &&
                    this.SandboxCompileResponse.StandardErrorOutput.Any())
                    return this.SandboxCompileResponse;

                this.SandboxCompileResponse.StandardOutput = await this.GetSandboxStandardOutput();

                this.SandboxCompileResponse.Result = CompilerResult.Succeeded;
                return this.SandboxCompileResponse;
            }
            finally
            {
                this.Cleanup();
            }
        }


        /// <summary>
        /// Stops the underlining container if its running (this is expected to also remove the container since it
        /// has the auto remove flag set).
        /// </summary>
        private async Task StopContainer()
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(this.ContainerId));

            // Stop the container and don't wait any number of seconds to kill it. e.g remove it
            // right away if it fails to stop.
            await this._dockerClient.Containers.StopContainerAsync(this.ContainerId,
                new ContainerStopParameters() {WaitBeforeKillSeconds = 1}, CancellationToken.None);
        }

        /// <summary>
        ///  Based on the docker system even for the given sandbox, update its related status.
        /// </summary>
        /// <param name="status">The status update from the docker system message.</param>
        private void UpdateStatusFromDockerEventMessageStatus(string status)
        {
            switch (status)
            {
                case "create":
                    this.HandleContainerCreated();
                    break;
                case "start":
                    this.HandleContainerStarted();
                    break;
                case "kill":
                    this.HandleContainerKilling();
                    break;
                case "die":
                    this.HandleContainerKilled();
                    break;
                case "destroy":
                    this.HandleContainerRemoved();
                    break;
                default:
                    this.Status = this.Status;
                    break;
            }
        }

        #region Container Status Handlers

        /// <summary>
        /// Handles the case in which the given container has been created.
        /// </summary>
        private void HandleContainerCreated()
        {
            this.Status = ContainerStatus.Created;
        }

        /// <summary>
        /// Handles the case in which the given container has been started.
        /// </summary>
        private void HandleContainerStarted()
        {
            this.Status = ContainerStatus.Started;

            // Start the one shot timeout timer that will be used to ensure that the 
            // given container does not seed its time/execution limit.
            this._timeoutTimer.Start();
        }

        /// <summary>
        /// Handles the case in which the given container is being killed.
        /// </summary>
        private void HandleContainerKilling()
        {
            this.Status = ContainerStatus.Killing;
        }

        /// <summary>
        /// Handles the case in which the given container has been killed.
        /// </summary>
        private void HandleContainerKilled()
        {
            this._timeoutTimer.Stop();

            // Ensure that the status is the last thing updated, since this will trigger the
            // event, and we don't want the worker service knowing we are "killed" until
            // ready.
            this.Status = ContainerStatus.Killed;
        }

        /// <summary>
        /// Handles the case in which the given container has been removed.
        /// </summary>
        private void HandleContainerRemoved()
        {
            // Ensure that the status is the last thing updated, since this will trigger the
            // event, and we don't want the worker service knowing we are "removed" until we
            // have loaded the data ready for finishing.
            this.Status = ContainerStatus.Removed;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle the case in which the container is still executing after the timeout
        /// limit has been reached.
        /// </summary>
        private void SandboxTimeoutLimitExceeded(object sender, ElapsedEventArgs e)
        {
            // Just ensure that the timer is not going to run again.
            this._timeoutTimer.AutoReset = false;
            this._timeoutTimer.Enabled = false;

            // If the status has been killed / removed at the point of which the timer was triggered.
            // Then don't bother doing anything and just return out.
            if (this.Status == ContainerStatus.Killed || this.Status == ContainerStatus.Removed) return;

            this._logger.LogWarning($"container {this.ContainerId}[{this.Status}] has exceeded timeout, stopping");
            this._sandboxStatus.ExceededTimeoutLimit = true;

            this.StopContainer().FireAndForgetSafeAsync(this.HandleTimeoutStopContainerException);
        }

        /// <summary>
        /// Handles the case in which the container failed to stop after the sandbox timeout event fired.;w
        /// 
        /// </summary>
        /// <param name="exception">The exception caused during the timeout.</param>
        private void HandleTimeoutStopContainerException(Exception exception)
        {
            this._logger.LogError($"error stopping container on sandbox timeout, error={exception.Message}");
        }

        /// <summary>
        /// The delegate event handler for the status change event on the given sandbox.
        /// </summary>
        /// <param name="sender">The sending sandbox</param>
        /// <param name="args">The updated status/id of the sandbox.</param>
        public delegate void StatusChangeEventHandler(object sender, SandboxStatusChangeEventArgs args);

        /// <summary>
        ///  The status update event.
        /// </summary>
        public event StatusChangeEventHandler StatusChangeEvent;

        /// <summary>
        /// Raise the status update event for the given sandbox.
        /// </summary>
        /// <param name="status">The updated status.</param>
        private void RaiseStatusChangeEvent(ContainerStatus status) =>
            this.StatusChangeEvent?.Invoke(this,
                new SandboxStatusChangeEventArgs(this.ContainerId, status));

        #endregion

        /// <summary>
        /// Adds a new docker event message that is related to the given container for the sandbox.
        /// This will be used to also update its current status.
        /// </summary>
        /// <param name="message">The message that was triggered within the docker container.</param>
        public void AddDockerEventMessage(JSONMessage message)
        {
            this.UpdateStatusFromDockerEventMessageStatus(message.Status);
            this._containerEventmessages.Add(message);
        }

        /// <summary>
        /// Takes a valid windows complete path and converts it to a supporting unix version which allows for the support
        /// of absolute paths within docker.
        /// </summary>
        /// <param name="path">The path being converted.</param>
        private static string ConvertPathToUnix(string path)
        {
            // Convert the working directory to a unix based format. Instead of C:\\a\\b => //c//a//b//
            var workingDirectorySplit = Path.GetFullPath(path).Split(":").ToList();
            var directory = workingDirectorySplit.TakeLast(workingDirectorySplit.Count - 1);

            var rootDrive = workingDirectorySplit[0].ToLower();
            return $"/{rootDrive}{string.Join("", directory)}".Replace("\\", "/");
        }
    }
}