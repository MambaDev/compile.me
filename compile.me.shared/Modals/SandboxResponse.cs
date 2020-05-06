namespace Compile.Me.Shared.Modals
{
    /// <summary>
    /// The current status of the sandbox request.
    /// </summary>
    public enum SandboxResponseStatus
    {
        /// <summary>
        /// In a unknown state.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// In pending state, e.g it as yet to begin executing.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// In running state, the sandbox is currently running.
        /// </summary>
        Running = 2,

        /// <summary>
        /// In a finished state, the sandbox is no longer running for any reason.
        /// </summary>
        Finished = 3,

        /// <summary>
        /// The sandbox exceeded its memory constraint limitations.
        /// </summary>
        MemoryConstraintExceeded = 4,

        /// <summary>
        /// The sandbox exceeded its time limit constraints.
        /// </summary>
        TimeLimitExceeded = 5,
    }

    public enum SandboxResponseResult
    {
        /// <summary>
        /// In a unknown state.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The sandbox execution completed successfully.
        /// </summary>
        Succeeded = 1,

        /// <summary>
        /// The sandbox execution failed for any number of reasons, check the status.
        /// </summary>
        Failed = 2,
    }

    public class SandboxResponse
    {
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
    }
}