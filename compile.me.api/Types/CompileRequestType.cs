namespace compile.me.api.Types
{
    public enum CompileRequestType
    {
        /// <summary>
        /// The request is just a single compile, containing no additional data.
        /// </summary>
        Compile = 1,

        /// <summary>
        /// A single test request, it performs a standard compile, comparing the results to
        /// the provided expected results. Marking the resulting status.
        /// </summary>
        SingleTest = 2,

        /// <summary>
        /// A multiple test run, executing the code against a range of tests. Comparing the
        /// output to the provided results, and then repeating the process again for the following
        /// tests. If any fail, the following are not executed.
        /// </summary>
        MultipleTests = 3
    }
}