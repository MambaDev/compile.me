﻿namespace Compile.Me.Shared.Types
{
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
        Failed = 2
    }
}