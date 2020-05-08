using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Xml.XPath;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.Compile;

namespace Compile.Me.Worker.Service.Types.SingleTest
{
    public class SandboxSingleTestResponse : SandboxCompileResponse
    {
        /// <summary>
        /// The resulting test case of the execution.
        /// </summary>
        public CompilerTestCaseResult TestCaseResult { get; set; }

        public SandboxSingleTestResponse(IReadOnlyList<string> standardOutput,
            IReadOnlyList<string> standardErrorOutput, CompilerResult compilerResult,
            SandboxResponseStatus sandboxResponseStatus, CompilerTestCaseResult testCaseResult) : base(standardOutput,
            standardErrorOutput, compilerResult,
            sandboxResponseStatus)
        {
            this.TestCaseResult = testCaseResult;
        }

        public SandboxSingleTestResponse()
        {
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
            var singletTestResponse = new SandboxSingleTestResponse(response.StandardOutput,
                response.StandardErrorOutput,
                response.Result, response.Status, testCaseResult);

            if (testCaseResult == null || testCaseResult.Result != CompilerTestResult.Failed)
                return singletTestResponse;

            singletTestResponse.Result = CompilerResult.Failed;
            singletTestResponse.Status = SandboxResponseStatus.TestFailed;

            return singletTestResponse;
        }
    }
}