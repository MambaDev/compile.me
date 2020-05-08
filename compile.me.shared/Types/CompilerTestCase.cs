using System;
using System.Collections.Generic;

namespace Compile.Me.Shared.Types
{
    public class CompilerTestCase
    {
        /// <summary>
        /// The id of ethe test.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The standard input for the given test case.
        /// </summary>
        public IReadOnlyList<string> StandardDataIn { get; set; }

        /// <summary>
        /// The expected output for the given test.
        /// </summary>
        public IReadOnlyList<string> ExpectedStandardDataOut { get; set; }

        /// <summary>
        /// Creates a new instance of the compile test case.
        /// </summary>
        /// <param name="id">The id of the test</param>
        /// <param name="standardDataInIn">The standard data input.</param>
        /// <param name="expectedStandardDataOut">The expected output results for the standard data in.</param>
        public CompilerTestCase(Guid id, IReadOnlyList<string> standardDataInIn,
            IReadOnlyList<string> expectedStandardDataOut)
        {
            this.Id = id;
            this.StandardDataIn = standardDataInIn;
            this.ExpectedStandardDataOut = expectedStandardDataOut;
        }

        public CompilerTestCase()
        {
        }
    }
}