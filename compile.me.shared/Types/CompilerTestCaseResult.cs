using System;
using System.Collections.Generic;

namespace Compile.Me.Shared.Types
{
    public enum CompilerTestResult
    {
        /// <summary>
        /// The state of the test case is unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The state of the test case is that it passed.
        /// </summary>
        Passed = 1,

        /// <summary>
        /// The state of the test case is that it failed.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// The test case was never ran, probably because of a fault or previous test cases
        /// did not pass to get to this test.
        /// </summary>
        NotRan = 3
    }

    public class CompilerTestCaseResult
    {
        /// <summary>
        /// The id of the test.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The result of the test.
        /// </summary>
        public CompilerTestResult Result { get; set; }

        /// <summary>
        /// The raw output that was produced by the sandbox.
        /// </summary>
        public IReadOnlyList<string> StandardOutput { get; set; }

        /// <summary>
        /// The raw error output that was produced by the sandbox.
        /// </summary>
        public IReadOnlyList<string> StandardErrorOutput { get; set; }

        /// <summary>
        /// Creates a new instance of the compile test case result.
        /// </summary>
        /// <param name="id">The id of the test that was ran.</param>
        /// <param name="result">The result of the test.</param>
        /// <param name="standardOutput">The standard output of the test case.</param>
        /// <param name="standardErrorOutput">The error output of the test case.</param>
        public CompilerTestCaseResult(Guid id, CompilerTestResult result, IReadOnlyList<string> standardOutput,
            IReadOnlyList<string> standardErrorOutput)
        {
            this.Id = id;
            this.Result = result;
            this.StandardOutput = standardOutput;
            this.StandardErrorOutput = standardErrorOutput;
        }

        public CompilerTestCaseResult()
        {
        }
    }
}