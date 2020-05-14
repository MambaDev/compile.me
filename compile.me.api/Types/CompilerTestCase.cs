using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace compile.me.api.Types
{
    public class CompilerTestCase
    {
        /// <summary>
        /// The id of ethe test.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The standard input for the given test case.
        /// </summary>
        [JsonProperty("standard_input_data")]
        public IReadOnlyList<string> StandardInputIn { get; set; }

        /// <summary>
        /// The expected output for the given test.
        /// </summary>
        [JsonProperty("expected_standard_data_out")]
        public IReadOnlyList<string> ExpectedStandardDataOut { get; set; }

        /// <summary>
        /// Creates a new instance of the compile test case.
        /// </summary>
        /// <param name="id">The id of the test</param>
        /// <param name="standardInputInIn">The standard data input.</param>
        /// <param name="expectedStandardDataOut">The expected output results for the standard data in.</param>
        public CompilerTestCase(Guid id, IReadOnlyList<string> standardInputInIn,
            IReadOnlyList<string> expectedStandardDataOut)
        {
            this.Id = id;
            this.StandardInputIn = standardInputInIn;
            this.ExpectedStandardDataOut = expectedStandardDataOut;
        }

        public CompilerTestCase()
        {
        }
    }
}