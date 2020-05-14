using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace compile.me.api.Types.Requests
{
    public class CompileMultipleTestsSourceResponse : CompileResponseBase
    {
        /// <summary>
        /// The result of the executed tests cases.
        /// </summary>
        [JsonProperty("test_case_results")]
        public IReadOnlyList<CompileTestSourceResponse> Results { get; set; }

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        [JsonProperty("status")]
        public SandboxResponseStatus Status { get; set; }

        public CompileMultipleTestsSourceResponse(Guid id, CompilerResult result, SandboxResponseStatus status,
            IReadOnlyList<CompileTestSourceResponse> results)
            : base(id, CompileRequestType.MultipleTests, result)
        {
            this.Results = results;
            this.Status = status;
        }

        public CompileMultipleTestsSourceResponse()
        {
        }
    }
}