using System.Collections.Generic;
using Compile.Me.Shared.Types;

namespace Compile.Me.Worker.Service.Types.Compile
{
    public class SandboxCompileResponse
    {
        /// <summary>
        /// The raw output that was produced by the sandbox.
        /// </summary>
        public IReadOnlyList<string> StandardOutput { get; set; }

        /// <summary>
        /// The raw error output that was produced by the sandbox.
        /// </summary>
        public IReadOnlyList<string> StandardErrorOutput { get; set; }

        /// <summary>
        /// The result of the sandbox.
        /// </summary>
        public CompilerResult Result { get; set; } = CompilerResult.Unknown;

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        public SandboxResponseStatus Status { get; set; } = SandboxResponseStatus.Unknown;

        public SandboxCompileResponse(IReadOnlyList<string> standardOutput, IReadOnlyList<string> standardErrorOutput,
            CompilerResult compilerResult, SandboxResponseStatus sandboxResponseStatus)
        {
            this.StandardOutput = standardOutput;
            this.StandardErrorOutput = standardErrorOutput;
            this.Result = compilerResult;
            this.Status = sandboxResponseStatus;
        }

        public SandboxCompileResponse()
        {
        }
    }
}