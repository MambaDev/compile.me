using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using compile.me.service;
using compile.me.shared.Modals;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace compile.me.worker.service
{
    public class Sandbox
    {
        #region Fields

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

        #endregion

        #region Properties

        /// <summary>
        /// The executing containers id.
        /// </summary>
        public string ContainerId { get; private set; }

        #endregion

        /// <summary>
        /// Creates a new instance of the sandbox.
        /// </summary>
        /// <param name="sandboxRequest">The requesting information for the given sandbox.</param>
        public Sandbox(ILogger<CompilerService> logger, SandboxRequest sandboxRequest)
        {
            this._dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

            this._sandboxRequest = sandboxRequest;
            this._logger = logger;
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
            }
            finally
            {
                // Regardless of what actually happened, we need to clean up all the related files that are
                // mounted for the docker container. Including ensuring that the container has been fully
                // removed since we don't need any containers sitting around.
                await this.Cleanup();
            }
        }

        private async Task Execute()
        {
            var workingDirectory = this.ConvertPathToUnix(this._sandboxRequest.Path);

            var commandLine = new List<string>()
            {
                "sh", "./script.sh", this._sandboxRequest.Compiler.CompilerEntry,
                $"{this._sandboxRequest.Compiler.Language}.source",
                $"{this._sandboxRequest.Compiler.Language}.input",
                this._sandboxRequest.Compiler.Interpreter ? string.Empty : "out.o",
                this._sandboxRequest.Compiler.AdditionalArguments,
                this._sandboxRequest.Compiler.StandardOutputFile,
                this._sandboxRequest.Compiler.StandardErrorFile
            };

            var container = await this._dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                WorkingDir = "/input",
                Image = this._sandboxRequest.Compiler.VirtualMachineName,
                NetworkDisabled = true,
                Entrypoint = commandLine,
                HostConfig = new HostConfig()
                {
                    Binds = new List<string>() {$"{workingDirectory}:/input"},
                    Memory = this._sandboxRequest.MemoryConstraint * 1000000,
                    AutoRemove = true,
                },
            });

            // Bind the container id that will be later used to ensure that the container is removed and cleaned up
            // while additionally used to start the container.
            this.ContainerId = container.ID;
            await this._dockerClient.Containers.StartContainerAsync(this.ContainerId,
                new ContainerStartParameters() { });
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
            foreach (var filePath in new List<string>()
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
        private async Task Cleanup()
        {
            // If the container id is set, lets go and attempt to remove it.
            if (!string.IsNullOrWhiteSpace(this.ContainerId))
            {
                try
                {
                    await this._dockerClient.Containers.RemoveContainerAsync(this.ContainerId,
                        new ContainerRemoveParameters() {Force = true});
                }
                catch (Exception error)
                {
                }
            }

            // Remove the temporary directory and all its contents since at this point they should all be fully
            // read and ready to return from the completed request.
            Directory.Delete(this._sandboxRequest.Path, true);
        }

        /// <summary>
        /// Takes a valid windows complete path and converts it to a supporting unix version which allows for the support
        /// of absolute paths within docker.
        /// </summary>
        /// <param name="path">The path being converted.</param>
        private string ConvertPathToUnix(string path)
        {
            // Convert the working directory to a unix based format. Instead of C:\\a\\b => //c//a//b//
            var workingDirectorySplit = Path.GetFullPath(path).Split(":").ToList();
            var directory = workingDirectorySplit.TakeLast(workingDirectorySplit.Count - 1);
            var rootDrive = workingDirectorySplit[0].ToLower();

            return $"/{rootDrive}{string.Join("", directory)}".Replace("\\", "/");
        }
    }
}