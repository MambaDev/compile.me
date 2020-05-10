using System;
using Compile.Me.Worker.Service.Types.SingleTest;

namespace Compile.Me.Worker.Service.Events
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
        public SandboxSingleTestResponse Response { get; set; }
    }
}