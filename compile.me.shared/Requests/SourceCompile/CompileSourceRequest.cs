using System;
using System.Collections.Generic;
using Compile.Me.Shared.Modals;
using compile.me.shared.Requests;
using Compile.Me.Shared.Types;

namespace compile.me.shared.Modals.SourceCompile
{
    public class CompileSourceRequest : CompileRequestBase
    {
        /// <summary>
        /// The standard input data for the single compile request.
        /// </summary>
        public IReadOnlyList<string> StandardInputData { get; set; }

        public CompileSourceRequest(Guid id, uint timeoutSeconds, uint memoryConstraint,
            IReadOnlyList<string> sourceCode, IReadOnlyList<string> standardInputData, string compilerName) : base(id,
            CompileRequestType.Compile, timeoutSeconds, memoryConstraint,
            sourceCode, compilerName)
        {
            this.StandardInputData = standardInputData;
        }

        public CompileSourceRequest()
        {
        }
    }
}