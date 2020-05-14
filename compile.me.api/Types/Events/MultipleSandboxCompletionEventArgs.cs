using System;
using compile.me.api.Types.Requests;

namespace compile.me.api.Types.Events
{
    public class MultipleSandboxCompletionEventArgs
    {
        /// <summary>
        /// The id of the request
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The response of the completion process.
        /// </summary>
        public CompileMultipleTestsSourceResponse Response { get; set; }
    }
}