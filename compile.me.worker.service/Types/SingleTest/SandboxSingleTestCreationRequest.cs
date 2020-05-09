using System;
using System.Collections.Generic;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.Compile;

namespace Compile.Me.Worker.Service.Types.SingleTest
{
    public class SandboxSingleTestCreationRequest : SandboxCompileCreationRequest
    {
        /// <summary>
        /// The test case used to validate the compile results too.
        /// </summary>
        public CompilerTestCase TestCase { get; set; }

        /// <summary>
        /// Creates a new instance of the sandbox compile creation request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="timeoutSeconds">The total number of time out seconds for the container.</param>
        /// <param name="memoryConstraint">The memory constraint in mb.</param>
        /// <param name="path">The path the files will be written too.</param>
        /// <param name="sourceCode">The source code being used.</param>
        /// <param name="compiler">The compiler being used for the sandbox.</param>
        /// <param name="testCase">The single test case used for the execution.</param>
        public SandboxSingleTestCreationRequest(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, Compiler compiler, CompilerTestCase testCase) : base(id, timeoutSeconds,
            memoryConstraint, path, sourceCode, testCase.StandardInputIn, compiler)
        {
            this.TestCase = testCase;
        }
    }
}