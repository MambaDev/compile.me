using System;
using Compile.Me.Shared.Modals;
using Compile.Me.Shared.Types;

namespace compile.me.shared.Requests.TestSourceCompile
{
    public class CompileTestSourceResponse : CompileResponseBase
    {
        /// <summary>
        /// The result of the executed test case.
        /// </summary>
        public CompilerTestCaseResult TestCaseResult { get; set; }

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        public SandboxResponseStatus Status { get; set; }

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