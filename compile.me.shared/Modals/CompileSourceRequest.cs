using System;

namespace Compile.Me.Shared.Modals
{
    public class CompileSourceRequest
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
        ///  The name of the compiler being used.
        /// </summary>
        public string Compiler { get; set; }
    }
}