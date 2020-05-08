using System;
using System.Collections.Generic;
using Compile.Me.Shared.Modals;
using Compile.Me.Shared.Types;

namespace compile.me.shared.Requests.TestSourceCompile
{
    public class CompileTestSourceRequest : CompileRequestBase
    {
        /// <summary>
        /// The single test case for the compile requets.
        /// </summary>
        public CompilerTestCase TestCase { get; set; }

        public CompileTestSourceRequest(Guid id, uint timeoutSeconds, uint memoryConstraint,
            IReadOnlyList<string> sourceCode, CompilerTestCase testCase, string compilerName) : base(id,
            CompileRequestType.SingleTest, timeoutSeconds, memoryConstraint,
            sourceCode, compilerName)
        {
            this.TestCase = testCase;
        }

        public CompileTestSourceRequest()
        {
        }
    }
}