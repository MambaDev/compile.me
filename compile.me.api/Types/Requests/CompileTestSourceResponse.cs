using System;
using Newtonsoft.Json;

namespace compile.me.api.Types.Requests
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
            CompilerTestCaseResult testCaseResult) : base(id, CompileRequestType.SingleTest, result)
        {
            this.TestCaseResult = testCaseResult;
            this.Status = status;
        }

        public CompileTestSourceResponse()
        {
        }

        /// <summary>
        /// Creates a new instance of the single test case response from the compile based response.
        /// </summary>
        /// <param name="response">the base response</param>
        /// <param name="testCaseResult">The executed test result.</param>
        /// <returns></returns>
        public static CompileTestSourceResponse FromSandboxCompileResponse(CompileSourceResponse response,
            CompilerTestCaseResult testCaseResult = null)
        {
            var singletTestResponse =
                new CompileTestSourceResponse(response.Id, response.Result, response.Status, testCaseResult);

            if (testCaseResult == null || testCaseResult.Result != CompilerTestResult.Failed)
                return singletTestResponse;

            singletTestResponse.Result = CompilerResult.Failed;
            singletTestResponse.Status = SandboxResponseStatus.TestFailed;

            return singletTestResponse;
        }
    }
}