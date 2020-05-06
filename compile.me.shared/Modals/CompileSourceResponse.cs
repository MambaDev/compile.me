using System;
using Compile.Me.Shared.Types;

namespace Compile.Me.Shared.Modals
{
    public class CompileSourceResponse
    {
        /// <summary>
        /// The id of the compiler request, this will be used when updating / sending the data back
        /// into the queue.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The raw output that was produced by the sandbox.
        /// </summary>
        public string StandardOutput { get; set; }

        /// <summary>
        /// The raw error output that was produced by the sandbox.
        /// </summary>
        public string StandardErrorOutput { get; set; }

        /// <summary>
        /// The result of the sandbox.
        /// </summary>
        public SandboxResponseResult Result { get; set; } = SandboxResponseResult.Unknown;

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        public SandboxResponseStatus Status { get; set; } = SandboxResponseStatus.Unknown;

        /// <summary>
        /// Creates a new instance of the compile source response.
        /// </summary>
        /// <param name="id">The id of the request (allowing the match up).</param>
        /// <param name="standardOutput">The standard output result.</param>
        /// <param name="standardErrorOutput">The standard error result.</param>
        /// <param name="result">The resulting result.</param>
        /// <param name="status">The resulting status.</param>
        public CompileSourceResponse(Guid id, string standardOutput, string standardErrorOutput,
            SandboxResponseResult result, SandboxResponseStatus status)
        {
            this.Id = id;
            this.StandardOutput = standardOutput;
            this.StandardErrorOutput = standardErrorOutput;
            this.Result = result;
            this.Status = status;
        }

        /// <summary>
        /// Creates a new empty instance of the CompileSourceResponse.
        /// </summary>
        public CompileSourceResponse()
        {
        }
    }
}