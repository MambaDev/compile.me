namespace Compile.Me.Shared.Types
{
    public enum ContainerStatus
    {
        /// <summary>
        /// The sandbox is currently in a unknown state.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The box has just been created but has not been started yet.
        /// </summary>
        Created = 1,

        /// <summary>
        /// The sandbox has been started and will be begin executing the code.
        /// </summary>
        Started = 2,

        /// <summary>
        /// The docker container is being killed and thus is currently shutting down.
        /// </summary>
        Killing = 3,

        /// <summary>
        /// The docker container has been killed.
        /// </summary>
        Killed = 4,

        /// <summary>
        /// The container has been removed and thus ending its life cycle.
        /// </summary>
        Removed = 5
    }
}