using System;
using System.Collections.Generic;
using Compile.Me.Shared.Types;

namespace compile.me.shared.Requests
{
    public class CompileRequestBase
    {
        /// <summary>
        /// The id of the compiler request, this will be used when updating / sending the data back
        /// into the queue.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of the request that is being performed (compile, test, multiple tests, etc).
        /// </summary>
        public CompileRequestType Type { get; set; }

        /// <summary>
        /// The max amount of timeout for the given executed code, if the code docker container is running
        /// for longer than the given timeout then the code is rejected. This is used to ensure that the
        /// source code is not running for longer than required.
        /// </summary>
        public uint TimeoutSeconds { get; set; } = 2;

        /// <summary>
        /// The upper limit of the max amount of memory that the given execution can perform. By default, the upper
        /// limit of the amount of mb the given execution can run with.
        /// </summary>
        public uint MemoryConstraint { get; set; } = 128;

        /// <summary>
        /// The source code that will be executed, this is the code that will be written to the path and
        /// mounted to the docker container.
        /// </summary>
        public IReadOnlyList<string> SourceCode { get; set; }

        /// <summary>
        ///  The name of the compiler being used.
        /// </summary>
        public string CompilerName { get; set; }

        public CompileRequestBase(Guid id, CompileRequestType type, uint timeoutSeconds, uint memoryConstraint,
            IReadOnlyList<string> sourceCode, string compilerName)
        {
            this.Id = id;
            this.Type = type;
            this.TimeoutSeconds = timeoutSeconds;
            this.MemoryConstraint = memoryConstraint;
            this.SourceCode = sourceCode;
            this.CompilerName = compilerName;
        }

        public CompileRequestBase()
        {
        }
    }
}