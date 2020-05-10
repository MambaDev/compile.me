using System;
using System.Collections.Generic;
using compile.me.shared.Requests.MultipleCompileTestsSourceCompile;
using Compile.Me.Shared.Types;
using Compile.Me.Worker.Service.Types.MultipleTests;

namespace Compile.Me.Worker.Service.Events
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
        public SandboxMultipleTestCreationResponse Response { get; set; }

   }
}