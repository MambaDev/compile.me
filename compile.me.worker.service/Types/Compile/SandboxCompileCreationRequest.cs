using System;
using System.Collections.Generic;

namespace Compile.Me.Worker.Service.Types.Compile
{
    public class SandboxCompileCreationRequest : SandboxCreationRequestBase
    {
        /// <summary>
        /// The standard input data that will be used with the given code file. This can be used for when
        /// projects require that a given code input should  be executing after reading input. e.g taking
        /// in a input and performing actions on it.
        /// </summary>
        public IReadOnlyList<string> StdinData { get; }

        public SandboxCompileCreationRequest(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, IReadOnlyList<string> stdinData, Compiler compiler) : base(id,
            timeoutSeconds, memoryConstraint, path, sourceCode, compiler)
        {
            this.StdinData = stdinData;
        }

        public SandboxCompileCreationRequest()
        {
        }
    }
}