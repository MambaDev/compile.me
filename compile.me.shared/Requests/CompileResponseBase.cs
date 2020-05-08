﻿using System;
using Compile.Me.Shared.Types;

namespace Compile.Me.Shared.Modals
{
    public class CompileResponseBase
    {
        /// <summary>
        /// The id of the compiler request, this will be used when updating / sending the data back
        /// into the queue.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The result of the sandbox.
        /// </summary>
        public CompilerResult Result { get; set; }

        /// <summary>
        /// Creates a new instance of the compiler base response.
        /// </summary>
        /// <param name="id">the id of the request (used to match up).</param>
        /// <param name="result">The result of the compile.</param>
        public CompileResponseBase(Guid id, CompilerResult result = CompilerResult.Unknown)
        {
            this.Id = id;
            this.Result = result;
        }

        public CompileResponseBase()
        {
        }
    }
}