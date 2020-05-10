using System.Collections.Generic;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.Compile;

namespace Compile.Me.Worker.Service.Types.SingleTest
{
    public class SandboxSingleTestResponse : SandboxResponseBase

    {
        /// <summary>
        /// The resulting test case of the execution.
        /// </summary>
        public CompilerTestCaseResult TestCaseResult { get; }

        /// <summary>
        /// Creates a ne instance of the sandbox compile response. 
        /// </summary>
        /// <param name="compilerResult">The result of the compiling.</param>
        /// <param name="sandboxStatus">The status of the sandbox.</param>
        /// <param name="testCaseResult">The result of the test case.</param>
        public SandboxSingleTestResponse(CompilerResult compilerResult, SandboxResponseStatus sandboxStatus,
            CompilerTestCaseResult testCaseResult) : base(compilerResult,
            sandboxStatus)
        {
            this.TestCaseResult = testCaseResult;
        }

        /// <summary>
        /// Creates a new instance of the single test case response from the compile based response.
        /// </summary>
        /// <param name="response">the base response</param>
        /// <param name="testCaseResult">The executed test result.</param>
        /// <returns></returns>
        public static SandboxSingleTestResponse FromSandboxCompileResponse(SandboxCompileResponse response,
            CompilerTestCaseResult testCaseResult = null)
        {
            var singletTestResponse =
                new SandboxSingleTestResponse(response.CompilerResult, response.SandboxStatus, testCaseResult);

            if (testCaseResult == null || testCaseResult.Result != CompilerTestResult.Failed)
                return singletTestResponse;

            singletTestResponse.CompilerResult = CompilerResult.Failed;
            singletTestResponse.SandboxStatus = SandboxResponseStatus.TestFailed;

            return singletTestResponse;
        }
    }
}