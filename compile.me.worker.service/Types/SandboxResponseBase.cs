using Compile.Me.Shared.Types;

namespace Compile.Me.Worker.Service.Types
{
    public class SandboxResponseBase
    {
        /// <summary>
        /// The result of the sandbox.
        /// </summary>
        public CompilerResult CompilerResult { get; set; }

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        public SandboxResponseStatus SandboxStatus { get; set; }

        /// <summary>
        /// Creates a new instance of the sandbox response base.
        /// </summary>
        /// <param name="compilerResult">The result of the compiling and execution process.</param>
        /// <param name="sandboxStatus">The final status of the container.</param>
        public SandboxResponseBase(CompilerResult compilerResult, SandboxResponseStatus sandboxStatus)
        {
            this.CompilerResult = compilerResult;
            this.SandboxStatus = sandboxStatus;
        }
    }
}