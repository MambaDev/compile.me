using System;
using compile.me.api.Types.Requests;

namespace compile.me.api.Types.Events
{
    public class CompileSingleTestSandboxCompletionEventArgs
    {
        /// <summary>
        /// The id of the request
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the container that completed.
        /// </summary>
        public string ContainerId { get; set; }

        /// <summary>
        /// The response of the completion process.
        /// </summary>
        public CompileTestSourceResponse Response { get; set; }
    }
}