using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Compile.Me.Shared;
using compile.me.shared.Modals.SourceCompile;
using compile.me.shared.Requests.MultipleCompileTestsSourceCompile;
using compile.me.shared.Requests.SourceCompile;
using compile.me.shared.Requests.TestSourceCompile;
using Compile.Me.Shared.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Logging;

namespace Compile.Me.Api.Controllers
{
    public class CompileRequest
    {
        public IReadOnlyList<string> Source { get; set; }
        public IReadOnlyList<IReadOnlyList<string>> Input { get; set; }
        public IReadOnlyList<IReadOnlyList<string>> Tests { get; set; }
        public string Language { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class CompileController : ControllerBase
    {
        /// <summary>
        /// The logger for the service.
        /// </summary>
        private readonly ILogger<CompileController> _logger;

        /// <summary>
        /// The publisher for pushing compile events into the queue.
        /// </summary>
        private readonly CompilerPublisher _compilerPublisher;

        public CompileController(ILogger<CompileController> logger, CompilerPublisher publisher)
        {
            this._logger = logger;
            this._compilerPublisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> CompileSource([FromBody] CompileRequest request)
        {
            // await this._compilerPublisher.PublishCompileSourceRequest(new CompileSourceRequest(Guid.NewGuid(),
            //     3, 128, request.Source, request.Input, request.Language));


            var testCases = new List<CompilerTestCase>();

            for (var i = 0; i < request.Tests.Count; i++)
            {
                testCases.Add(new CompilerTestCase(Guid.NewGuid(), request.Input[i], request.Tests[i]));
            }

            await this._compilerPublisher.PublishMultipleTestCompileSourceRequest(
                new CompileMultipleTestsSourceRequest(Guid.NewGuid(), 3, 128, request.Source,
                    request.Language, testCases, false, false));

            return this.Ok();
        }
    }
}