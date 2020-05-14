using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using compile.me.api.Types;
using compile.me.api.Types.Events;
using compile.me.api.Types.Requests;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureNSQSharp;
using CompileSourceResponse = compile.me.api.Types.Requests.CompileSourceResponse;
using Config = PureNSQSharp.Config;
using Message = PureNSQSharp.Message;

namespace compile.me.api.Services.Compiler
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
        private readonly List<CompileSandbox> _executingSandboxes = new List<CompileSandbox>();

        #endregion

        /// <summary>
        ///  Creates a new instance of the compiler service.
        /// </summary>
        /// <param name="logger">The logger for the service.</param>
        /// <param name="applicationLifetime">The application lifetime.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="publisher">The queue publisher used to sent messages into the queue.</param>
        public CompilerService(ILogger<CompilerService> logger, IHostApplicationLifetime applicationLifetime,
            IConfiguration configuration, CompilerPublisher publisher)
        {
            this._logger = logger;
            this._hostApplicationLifetime = applicationLifetime;
            this._publisher = publisher;


            this._configuration = configuration.GetSection("configuration").GetSection("compiler")
                .Get<CompileServiceConfiguration>();

            this._dockerClient = new DockerClientConfiguration().CreateClient();
        }

        /// <summary>
        /// Creates the new sandbox based on the given request and runs it.
        /// </summary>
        /// <param name="request">The request that will contain the sandbox details.</param>
        /// <param name="compiler">The compiler being used for the process.</param>
        internal async Task HandleSingleCompileSandboxRequest(CompileSourceRequest request, Types.Compiler compiler)
        {
            var sandbox = new CompileSandbox(this._logger, this._dockerClient, request, compiler);
            this.AddSandbox(sandbox);

            sandbox.StatusChangeEvent += this.OnStatusChangeEvent;
            sandbox.CompletedEvent += this.OnCompileSandboxCompletionEvent;

            await sandbox.Run();
        }

        /// <summary>
        /// Creates a new sandbox and performs the compiling process with a supporting test.
        /// </summary>
        /// <param name="request">The request that will contain the sandbox details.</param>
        /// <param name="compiler">The compiler being used for the process.</param>
        internal async Task HandleSingleCompileTestSandboxRequest(CompileTestSourceRequest request,
            Types.Compiler compiler)
        {
            var sandbox = new SingleTestSandbox(this._logger, this._dockerClient, request, compiler);

            this.AddSandbox(sandbox);

            sandbox.StatusChangeEvent += this.OnStatusChangeEvent;
            sandbox.CompletedEvent += this.OnSingleTestCompileSandboxCompletionEvent;

            await sandbox.Run();
        }


        /// <summary>
        /// Creates multiple sandboxes that will be used to execute multiple test cases. Completing once all
        /// tests have passed or any single test has failed.
        /// </summary>
        /// <param name="request">The request that will contain the compile details.</param>
        /// <param name="compiler">The compiler being used for the process.</param>
        internal async Task HandleMultipleCompileTestSandboxRequest(CompileMultipleTestsSourceRequest request,
            Types.Compiler compiler)
        {
            this._logger.LogInformation($"starting multiple test case execution for: " +
                                        $"{request.Id}, tests: {request.TestCases.Count}");

            var multipleWrapper =
                new MultipleTestSandboxWrapper(this._logger, this._dockerClient, this, request, compiler);

            multipleWrapper.CompletedEvent += this.OnMultipleSandboxCompletionEvent;

            await multipleWrapper.Start();
        }

        /// <summary>
        /// Handle incoming messages from the docker stream, this will be used to ensure we know when the
        /// container has started and when the container has stopped. And thus allows us to understand
        /// if and when we should be killing the process.
        /// </summary>
        /// <param name="message">The message from the docker service.</param>
        private void OnDockerSystemMessage(Docker.DotNet.Models.Message message)
        {
            var relatedSandbox = this.GetSandboxByContainerId(message.ID);

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
        private void OnStatusChangeEvent(object sender, SandboxStatusChangeEventArgs args)
        {
        }

        private void OnCompileSandboxCompletionEvent(object sender, CompileSandboxCompletionEventArgs args)
        {
            var sandbox = (CompileSandbox) sender;
            this.RemoveSandbox(sandbox);

            sandbox.StatusChangeEvent -= this.OnStatusChangeEvent;
            sandbox.CompletedEvent -= this.OnCompileSandboxCompletionEvent;

            var compileResponse = new CompileSourceResponse(args.Id, args.Response.StandardOutput, args.Response
                .StandardErrorOutput, args.Response.Result, args.Response.Status);

            this._logger.LogInformation($"compile: {JsonConvert.SerializeObject(compileResponse)}");

            this._publisher.PublishCompileSourceResponse(compileResponse)
                .FireAndForgetSafeAsync(this.HandleSandboxCompleteFailedException);
        }

        private void OnSingleTestCompileSandboxCompletionEvent(object sender,
            CompileSingleTestSandboxCompletionEventArgs args)
        {
            var sandbox = (SingleTestSandbox) sender;
            this.RemoveSandbox(sandbox);

            sandbox.StatusChangeEvent -= this.OnStatusChangeEvent;
            sandbox.CompletedEvent -= this.OnSingleTestCompileSandboxCompletionEvent;

            var response = new CompileTestSourceResponse(args.Id, args.Response.Result,
                args.Response.Status, args.Response.TestCaseResult);

            this._logger.LogInformation($"single test: {JsonConvert.SerializeObject(response)}");
            this._publisher.PublishSingleTestCompileSourceResponse(response)
                .FireAndForgetSafeAsync(this.HandleSandboxCompleteFailedException);
        }

        private void OnMultipleSandboxCompletionEvent(object sender, MultipleSandboxCompletionEventArgs args)
        {
            var wrapper = (MultipleTestSandboxWrapper) sender;
            wrapper.CompletedEvent -= this.OnMultipleSandboxCompletionEvent;

            // TODO: Track these and remove them once complete.

            var testCaseResult = args.Response.Results.Select(e =>
                    new CompileTestSourceResponse(args.Id, e.Result, e.Status, e.TestCaseResult))
                .ToList();

            var response = new CompileMultipleTestsSourceResponse(args.Id, args.Response.Result,
                args.Response.Status, testCaseResult);

            this._logger.LogInformation($"multiple test: {JsonConvert.SerializeObject(response)}");
            this._publisher.PublishMultipleTestCompileSourceResponse(response)
                .FireAndForgetSafeAsync(this.HandleSandboxCompleteFailedException);
        }


        /// <summary>
        /// Handles the case in which the handling of the sandbox complete failed. 
        /// </summary>
        /// <param name="exception">The exception caused during the timeout.</param>
        private void HandleSandboxCompleteFailedException(Exception exception)
        {
            this._logger.LogError($"error completing sandbox failed, error={exception.Message}");
        }

        #region IHost Start/Stop

        /// <summary>
        /// Called when the application has started.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that would be used to stop the application.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._dockerClient.System.MonitorEventsAsync(new ContainerEventsParameters(),
                new Progress<Docker.DotNet.Models.Message>(this.OnDockerSystemMessage), cancellationToken);

            this._hostApplicationLifetime.ApplicationStarted.Register(this.OnStart);
            this._hostApplicationLifetime.ApplicationStopping.Register(this.OnStopping);
            this._hostApplicationLifetime.ApplicationStopped.Register(this.OnStopped);

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
        /// Used to add sandboxes to the internal queue, allowing the boxes to get the updated events.
        /// </summary>
        /// <param name="sandbox">The sandbox being updated.</param>
        public void AddSandbox(CompileSandbox sandbox) => this._executingSandboxes.Add(sandbox);


        /// <summary>
        /// Removes the sandbox from the internal queue.
        /// </summary>
        /// <param name="sandbox">The sandbox being removed.</param>
        public void RemoveSandbox(CompileSandbox sandbox) => this._executingSandboxes.Remove(sandbox);

        /// <summary>
        /// Gets a given sandbox by the container id.
        /// </summary>
        /// <param name="id">The id of the container.</param>
        public CompileSandbox GetSandboxByContainerId(string id) =>
            this._executingSandboxes.FirstOrDefault(e => e.ContainerId == id);

        #endregion

        #region Worker Events

        /// <summary>
        /// Called when the application is starting.
        /// </summary>
        private void OnStart()
        {
            this._logger.LogInformation(("onStart has been called."));
            this._consumer = new Consumer("compiling", "request", new Config {MaxInFlight = 2});

            this._consumer.AddHandler(new CompileQueueMessageHandler(this._logger, this), 4);
            this._consumer.ConnectToNSQLookupd(this._configuration.Consumer);
        }

        /// <summary>
        /// Called when the application is stopping.
        /// </summary>
        private void OnStopping()
        {
            this._logger.LogInformation(("onStopping has been called."));
        }

        /// <summary>
        /// Called when the application has stopped.
        /// </summary>
        private void OnStopped()
        {
            this._logger.LogInformation(("onStopped has been called."));
        }

        #endregion
    }

    internal class CompileQueueMessageHandler : IHandler
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
        /// Initializes a new instance of the <see cref="CompileQueueMessageHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="compileService">The sentiment service.</param>
        internal CompileQueueMessageHandler(ILogger<CompilerService> logger, CompilerService compileService)
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
            var compileRequest = JsonConvert.DeserializeObject<CompileRequestBase>(stringMessage);

            Trace.Assert(compileRequest != null && compileRequest.Id != Guid.Empty);

            var compiler = Constants.Compilers.FirstOrDefault(e => e.Language == compileRequest.CompilerName);

            Trace.Assert(compiler != null);
            Task compilingTask;

            switch (compileRequest.Type)
            {
                case CompileRequestType.Compile:
                    var compile = JsonConvert.DeserializeObject<CompileSourceRequest>(stringMessage);
                    compilingTask = this._compileService.HandleSingleCompileSandboxRequest(compile, compiler);
                    break;
                case CompileRequestType.SingleTest:
                {
                    var single = JsonConvert.DeserializeObject<CompileTestSourceRequest>(stringMessage);
                    compilingTask = this._compileService.HandleSingleCompileTestSandboxRequest(single, compiler);
                    break;
                }
                case CompileRequestType.MultipleTests:
                {
                    var multi = JsonConvert.DeserializeObject<CompileMultipleTestsSourceRequest>(stringMessage);
                    compilingTask = this._compileService.HandleMultipleCompileTestSandboxRequest(multi, compiler);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            compilingTask.FireAndForgetSafeAsync(this.HandleFailedSandboxCreationRequest);

            message.Finish();
        }

        /// <summary>
        /// Handles the case in which the sandbox request could not be created.
        /// </summary>
        private void HandleFailedSandboxCreationRequest(Exception e) => this._logger.LogError(e.Message);

        /// <summary>
        /// Called when a message has exceeded the specified <see cref="Config.MaxAttempts" />.
        /// </summary>
        /// <param name="message">The failed message.</param>
        public void LogFailedMessage(IMessage message) => this._logger.LogError(Encoding.UTF8.GetString(message.Body));
    }
}