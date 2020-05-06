using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Compile.Me.Shared.Modals
{
    public class Compiler
    {
        /// <summary>
        /// The language that the given compiler is going to be using or not. This is the can be seen
        /// as the kind of code that is going to be executed by the requesting machine. e.g Python, Node,
        /// JavaScript, C++.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The name of the compiler that will be used to run the code. This is the name of the file that
        /// will be called from the root of the docker container. e.g node, py, python3
        /// </summary>
        public string CompilerEntry { get; set; }

        /// <summary>
        /// If the given compiler is a interpreter or not, since based on this action we would need to create
        /// additional steps for compiling to a file if not.
        /// </summary>
        public bool Interpreter { get; set; } = false;

        /// <summary>
        /// The additional arguments that might be required for performing compiling actions.
        /// For example letting a compiler to know that they need to build first.
        /// </summary>
        public string AdditionalArguments { get; set; } = "";

        /// <summary>
        /// This is the name of docker image that will be executed for the given code sample, this will be the
        /// container that will be used for just this language. Most likely virtual_machine_language, e.g
        /// virtual_machine_python.
        /// </summary>
        public string VirtualMachineName { get; set; }

        /// <summary>
        ///  The file in which the given compiler will be writing too (standard output), since this file will
        /// be read when the response returned back to the user.
        /// </summary>
        public string StandardOutputFile { get; set; } = "standard.out";

        /// <summary>
        ///  The file in which the given compiler will be writing too (error output), since this file will be
        /// read when the response returned back to the user.
        /// </summary>
        public string StandardErrorFile { get; set; } = "error.out";

        /// <summary>
        /// Creates a new instance of the compiler.
        /// </summary>
        /// <param name="language">The language of the compiler</param>
        /// <param name="compilerEntry">The compiler entry point.</param>
        /// <param name="interpreter">If its a interpreter or not.</param>
        /// <param name="additionalArguments">A string of additional arguments if required.</param>
        /// <param name="virtualMachineName">The name of the virtual machine of the compiler.</param>
        /// <param name="outputFile">The output file path of the compiler.</param>
        /// <param name="outputErrorFile">The output file path of the compiler error.</param>
        public Compiler(string language, string compilerEntry, bool interpreter, string additionalArguments,
            string virtualMachineName, string outputFile, string
                outputErrorFile)
        {
            this.Language = language;
            this.CompilerEntry = compilerEntry;
            this.Interpreter = interpreter;
            this.AdditionalArguments = additionalArguments;
            this.VirtualMachineName = virtualMachineName;
            this.StandardOutputFile = outputFile;
            this.StandardErrorFile = outputErrorFile;
        }

        /// <summary>
        ///  Creates a new instance of the compiler.
        /// </summary>
        public Compiler()
        {
        }
    }
}