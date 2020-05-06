using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Compile.Me.Shared;
using Compile.Me.Shared.Modals;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service;
using Compile.Me.Worker.Service.Events;
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
        /// Creates the new sandbox based on the given request and runs it.
        /// </summary>
        /// <param name="request">The request that will contain the sandbox details.</param>
        /// <returns></returns>
        internal async Task HandleCreateSandboxRequest(SandboxCreationRequest request)
        {
            var sandbox = new Sandbox(this._logger, this._dockerClient, request);
            this._executingSandboxes.Add(sandbox);

            sandbox.StatusChangeEvent += this.OnStatusChangeEvent;

            await sandbox.Run();
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
        private async void OnStatusChangeEvent(object sender, SandboxStatusChangeEventArgs args)
        {
            if (args.Status == Shared.Types.ContainerStatus.Removed)
                this.HandleSandboxComplete(args.Id).FireAndForgetSafeAsync(this.HandleSandboxCompleteFailedException);
        }


        /// <summary>
        /// Handles the case in which the handling of the sandbox complete failed. 
        /// </summary>
        /// <param name="exception">The exception caused during the timeout.</param>
        private void HandleSandboxCompleteFailedException(Exception exception)
        {
            this._logger.LogError($"error completing sandbox failed, error={exception.Message}");
        }

        /// <summary>
        /// Handles the case in which the container is complete, e.g getting the response and pushing it back
        /// onto the queue and ensuring that the container and its information is removed from memory.
        /// </summary>
        /// <param name="containerId">The id of the container being handled.</param>
        private async Task HandleSandboxComplete(string containerId)
        {
            var sandbox = this._executingSandboxes.FirstOrDefault(e => e.ContainerId == containerId);

            var response = sandbox.GetResponse();

            sandbox.StatusChangeEvent -= this.OnStatusChangeEvent;
            this._executingSandboxes.Remove(sandbox);

            // Publish the result back into the queue.
            var compileResponse = new CompileSourceResponse(sandbox.RequestId, response.StandardOutput,
                response.StandardErrorOutput, response.Result, response.Status);

            await this._publisher.PublishCompileSourceResponse(compileResponse);
        }

        #region IHost Start/Stop

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

        #endregion

        #region Worker Events

        /// <summary>
        /// Called when the application is starting.
        /// </summary>
        private void OnStart()
        {
            this._logger.LogInformation(("onStart has been called."));
            this._consumer = new Consumer("compiling", "request", new Config()
            {
                MaxInFlight = 2
            });

            this._consumer.AddHandler(new CompileQueueMessageHandler(this._logger, this), 4);
            this._consumer.ConnectToNSQLookupd(this._configuration.Consumer);
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
        /// Initializes a new instance of the <see cref="CompileQueueMessageHandler" /> class.
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
            var stringMessage = Encoding.UTF8.GetString(message.Body);
            var compileRequest = JsonConvert.DeserializeObject<CompileSourceRequest>(stringMessage);

            Trace.Assert(compileRequest != null && compileRequest.Id != Guid.Empty);

            var compiler = Constants.Compilers.FirstOrDefault(e => e.Language == compileRequest.Compiler);

            Trace.Assert(compiler != null);

            var sandboxCreationRequest = new SandboxCreationRequest(compileRequest.Id, compileRequest.TimeoutSeconds, 
                compileRequest.MemoryConstraint, $"./temp/{compiler.Language}/{Guid.NewGuid():N}/", 
                compileRequest.SourceCode, compileRequest.StdinData, compiler);

            this._compileService.HandleCreateSandboxRequest(sandboxCreationRequest)
                .FireAndForgetSafeAsync(this.HandleFailedSandboxCreationRequest);

            message.Finish();
        }

        /// <summary>
        /// Handles the case in which the sandbox request could not be created.
        /// </summary>
        private void HandleFailedSandboxCreationRequest(Exception e)
        {
            this._logger.LogError(e.Message);
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