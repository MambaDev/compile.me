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

        public SandboxSingleTestCreationRequest(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, CompilerTestCase testCase, Compiler compiler) : base(id, timeoutSeconds,
            memoryConstraint, path, sourceCode, testCase.StandardDataIn, compiler)
        {
            this.TestCase = testCase;
        }

        public SandboxSingleTestCreationRequest()
        {
        }
    }
}