using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using compile.me.shared.Modals;
using compile.me.worker.service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace compile.me.service
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
        }


        /// <summary>
        /// Called when the application has started.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that would be used to stop the application.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
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
        /// Called when the application is starting.
        /// </summary>
        private void onStart()
        {
            this._logger.LogInformation(("onStart has been called."));

            var python = Constants.Compilers.First(e =>
                e.Language.Equals("python", StringComparison.InvariantCultureIgnoreCase));

            var sandbox = new Sandbox(this._logger, new SandboxRequest()
            {
                Path = $"./temp/python/{Guid.NewGuid():N}/",
                SourceCode = "print('hello anna! {}, input={}'.format(5 * 10 * 11, input()))",
                StdinData = "5",
                Compiler = python,
                TimeoutSeconds = 20,
            });

            // Execute the sample sandbox and log the output.
            sandbox.Run().ContinueWith((t) => { this._logger.LogInformation("completed"); })
                .ContinueWith((t) => { this._logger.LogError(t.Exception.Message); },
                    CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
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
    }
}