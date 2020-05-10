using System.Collections.Generic;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.SingleTest;
using Newtonsoft.Json;

namespace Compile.Me.Worker.Service.Types.MultipleTests
{
    public class SandboxMultipleTestCreationResponse
    {
        /// <summary>
        /// The result of the multiple sandboxes.
        /// </summary>
        public CompilerResult CompilerResult { get; }

        /// <summary>
        /// The given status of the multiple sandboxes, if its run all, then if one fails it will be marked as
        /// a fail, otherwise it will be marked as the failing sandboxes reason.
        /// </summary>
        public SandboxResponseStatus SandboxStatus { get; }


        /// <summary>
        /// The resulting test case of the execution.
        /// </summary>
        public IReadOnlyList<SandboxSingleTestResponse> TestCases { get; }

        /// <summary>
        /// Creates a ne instance of the sandbox compile response. 
        /// </summary>
        /// <param name="result">The complete result.</param>
        /// <param name="status">The status of the execution.</param>
        /// <param name="testCases">The result of the sandboxes and test case.</param>
        public SandboxMultipleTestCreationResponse(CompilerResult result, SandboxResponseStatus status,
            IReadOnlyList<SandboxSingleTestResponse> testCases)
        {
            this.CompilerResult = result;
            this.SandboxStatus = status;
            this.TestCases = testCases;
        }
    }
}