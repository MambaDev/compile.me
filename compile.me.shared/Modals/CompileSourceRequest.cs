using System;
using Compile.Me.Shared.Types;
using Newtonsoft.Json.Linq;

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
        /// The type of the request that is being performed (compile, test, multiple tests, etc).
        /// </summary>
        public CompileRequestType Type { get; set; }

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
        public string[] SourceCode { get; set; }

        /// <summary>
        ///  The name of the compiler being used.
        /// </summary>
        public string CompilerName { get; set; }

        /// <summary>
        ///  The related content for the given request type, this will be based on the given type
        /// and will be casted before use. Ensure this is the correct content for the correct
        /// type otherwise the request will fail.
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Creates a new instance of the compile source code request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="type">The type of the request.</param>
        /// <param name="timeoutSeconds">The timeout of the request in seconds.</param>
        /// <param name="memoryConstraint">The memory constraint in mb.</param>
        /// <param name="sourceCode">The source code of the request.</param>
        /// <param name="compilerName">The compilers name.</param>
        /// <param name="content">The additional request content related to the type.</param>
        public CompileSourceRequest(Guid id, CompileRequestType type, int timeoutSeconds, long memoryConstraint,
            string[] sourceCode, string compilerName, object content)
        {
            this.Id = id;
            this.Type = type;
            this.TimeoutSeconds = timeoutSeconds;
            this.MemoryConstraint = memoryConstraint;
            this.SourceCode = sourceCode;
            this.CompilerName = compilerName;
            this.Content = content;
        }

        /// <summary>
        /// Creates a new instance of the compile source code request.
        /// </summary>       
        public CompileSourceRequest()
        {
        }
    }
}