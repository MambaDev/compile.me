using System;
using System.Collections.Generic;
using Compile.Me.Shared.Types;
using Newtonsoft.Json;

namespace compile.me.shared.Requests.MultipleCompileTestsSourceCompile
{
    public class CompileMultipleTestsSourceResponse : CompileResponseBase
    {
        /// <summary>
        /// The result of the executed tests cases.
        /// </summary>
        [JsonProperty("test_case_results")]
        public IReadOnlyList<(CompilerTestCaseResult result, SandboxResponseStatus status)> TestCaseResults
        {
            get;
            set;
        }

        public CompileMultipleTestsSourceResponse(Guid id, CompilerResult result,
            IReadOnlyList<(CompilerTestCaseResult result, SandboxResponseStatus status)> testCaseResults)
            : base(id, result)
        {
            this.TestCaseResults = testCaseResults;
        }

        public CompileMultipleTestsSourceResponse()
        {
        }
    }
}