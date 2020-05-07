using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compile.Me.Shared;
using Compile.Me.Shared.Modals;
using Compile.Me.Shared.Types;
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
            await this._compilerPublisher.PublishCompileSourceRequest(new CompileSourceRequest(
                Guid.NewGuid(),
                CompileRequestType.SingleTest, 3, 128,
                new[] {"print('Hello: {}!'.format(input()))"}, "python",
                new CompileSourceTestBody(new[] {"bob"}, new[] {"Hello: Bob!"}))
            );

            return this.Ok();
        }
    }
}