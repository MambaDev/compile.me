using System.Collections.Generic;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.Compile;

namespace Compile.Me.Worker.Service.Types.MultipleTests
{
    public class SandboxMultipleTestCreationResponse : SandboxResponseBase
    {
        /// <summary>
        /// The resulting test case of the execution.
        /// </summary>
        public IReadOnlyList<CompilerTestCaseResult> TestCasesResults { get;  }

        /// <summary>
        /// Creates a ne instance of the sandbox compile response. 
        /// </summary>
        /// <param name="compilerResult">The result of the compiling.</param>
        /// <param name="sandboxStatus">The status of the sandbox.</param>
        /// <param name="testsCasesResults">The result of the test cases.</param>
        public SandboxMultipleTestCreationResponse(CompilerResult compilerResult, SandboxResponseStatus sandboxStatus,
            IReadOnlyList<CompilerTestCaseResult> testsCasesResults) : base(compilerResult, sandboxStatus)
        {
            this.TestCasesResults = testsCasesResults;
        }
    }
}