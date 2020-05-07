namespace Compile.Me.Shared.Modals
{
    public class CompileSourceTestBody
    {
        /// <summary>
        /// The standard input data that will be used with the given code file. This can be used for when
        /// projects require that a given code input should  be executing after reading input. e.g taking
        /// in a input and performing actions on it.
        /// </summary>
        public string[] StdinData { get; set; } = new string[] { };

        /// <summary>
        /// The tests that will be used to validate against the output as long as no error occurred during
        /// the execution of the source code. 
        /// </summary>
        public string[] Tests { get; set; }

        /// <summary>
        /// Creates a new instance of the compile source body.
        /// 
        /// Creates the body for the standard compile request type. Since a standard compile request does not
        /// perform any additional testing process, only the standard in data is required (if needed).
        /// </summary>
        /// <param name="stdinData">The standard data input for just compiling.</param>
        public CompileSourceTestBody(string[] stdinData, string[] tests)
        {
            this.StdinData = stdinData;
            this.Tests = tests;
        }

        /// <summary>
        /// Creates a new instance of the compile source body.
        /// </summary>
        public CompileSourceTestBody()
        {
        }
    }
}