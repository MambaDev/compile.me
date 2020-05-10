using Newtonsoft.Json;

namespace Compile.Me.Shared.Types
{
    public class SandboxStatus
    {
        private readonly object _lock = new object();

        private ContainerStatus _containerStatus = ContainerStatus.Unknown;

        /// <summary>
        ///  The current sandbox containers status (the container running the code).
        /// </summary>
        public ContainerStatus ContainerStatus
        {
            get

            {
                lock (this._lock)
                {
                    return this._containerStatus;
                }
            }
            set
            {
                lock (this._lock)
                {
                    this._containerStatus = value;
                }
            }
        }

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