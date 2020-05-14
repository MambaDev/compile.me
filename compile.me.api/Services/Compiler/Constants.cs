using System.Collections.Generic;

namespace compile.me.api.Services.Compiler
{
    public static class Constants
    {
        /// <summary>
        /// The list of supported compilers and there related properties.
        /// </summary>
        public static readonly List<Types.Compiler> Compilers = new List<Types.Compiler>()
        {
            new Types.Compiler("python", "python", true, string.Empty, "virtual_machine_python", "standard.out", "error.out"),
            new Types.Compiler("javascript", "node", true, string.Empty, "virtual_machine_node", "standard.out", "error.out"),
        };
    }
}