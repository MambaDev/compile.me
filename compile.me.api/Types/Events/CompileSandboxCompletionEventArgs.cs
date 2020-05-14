using System;
using compile.me.api.Types.Requests;

namespace compile.me.api.Types.Events
{
    public class CompileSandboxCompletionEventArgs
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
        public CompileSourceResponse Response { get; set; }
    }
}