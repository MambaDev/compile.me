using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using compile.me.shared.Modals;
using compile.me.worker.service.source.events;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace compile.me.worker.service
{
    public enum SandboxStatus
    {
        /// <summary>
        /// The sandbox is currently in a unknown state.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The box has just been created but has not been started yet.
        /// </summary>
        Created = 1,

        /// <summary>
        /// The sandbox has been started and will be begin executing the code.
        /// </summary>
        Started = 2,

        /// <summary>
        /// The docker container is being killed and thus is currently shutting down.
        /// </summary>
        Killing = 3,

        /// <summary>
        /// The docker container has been killed.
        /// </summary>
        Killed = 4,

        /// <summary>
        /// The container has been removed and thus ending its life cycle.
        /// </summary>
        Removed = 5
    }

    public class Sandbox
    {
        #region Fields

        /// <summary>
        /// The current status of the docker container/sanbox is currently in.
        /// </summary>
        private SandboxStatus _status = SandboxStatus.Unknown;

        /// <summary>
        /// The request that will be processed for the given sandbox. Including all the required code and compiler
        /// information that will be used.
        /// </summary>
        private readonly SandboxRequest _sandboxRequest;

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
        private readonly List<JSONMessage> _dockerSystemEventMessages = new List<JSONMessage>();

        /// <summary>
        /// The time out timer, this will be triggered when the container starts.
        /// If the given container is still executing when the timer is fired,
        /// then the container will be killed with no waiting period.
        /// </summary>
        private System.Timers.Timer _timeoutTimer;

        #endregion

        #region Properties

        /// <summary>
        /// The executing containers id.
        /// </summary>
        public string ContainerId { get; private set; }

        /// <summary>
        /// The current status of the docker container/sanbox is currently in.
        /// </summary>
        private SandboxStatus Status
        {
            get => this._status;
            set
            {
                // Update the local status event and trigger the update event.
                // if and only if the updated status is new and does not match
                // the already existing status.
                if (this._status != value) this.RaiseStatusChangeEvent(value);
                this._status = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the sandbox.
        /// </summary>
        /// <param name="dockerClient">The workers docker client that will be used to create the underlining container</param>
        /// <param name="sandboxRequest">The requesting information for the given sandbox.</param>
        /// <param name="logger">The workers logger that will be used to log information</param>
        public Sandbox(ILogger<CompilerService> logger, DockerClient dockerClient, SandboxRequest sandboxRequest)
        {
            this._logger = logger;
            this._dockerClient = dockerClient;
            this._sandboxRequest = sandboxRequest;

            // Setup the timeout timer for removing and stopping the container if its still executing after this
            // time limit.
            this._timeoutTimer = new System.Timers.Timer(this._sandboxRequest.TimeoutSeconds * 1000)
            {
                AutoReset = false,
                Enabled = false
            };

            this._timeoutTimer.Elapsed += this.SandboxTimeoutLimitExceeded;
        }

        /// <summary>
        /// Handle the case in which the container is still executing after the timeout
        /// limit has been reached.
        /// </summary>
        private void SandboxTimeoutLimitExceeded(object sender, ElapsedEventArgs e)
        {
            // If the status has been killed / removed at the point of which the timer was triggered.
            // Then don't bother doing anything and just return out.
            if (this.Status == SandboxStatus.Killed || this.Status == SandboxStatus.Removed) return;

            // TODO: KILL THE CONTAINER IF STILL RUNNING
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
                this._logger.LogError(error.Message);

                // If something went wrong, lets just clean up and exist now. Ensuring
                // that we clean up on the way out.
                await this.Cleanup();
            }
            finally
            {
                await this.Cleanup();
            }
        }

        /// <summary>
        /// Execute the sandbox environment, building up the arguments, creating the container and starting
        /// it. Everything after this point will be based on the stream of data being produced by the
        /// docker stream.
        /// </summary>
        private async Task Execute()
        {
            var language = this._sandboxRequest.Compiler.Language;
            var compiler = this._sandboxRequest.Compiler.CompilerEntry;

            var commandLine = new List<string>
            {
                "sh", "./script.sh", compiler, $"{language}.source", $"{language}.input",
                this._sandboxRequest.Compiler.Interpreter ? string.Empty : $"{language}.out.o",
                this._sandboxRequest.Compiler.AdditionalArguments,
                this._sandboxRequest.Compiler.StandardOutputFile,
                this._sandboxRequest.Compiler.StandardErrorFile
            };

            // The working directory just be in a unix based absolute format otherwise its not
            // going to work as expected and thus needs to be converted to ensure that it is in
            // that format.
            var workingDirectory = ConvertPathToUnix(this._sandboxRequest.Path);

            var container = await this._dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                WorkingDir = "/input",
                Image = this._sandboxRequest.Compiler.VirtualMachineName,
                NetworkDisabled = true,
                Entrypoint = commandLine,
                HostConfig = new HostConfig
                {
                    Binds = new List<string> {$"{workingDirectory}:/input"},
                    Memory = this._sandboxRequest.MemoryConstraint * 1000000,
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
            Directory.CreateDirectory(this._sandboxRequest.Path);

            // Write down the source code for the given request. 
            await File.WriteAllTextAsync(
                Path.Join(this._sandboxRequest.Path, $"{this._sandboxRequest.Compiler.Language}.source"),
                this._sandboxRequest.SourceCode);

            // Write down the standard input for the given request.
            await File.WriteAllTextAsync(
                Path.Join(this._sandboxRequest.Path, $"{this._sandboxRequest.Compiler.Language}.input"),
                this._sandboxRequest.StdinData);

            // Create the empty files for standard output and standard error output for the request.
            // This just ensures that we are always going to expect it to exist. Resolving file related
            // cases later on.
            foreach (var filePath in new List<string>
            {
                Path.Join(this._sandboxRequest.Path, this._sandboxRequest.Compiler.StandardOutputFile),
                Path.Join(this._sandboxRequest.Path, this._sandboxRequest.Compiler.StandardErrorFile)
            })
            {
                await File.Create(filePath).DisposeAsync();
            }

            // Finally copy in the script file that will be executed to execute the program.
            File.Copy(Path.Join(Directory.GetCurrentDirectory(), "/source/script.sh"),
                Path.Join(this._sandboxRequest.Path, "script.sh"));
        }

        /// <summary>
        /// Cleans up  the instance after executing the code. Including the volume folder for
        /// mounting the code, source source code and the input data that will be given to
        /// the executing compiler.
        ///
        /// Destroying the container if its still executing for any reason.
        /// </summary>
        private Task Cleanup()
        {
            // Remove the temporary directory and all its contents since at this point they should all be fully
            // read and ready to return from the completed request.
            Directory.Delete(this._sandboxRequest.Path, true);
            return Task.CompletedTask;
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
                    this.Status = SandboxStatus.Created;
                    break;
                case "start":
                    // Since the container has started, start the timeout timer.
                    this.Status = SandboxStatus.Started;
                    this._timeoutTimer.Start();
                    break;
                case "kill":
                    this.Status = SandboxStatus.Killing;
                    break;
                case "die":
                    // ensure to stop the kill timer if its still running.
                    this.Status = SandboxStatus.Killed;
                    this._timeoutTimer.Stop();
                    break;
                case "destroy":
                    // ensure to stop the kill timer if its still running.
                    this.Status = SandboxStatus.Removed;
                    this._timeoutTimer.Stop();
                    break;
                default:
                    this.Status = this.Status;
                    break;
            }
        }

        #region Events

        /// <summary>
        /// The delegate event handler for the status change event on the given sandbox.
        /// </summary>
        /// <param name="sender">The sending sandbox</param>
        /// <param name="args">The updated status/id of the sandbox.</param>
        public delegate void StatusChangeEventHandler(object sender, SandboxStatusChangeEventArgs args);

        /// <summary>
        ///  The status update event.
        /// </summary>
        public event StatusChangeEventHandler SandboxStatusChangeEvent;

        /// <summary>
        /// Raise the status update event for the given sandbox.
        /// </summary>
        /// <param name="status">The updated status.</param>
        protected void RaiseStatusChangeEvent(SandboxStatus status) =>
            this.SandboxStatusChangeEvent?.Invoke(this,
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
            this._dockerSystemEventMessages.Add(message);
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