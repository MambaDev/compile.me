namespace Compile.Me.Shared.Modals
{
    public class SandboxRequest
    {
        /// The max amount of timeout for the given executed code, if the code docker container is running
        /// for longer than the given timeout then the code is rejected. This is used to ensure that the
        /// source code is not running for longer than required.
        public int TimeoutSeconds { get; set; } = 2;

        /// <summary>
        /// The upper limit of the max amount of memory that the given execution can perform. By default, the upper
        /// limit of the amount of mb the given execution can run with.
        /// </summary>
        public long MemoryConstraint { get; set; } = 128;

        /// The given path that would be mounted and shared with the given docker container. This is where
        /// the container will be reading the source code from and writing the response too. Once this has
        /// been completed, this is the path to files that will be cleaned up.
        public string Path { get; set; }

        /// The source code that will be executed, this is the code that will be written to the path and
        /// mounted to the docker container.
        public string SourceCode { get; set; }

        /// The standard input data that will be used with the given code file. This can be used for when
        /// projects require that a given code input should  be executing after reading input. e.g taking
        /// in a input and performing actions on it.
        public string StdinData { get; set; }

        /// The reference details of the compiler that will be running the code. Including details of the
        /// language, compiler name (or interrupter) and the name of the given output file.
        public Compiler Compiler { get; set; }
    }
}