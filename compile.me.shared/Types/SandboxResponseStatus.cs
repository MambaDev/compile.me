namespace Compile.Me.Shared.Types
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
        /// In a finished state, the sandbox is no longer running for any reason.
        /// </summary>
        Finished = 1,

        /// <summary>
        /// In running state, the sandbox is currently running.
        /// </summary>
        Running = 2,

        /// <summary>
        /// In pending state, e.g it as yet to begin executing.
        /// </summary>
        Pending = 3,

        /// <summary>
        /// The sandbox exceeded its memory constraint limitations.
        /// </summary>
        MemoryConstraintExceeded = 4,

        /// <summary>
        /// The sandbox exceeded its time limit constraints.
        /// </summary>
        TimeLimitExceeded = 5,

        /// <summary>
        /// The sandbox related test failed to pass.
        /// </summary>
        TestFailed = 6
    }
}