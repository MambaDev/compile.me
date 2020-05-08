﻿using System;
using System.Collections.Generic;
using Compile.Me.Shared.Types;
using Newtonsoft.Json;

namespace compile.me.shared.Requests.TestSourceCompile
{
    public class CompileTestSourceRequest : CompileRequestBase
    {
        /// <summary>
        /// The single test case for the compile request.
        /// </summary>
        [JsonProperty("test_case")]
        public CompilerTestCase TestCase { get; set; }

        /// <summary>
        /// Creates a new instance of the single compile tests source request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="timeoutSeconds">The total number of time out seconds for the container.</param>
        /// <param name="memoryConstraint">The memory constraint in mb.</param>
        /// <param name="sourceCode">The source code being used.</param>
        /// <param name="compilerName">The name of the compiler being used.</param>
        /// <param name="testCase">The test case used in the request.</param>
        public CompileTestSourceRequest(Guid id, uint timeoutSeconds, uint memoryConstraint,
            IReadOnlyList<string> sourceCode, string compilerName, CompilerTestCase testCase) : base(id,
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