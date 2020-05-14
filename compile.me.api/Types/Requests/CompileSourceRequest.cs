using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace compile.me.api.Types.Requests
{
    public class CompileSourceRequest : CompileRequestBase
    {
        /// <summary>
        /// The standard input data for the single compile request.
        /// </summary>
        [JsonProperty("standard_input_data")]
        public IReadOnlyList<string> StandardInputData { get; set; }

        /// <summary>
        /// Creates a new instance of the multiple compile tests source request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="timeoutSeconds">The total number of time out seconds for the container.</param>
        /// <param name="memoryConstraint">The memory constraint in mb.</param>
        /// <param name="sourceCode">The source code being used.</param>
        /// <param name="standardInputData">The standard input data used for the compile.</param>
        /// <param name="compilerName">The name of the compiler being used.</param>
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