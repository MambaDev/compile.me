using Compile.Me.Shared.Types;

namespace Compile.Me.Worker.Service
{
    public class SandboxResponse
    {
        /// <summary>
        /// The raw output that was produced by the sandbox.
        /// </summary>
        public string[] StandardOutput { get; set; }

        /// <summary>
        /// The raw error output that was produced by the sandbox.
        /// </summary>
        public string[] StandardErrorOutput { get; set; }

        /// <summary>
        /// The related test result for the given compile, only relevant if tests are being used.
        /// </summary>
        public CompilerTestResult TestResult { get; set; } = CompilerTestResult.Unknown;

        /// <summary>
        /// The result of the sandbox.
        /// </summary>
        public SandboxResponseResult Result { get; set; } = SandboxResponseResult.Unknown;

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        public SandboxResponseStatus Status { get; set; } = SandboxResponseStatus.Unknown;
    }
}