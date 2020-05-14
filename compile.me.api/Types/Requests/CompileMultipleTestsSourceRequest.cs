using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace compile.me.api.Types.Requests
{
    public class CompileMultipleTestsSourceRequest : CompileRequestBase
    {
        /// <summary>
        /// The single test case for the compile request.
        /// </summary>
        [JsonProperty("test_cases")]
        public IReadOnlyList<CompilerTestCase> TestCases { get; set; }

        /// <summary>
        /// If all tests should be ran regardless of pass state.
        /// </summary>
        [JsonProperty("run_all")]
        public bool RunAll { get; set; }

        /// <summary>
        /// If all tests should be executed at the same time regardless if one fails or not.
        /// </summary>
        public bool RunAllParallel { get; set; }

        /// <summary>
        /// Creates a new instance of the multiple compile tests source request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="timeoutSeconds">The total number of time out seconds for the container.</param>
        /// <param name="memoryConstraint">The memory constraint in mb.</param>
        /// <param name="sourceCode">The source code being used.</param>
        /// <param name="compilerName">The name of the compiler being used.</param>
        /// <param name="testCases">The single test case used for the execution.</param>
        /// <param name="runAll">If all tests should be ran regardless of pass state.</param>
        /// <param name="runAllParallel">If all tests should be ran in parallel.</param>
        public CompileMultipleTestsSourceRequest(Guid id, uint timeoutSeconds, uint memoryConstraint,
            IReadOnlyList<string> sourceCode, string compilerName, IReadOnlyList<CompilerTestCase> testCases,
            bool runAll, bool runAllParallel) :
            base(id, CompileRequestType.MultipleTests, timeoutSeconds, memoryConstraint, sourceCode, compilerName)
        {
            this.TestCases = testCases;
            this.RunAll = runAll;
            this.RunAllParallel = runAllParallel;
        }

        public CompileMultipleTestsSourceRequest()
        {
        }
    }
}