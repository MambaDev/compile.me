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

        /// <summary>
        /// Creates a new instance of the sandbox compile creation request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="timeoutSeconds">The total number of time out seconds for the container.</param>
        /// <param name="memoryConstraint">The memory constraint in mb.</param>
        /// <param name="path">The path the files will be written too.</param>
        /// <param name="sourceCode">The source code being used.</param>
        /// <param name="stdinData">The standard in for the sandbox.</param>
        /// <param name="compiler">The compiler being used for the sandbox.</param>
        public SandboxCompileCreationRequest(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, IReadOnlyList<string> stdinData, Compiler compiler) : base(id,
            timeoutSeconds, memoryConstraint, path, sourceCode, compiler)
        {
            this.StdinData = stdinData;
        }
    }
}