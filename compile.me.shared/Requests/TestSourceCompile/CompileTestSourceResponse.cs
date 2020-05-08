using System;
using Compile.Me.Shared.Types;
using Newtonsoft.Json;

namespace compile.me.shared.Requests.TestSourceCompile
{
    public class CompileTestSourceResponse : CompileResponseBase
    {
        /// <summary>
        /// The result of the executed test case.
        /// </summary>
        [JsonProperty("test_case_result")]
        public CompilerTestCaseResult TestCaseResult { get; set; }

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        [JsonProperty("status")]
        public SandboxResponseStatus Status { get; set; }

        /// <inheritdoc cref="CompileResponseBase"/>
        /// <summary>
        /// Creates a new instance of the compile source response.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <param name="status">The status of the sandbox after finishing.</param>
        /// <param name="testCaseResult">The result of the test.</param>
        public CompileTestSourceResponse(Guid id, CompilerResult result, SandboxResponseStatus status,
            CompilerTestCaseResult testCaseResult) : base(id, result)
        {
            this.TestCaseResult = testCaseResult;
            this.Status = status;
        }

        public CompileTestSourceResponse()
        {
        }
    }
}