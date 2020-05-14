using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using compile.me.api.Types;
using compile.me.api.Types.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace compile.me.api.Controllers
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
        //     await this._compilerPublisher.PublishCompileSourceRequest(new CompileSourceRequest(Guid.NewGuid(),
        //         3, 128, request.Source, request.Input.FirstOrDefault(), request.Language));

            //
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