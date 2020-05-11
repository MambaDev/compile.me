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
        /// If all tests should be executed regardless if one failed or not.
        /// </summary>
        public bool RunAll { get; set; }

        /// <summary>
        /// If all tests should be executed at the same time regardless if one fails or not.
        /// </summary>
        public bool RunAllParallel { get; set; }

        /// <summary>
        /// Creates a new instance of the sandbox creation request.
        /// </summary>
        /// <param name="id">The id of the request</param>
        /// <param name="timeoutSeconds">The max timeout of the sandbox</param>
        /// <param name="memoryConstraint">The memory constraint limit (mb) of the container.</param>
        /// <param name="path">The path used for the output and input.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <param name="compiler">The compiler being used.</param>
        /// <param name="testCases">The test cases being used.</param>
        /// <param name="runAll">If all tests should run regardless if any fail or not.</param>
        /// <param name="runAllParallel">If all tests should run in parallel not.</param>
        public SandboxMultipleTestCreationRequest(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, Compiler compiler, IReadOnlyList<CompilerTestCase> testCases, bool runAll,
            bool runAllParallel) : base(id,
            timeoutSeconds, memoryConstraint, path,
            sourceCode, compiler)
        {
            this.TestCases = testCases;
            this.RunAll = runAll;
            this.RunAllParallel = runAllParallel;
        }
    }
}