using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using compile.me.shared.Modals;
using compile.me.worker.service.source.events;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace compile.me.worker.service
{
    public class CompilerService : IHostedService
    {
        #region fields

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<CompilerService> _logger;

        /// <summary>
        /// The application life time.
        /// </summary>
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// The application configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The docker client that will be used to create, manage and work with containers.
        /// Including listening to and processing through the stream.
        /// </summary>
        private readonly DockerClient _dockerClient;

        /// <summary>
        /// A list of the current executing sandbox devices.
        /// </summary>
        private List<Sandbox> _executingSandboxes = new List<Sandbox>();

        #endregion

        /// <summary>
        ///  Creates a new instance of the compiler service.
        /// </summary>
        /// <param name="logger">The logger for the service.</param>
        /// <param name="applicationLifetime">The application lifetime.</param>
        /// <param name="configuration">The application configuration.</param>
        public CompilerService(ILogger<CompilerService> logger, IHostApplicationLifetime applicationLifetime,
            IConfiguration configuration)
        {
            this._logger = logger;
            this._hostApplicationLifetime = applicationLifetime;
            this._configuration = configuration;

            this._dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }


        /// <summary>
        /// Called when the application has started.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that would be used to stop the application.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await this._dockerClient.System.MonitorEventsAsync(new ContainerEventsParameters(),
                    new Progress<JSONMessage>(this.OnDockerSystemMessage), cancellationToken);
            }, cancellationToken);

            this._hostApplicationLifetime.ApplicationStarted.Register(this.onStart);
            this._hostApplicationLifetime.ApplicationStopping.Register(this.onStopping);
            this._hostApplicationLifetime.ApplicationStopped.Register(this.onStopped);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Called when the application should be stopping.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the stop.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle incoming messages from the docker stream, this will be used to ensure we know when the
        /// container has started and when the container has stopped. And thus allows us to understand
        /// if and when we should be killing the process.
        /// </summary>
        /// <param name="message">The message from the docker service.</param>
        private void OnDockerSystemMessage(JSONMessage message)
        {
            var relatedSandbox = this._executingSandboxes.FirstOrDefault(e => e.ContainerId == message.ID);

            // Add the docker event message,  this will trigger updates related to the status change and thus
            // firing out events that the container has completed and requires being removed. All following 
            // events and processes will relay on this. 
            //
            // regardless, if the container is still executing after N seconds (20 by default) it will be killed
            // this is just a fall back to protect us from it failing and the container still executing.
            relatedSandbox?.AddDockerEventMessage(message);
        }


        /// <summary>
        /// Handle status change events for the sandbox containers.
        /// </summary>
        /// <param name="sender">The container being triggered.</param>
        /// <param name="args">The arguments of the id and the status</param>
        private void SandboxOnSandboxStatusChangeEvent(object sender, SandboxStatusChangeEventArgs args)
        {
            this._logger.LogInformation($"update - id: {args.Id} - status: {args.Status}");
            // TODO: if removed, locate the reference to the container and clean it up (remove handles) 
        }

        #region Worker Events

        /// <summary>
        /// Called when the application is starting.
        /// </summary>
        private void onStart()
        {
            this._logger.LogInformation(("onStart has been called."));

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(100);
                var python = Constants.Compilers.First(e =>
                    e.Language.Equals("python", StringComparison.InvariantCultureIgnoreCase));

                var sandbox = new Sandbox(this._logger, this._dockerClient, new SandboxRequest()
                {
                    Path = $"./temp/python/{Guid.NewGuid():N}/",
                    SourceCode = "import time\ntime.sleep(5)\nprint('hi!')",
                    StdinData = "5",
                    Compiler = python,
                    TimeoutSeconds = 5,
                });

                this._executingSandboxes.Add(sandbox);

                sandbox.SandboxStatusChangeEvent += this.SandboxOnSandboxStatusChangeEvent;

                Task.Run(() => sandbox.Run());
            }
        }


        /// <summary>
        /// Called when the application is stopping.
        /// </summary>
        private void onStopping()
        {
            this._logger.LogInformation(("onStopping has been called."));
        }

        /// <summary>
        /// Called when the application has stopped.
        /// </summary>
        private void onStopped()
        {
            this._logger.LogInformation(("onStopped has been called."));
        }

        #endregion
    }
}