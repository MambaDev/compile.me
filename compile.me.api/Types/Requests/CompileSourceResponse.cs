using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace compile.me.api.Types.Requests
{
    public class CompileSourceResponse : CompileResponseBase
    {
        /// <summary>
        /// The raw output that was produced by the sandbox.
        /// </summary>
        [JsonProperty("standard_output")]
        public IReadOnlyList<string> StandardOutput { get; set; }

        /// <summary>
        /// The raw error output that was produced by the sandbox.
        /// </summary>
        [JsonProperty("standard_error_output")]
        public IReadOnlyList<string> StandardErrorOutput { get; set; }

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        [JsonProperty("status")]
        public SandboxResponseStatus Status { get; set; }

        /// <inheritdoc cref="CompileResponseBase"/>
        /// <summary>
        /// Creates a new instance of the compile source response.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <param name="standardOutput">The output of the compile.</param>
        /// <param name="standardErrorOutput">The error out of the compile.</param>
        /// <param name="status">The status of the sandbox after finishing.</param>
        public CompileSourceResponse(Guid id, IReadOnlyList<string> standardOutput,
            IReadOnlyList<string> standardErrorOutput, CompilerResult result, SandboxResponseStatus status) : base(id,
            CompileRequestType.Compile, result)
        {
            this.StandardOutput = standardOutput;
            this.StandardErrorOutput = standardErrorOutput;
            this.Status = status;
        }

        public CompileSourceResponse()
        {
        }
    }
}