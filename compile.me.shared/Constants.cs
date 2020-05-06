﻿using System.Collections.Generic;
using Compile.Me.Shared.Modals;

namespace compile.me.shared
{
    public static class Constants
    {
        /// <summary>
        /// The list of supported compilers and there related properties.
        /// </summary>
        public static readonly List<Compiler> Compilers = new List<Compiler>()
        {
            new Compiler("python", "python", true, string.Empty, "virtual_machine_python", "standard.out",
                "error.out"),

            new Compiler("javascript", "node", true, string.Empty, "virtual_machine_node", "standard.out",
                "error.out"),
        };
    }
}