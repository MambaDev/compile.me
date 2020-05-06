namespace Compile.Me.Shared.Types
{
    public class SandboxStatus
    {
        /// <summary>
        ///  The current sandbox containers status (the container running the code).
        /// </summary>
        public ContainerStatus ContainerStatus { get; set; } = ContainerStatus.Unknown;

        /// <summary>
        /// Has the sandbox environment exceeded its timeout limit.
        /// </summary>
        public bool ExceededTimeoutLimit { get; set; } = false;

        /// <summary>
        /// Has the sandbox environment exceeded its memory limit.
        /// </summary>
        public bool ExceededMemoryLimit { get; set; } = false;
    }
}