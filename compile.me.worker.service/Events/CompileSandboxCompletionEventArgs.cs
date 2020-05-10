using System;
using Compile.Me.Worker.Service.Types.Compile;

namespace Compile.Me.Worker.Service.Events
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
        public SandboxCompileResponse Response { get; set; }
    }
}