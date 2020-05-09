using System;
using System.Collections.Generic;
using Compile.Me.Shared.Types;

namespace Compile.Me.Worker.Service.Types.MultipleTests
{
    public class SandboxMultipleTestCreationRequest : SandboxCreationRequestBase
    {
        /// <summary>
        /// The range of test cases being used.
        /// </summary>
        public IReadOnlyList<CompilerTestCase> TestCases { get; }

        /// <summary>
        /// Creates a new instance of the sandbox creation request.
        /// </summary>
        /// <param name="id">The id of the request</param>
        /// <param name="timeoutSeconds">The max timeout of the sandbox</param>
        /// <param name="memoryConstraint">The memory constraint limit (mb) of the container.</param>
        /// <param name="path">The path used for the output and input.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <param name="compiler">The compiler being used.</param>
        /// <param name="testcases">The test cases being used.</param>
        public SandboxMultipleTestCreationRequest(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, Compiler compiler, IReadOnlyList<CompilerTestCase> testcases) : base(id,
            timeoutSeconds, memoryConstraint, path,
            sourceCode, compiler)
        {
            this.TestCases = testcases;
        }
    }
}