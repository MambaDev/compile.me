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
        /// No test cases are being used for this entry.
        /// </summary>
        NoTest = 3
    }
}