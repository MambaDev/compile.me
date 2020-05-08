using System;
using System.Collections.Generic;
using Compile.Me.Shared.Types;

namespace Compile.Me.Worker.Service
{
    public class SandboxCreationRequestBase
    {
        /// <summary>
        /// The id of the compiler request, this will be used when updating / sending the data back
        /// into the queue.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The max amount of timeout for the given executed code, if the code docker container is running
        /// for longer than the given timeout then the code is rejected. This is used to ensure that the
        /// source code is not running for longer than required.
        /// </summary>
        public uint TimeoutSeconds { get; } = 2;

        /// <summary>
        /// The upper limit of the max amount of memory that the given execution can perform. By default, the upper
        /// limit of the amount of mb the given execution can run with.
        /// </summary>
        public uint MemoryConstraint { get; } = 128;

        /// <summary>
        /// The given path that would be mounted and shared with the given docker container. This is where
        /// the container will be reading the source code from and writing the response too. Once this has
        /// been completed, this is the path to files that will be cleaned up.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The source code that will be executed, this is the code that will be written to the path and
        /// mounted to the docker container.
        /// </summary>
        public IReadOnlyList<string> SourceCode { get; }

        /// <summary>
        /// The reference details of the compiler that will be running the code. Including details of the
        /// language, compiler name (or interrupter) and the name of the given output file.
        /// </summary>
        public Compiler Compiler { get; }

        /// <summary>
        /// Creates a new instance of the sandbox creation request.
        /// </summary>
        /// <param name="id">The id of the request</param>
        /// <param name="timeoutSeconds">The max timeout of the sandbox</param>
        /// <param name="memoryConstraint">The memory constraint limit (mb) of the container.</param>
        /// <param name="path">The path used for the output and input.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <param name="compiler">The compiler being used.</param>
        public SandboxCreationRequestBase(Guid id, uint timeoutSeconds, uint memoryConstraint, string path,
            IReadOnlyList<string> sourceCode, Compiler compiler)
        {
            this.Id = id;
            this.TimeoutSeconds = timeoutSeconds;
            this.MemoryConstraint = memoryConstraint;
            this.Path = path;
            this.SourceCode = sourceCode;
            this.Compiler = compiler;
        }

        public SandboxCreationRequestBase()
        {
        }
    }
}