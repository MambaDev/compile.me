using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compile.Me.Shared;
using Compile.Me.Shared.Modals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Compile.Me.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// The logger for the service.
        /// </summary>
        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// The publisher for pushing compile events into the queue.
        /// </summary>
        private readonly CompilerPublisher _compilerPublisher;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, CompilerPublisher publisher)
        {
            this._logger = logger;
            this._compilerPublisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await this._compilerPublisher.PublishCompileSourceRequest(new CompileSourceRequest()
            {
                Compiler = "python",
                Id = Guid.NewGuid(),
                SourceCode = "print('hello! {}'.format(input()))",
                StdinData = "bob",
            });

            return this.Ok();
        }
    }
}