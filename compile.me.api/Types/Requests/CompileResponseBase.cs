using System;
using Newtonsoft.Json;

namespace compile.me.api.Types.Requests
{
    public class CompileResponseBase
    {
        /// <summary>
        /// The id of the compiler request, this will be used when updating / sending the data back
        /// into the queue.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The type of the response that is being performed (compile, test, multiple tests, etc).
        /// </summary>
        [JsonProperty("type")]
        public CompileRequestType Type { get; set; }

        /// <summary>
        /// The result of the sandbox.
        /// </summary>
        [JsonProperty("result")]
        public CompilerResult Result { get; set; }


        /// <summary>
        /// Creates a new instance of the compiler base response.
        /// </summary>
        /// <param name="id">the id of the request (used to match up).</param>
        /// <param name="result">The result of the compile.</param>
        public CompileResponseBase(Guid id, CompileRequestType type, CompilerResult result = CompilerResult.Unknown)
        {
            this.Id = id;
            this.Type = type;
            this.Result = result;
        }

        public CompileResponseBase()
        {
        }
    }
}