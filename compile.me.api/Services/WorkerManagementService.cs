using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using compile.me.api.Types;
using compile.me.api.Types.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureNSQSharp;

namespace compile.me.api.Services
{
    public class WorkerManagementService : IHostedService
    {
        /// <summary>
        /// The logger of the service.
        /// </summary>
        private readonly ILogger<WorkerManagementService> _logger;

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
        /// The subscriber
        /// </summary>
        private Consumer _consumer;

        /// <summary>
        ///  Creates a new instance of the compiler service.
        /// </summary>
        /// <param name="logger">The logger for the service.</param>
        /// <param name="applicationLifetime">The application lifetime.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="publisher">The queue publisher used to sent messages into the queue.</param>
        public WorkerManagementService(ILogger<WorkerManagementService> logger, IHostApplicationLifetime applicationLifetime,
            IConfiguration configuration, CompilerPublisher publisher)
        {
            this._logger = logger;
            this._hostApplicationLifetime = applicationLifetime;
            this._publisher = publisher;


            this._configuration = configuration.GetSection("configuration").GetSection("compiler")
                .Get<CompileServiceConfiguration>();
        }

        /// <summary>
        /// Called when the application has started.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that would be used to stop the application.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._hostApplicationLifetime.ApplicationStarted.Register(this.OnStart);
            this._hostApplicationLifetime.ApplicationStopping.Register(this.OnStopping);
            this._hostApplicationLifetime.ApplicationStopped.Register(this.OnStopped);

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #region Worker Events

        /// <summary>
        /// Called when the application is starting.
        /// </summary>
        private void OnStart()
        {
            this._logger.LogInformation(("onStart has been called."));
            this._consumer = new Consumer("compiled", "response", new Config {MaxInFlight = 2});

            this._consumer.AddHandler(new CompiledResponseQueueMessageHandler(this._logger), 4);
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

        #endregion}
    }

    internal class CompiledResponseQueueMessageHandler : IHandler
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<WorkerManagementService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledResponseQueueMessageHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        internal CompiledResponseQueueMessageHandler(ILogger<WorkerManagementService> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Handles the incoming compiled responses..
        /// </summary>
        /// <param name="message">The raw message.</param>
        public void HandleMessage(IMessage message)
        {
            var stringMessage = Encoding.UTF8.GetString(message.Body);
            var compileResponse = JsonConvert.DeserializeObject<CompileResponseBase>(stringMessage);

            Trace.Assert(compileResponse != null && compileResponse.Id != Guid.Empty);

            switch (compileResponse.Type)
            {
                case CompileRequestType.Compile:
                    var compile = JsonConvert.DeserializeObject<CompileSourceResponse>(stringMessage);
                    this._logger.LogInformation($"compile response: {stringMessage}");
                    break;
                case CompileRequestType.SingleTest:
                {
                    var single = JsonConvert.DeserializeObject<CompileTestSourceResponse>(stringMessage);
                    this._logger.LogInformation($"single test response: {stringMessage}");
                    break;
                }
                case CompileRequestType.MultipleTests:
                {
                    var multi = JsonConvert.DeserializeObject<CompileMultipleTestsSourceResponse>(stringMessage);
                    this._logger.LogInformation($"multiple test response: {stringMessage}");
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            message.Finish();
        }

        /// <summary>
        /// Called when a message has exceeded the specified <see cref="Config.MaxAttempts" />.
        /// </summary>
        /// <param name="message">The failed message.</param>
        public void LogFailedMessage(IMessage message) => this._logger.LogError(Encoding.UTF8.GetString(message.Body));
    }
}