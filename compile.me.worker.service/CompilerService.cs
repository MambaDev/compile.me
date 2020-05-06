using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using compile.me.shared;
using Compile.Me.Shared.Modals;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Service;
using Compile.Me.Worker.Service.Service.source.events;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureNSQSharp;
using Config = PureNSQSharp.Config;

namespace Compile.Me.Worker.Service
{
    public class CompilerService : IHostedService
    {
        #region fields

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<CompilerService> _logger;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly CompileServiceConfiguration _configuration;

        /// <summary>
        /// The application life time.
        /// </summary>
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// The compiling publisher that would be used to publish the data back to the
        /// queue once the job has been completed.
        /// </summary>
        private readonly CompilerPublisher _publisher;

        /// <summary>
        /// The docker client that will be used to create, manage and work with containers.
        /// Including listening to and processing through the stream.
        /// </summary>
        private readonly DockerClient _dockerClient;

        /// <summary>
        /// The subscriber
        /// </summary>
        private Consumer _consumer;


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
            IConfiguration configuration, CompilerPublisher publisher)
        {
            this._logger = logger;
            this._hostApplicationLifetime = applicationLifetime;
            this._publisher = publisher;


            this._configuration = configuration.GetSection("configuration").GetSection("compiler")
                .Get<CompileServiceConfiguration>();

            this._dockerClient = new DockerClientConfiguration(new Uri(this._configuration.Docker)).CreateClient();
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

            this._hostApplicationLifetime.ApplicationStarted.Register(this.OnStart);
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
            if (args.Status == Shared.Types.ContainerStatus.Removed) this.HandleSandboxComplete(args.Id);
        }


        /// <summary>
        /// Handles the case in which the container is complete, e.g getting the response and pushing it back
        /// onto the queue and ensuring that the container and its information is removed from memory.
        /// </summary>
        /// <param name="containerId">The id of the container being handled.</param>
        private void HandleSandboxComplete(string containerId)
        {
            var sandbox = this._executingSandboxes.FirstOrDefault(e => e.ContainerId == containerId);
            var response = JsonConvert.SerializeObject(sandbox.GetResponse());

            this._executingSandboxes.Remove(sandbox);

            this._logger.LogInformation($"{containerId.Substring(0, 5)}: - result {response}");
        }

        #region Worker Events

        /// <summary>
        /// Called when the application is starting.
        /// </summary>
        private void OnStart()
        {
            this._logger.LogInformation(("onStart has been called."));

            this._consumer = new Consumer("compiling", "request", new Config() {MaxInFlight = 2});
            this._consumer.AddHandler(new CompileQueueMessageHandler(this._logger, this), 4);
            // this._consumer.ConnectToNSQd(this._configuration.Consumer);


            for (var i = 0; i < 5; i++)
            {
                var python = Constants.Compilers.First(e =>
                    e.Language.Equals("python", StringComparison.InvariantCultureIgnoreCase));

                var sandbox = new Sandbox(this._logger, this._dockerClient, new SandboxRequest()
                {
                    Path = $"./temp/{python.Language}/{Guid.NewGuid():N}/",
                    SourceCode = "print('hello: {}!'.format(input()))",
                    StdinData = (i + 1) % 2 == 0 ? "bob" : "james",
                    Compiler = python,
                    TimeoutSeconds = 3,
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

    public class CompileQueueMessageHandler : IHandler
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<CompilerService> _logger;

        /// <summary>
        /// The compiler service
        /// </summary>
        private readonly CompilerService _compileService;

        /// <summary>
        /// The lock instance
        /// </summary>
        private readonly object LockInstance = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitchMessageHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="compileService">The sentiment service.</param>
        public CompileQueueMessageHandler(ILogger<CompilerService> logger, CompilerService compileService)
        {
            this._logger = logger;
            this._compileService = compileService;
        }

        /// <summary>
        /// Handles the incoming compile requests.
        /// </summary>
        /// <param name="message">The raw message.</param>
        public void HandleMessage(IMessage message)
        {
            // var compileRequest = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(message.Body));
            this._logger.LogInformation(Encoding.UTF8.GetString(message.Body));
        }


        /// <summary>
        /// Called when a message has exceeded the specified <see cref="Config.MaxAttempts" />.
        /// </summary>
        /// <param name="message">The failed message.</param>
        public void LogFailedMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            this._logger.LogError(msg);
        }
    }
}