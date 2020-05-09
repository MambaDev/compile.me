using System.Collections.Generic;
using Compile.Me.Shared.Types;

namespace Compile.Me.Worker.Service.Types.Compile
{
    public class SandboxCompileResponse : SandboxResponseBase
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
        /// Creates a ne instance of the sandbox compile response. 
        /// </summary>
        /// <param name="standardOutput">The standard out.</param>
        /// <param name="standardErrorOutput">The standard error out.</param>
        /// <param name="compilerResult">The result of the compiling.</param>
        /// <param name="sandboxStatus">The status of the sandbox.</param>
        public SandboxCompileResponse(IReadOnlyList<string> standardOutput, IReadOnlyList<string> standardErrorOutput,
            CompilerResult compilerResult, SandboxResponseStatus sandboxStatus) : base(compilerResult, sandboxStatus)
        {
            this.StandardOutput = standardOutput;
            this.StandardErrorOutput = standardErrorOutput;
        }
   }
}