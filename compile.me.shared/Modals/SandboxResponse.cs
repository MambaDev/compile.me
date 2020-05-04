namespace compile.me.shared.Modals
{
    /// <summary>
    /// The current status of the sandbox request.
    /// </summary>
    public enum SandboxResponseStatus
    {
        /// <summary>
        /// In pending state, e.g it as yet to begin executing.
        /// </summary>
        Pending = 0x1,

        /// <summary>
        /// In running state, the sandbox is currently running.
        /// </summary>
        Running = 0x2,

        /// <summary>
        /// The sandbox exceeded its memory constraint limitations.
        /// </summary>
        MemoryConstraintExceeded = 0x4,

        /// <summary>
        /// The sandbox exceeded its time limit constraints.
        /// </summary>
        TimeLimitExceeded = 0x8,
    }

    public enum SandboxResponseResult
    {
        /// <summary>
        /// The sandbox execution failed for any number of reasons, check the status.
        /// </summary>
        Failed = 0x2,

        /// <summary>
        /// The sandbox execution completed successfully.
        /// </summary>
        Completed = 0x4,
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
        public SandboxResponseResult Result { get; set; }

        /// <summary>
        /// The given status of the sandbox.
        /// </summary>
        public SandboxResponseStatus Status { get; set; }
    }
}