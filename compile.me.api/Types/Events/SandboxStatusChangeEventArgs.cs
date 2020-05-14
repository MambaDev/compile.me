namespace compile.me.api.Types.Events
{
    public class SandboxStatusChangeEventArgs
    {
        /// <summary>
        /// The id of the container / sandbox that had the updated status.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The updated status.
        /// </summary>
        public ContainerStatus Status { get; }

        /// <summary>
        /// Creates a new instance of the status update event args for the sandbox.
        /// </summary>
        /// <param name="id">The id of the container.</param>
        /// <param name="status">The updated status.</param>
        public SandboxStatusChangeEventArgs(string id, ContainerStatus status)
        {
            this.Id = id;
            this.Status = status;
        }
    }
}