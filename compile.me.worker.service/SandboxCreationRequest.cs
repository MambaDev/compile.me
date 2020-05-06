using System;

namespace Compile.Me.Worker.Service
{
    public class SandboxCreationRequest
    {
        /// <summary>
        /// The id of the compiler request, this will be used when updating / sending the data back
        /// into the queue.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The max amount of timeout for the given executed code, if the code docker container is running
        /// for longer than the given timeout then the code is rejected. This is used to ensure that the
        /// source code is not running for longer than required.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 2;

        /// <summary>
        /// The upper limit of the max amount of memory that the given execution can perform. By default, the upper
        /// limit of the amount of mb the given execution can run with.
        /// </summary>
        public long MemoryConstraint { get; set; } = 128;

        /// <summary>
        /// The given path that would be mounted and shared with the given docker container. This is where
        /// the container will be reading the source code from and writing the response too. Once this has
        /// been completed, this is the path to files that will be cleaned up.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The source code that will be executed, this is the code that will be written to the path and
        /// mounted to the docker container.
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// The standard input data that will be used with the given code file. This can be used for when
        /// projects require that a given code input should  be executing after reading input. e.g taking
        /// in a input and performing actions on it.
        /// </summary>
        public string StdinData { get; set; }

        /// <summary>
        /// The reference details of the compiler that will be running the code. Including details of the
        /// language, compiler name (or interrupter) and the name of the given output file.
        /// </summary>
        public Compiler Compiler { get; set; }

        /// <summary>
        /// Creates a new instance of the sandbox creation request.
        /// </summary>
        /// <param name="id">The id of the request</param>
        /// <param name="timeoutSeconds">The max timeout of the sandbox</param>
        /// <param name="memoryConstraint">The memory constraint limit (mb) of the container.</param>
        /// <param name="path">The path used for the output and input.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <param name="stdinData">The source input.</param>
        /// <param name="compiler">The compiler being used.</param>
        public SandboxCreationRequest(Guid id, int timeoutSeconds, long memoryConstraint, string path,
            string sourceCode, string stdinData, Compiler compiler)
        {
            this.Id = id;
            this.TimeoutSeconds = timeoutSeconds;
            this.MemoryConstraint = memoryConstraint;
            this.SourceCode = sourceCode;
            this.StdinData = stdinData;
            this.Compiler = compiler;
        }

        public SandboxCreationRequest()
        {
        }
    }
}