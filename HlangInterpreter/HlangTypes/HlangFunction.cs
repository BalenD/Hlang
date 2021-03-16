﻿using HlangInterpreter.HelperInterfaces;
using HlangInterpreter.Lib;
using HlangInterpreter.Statements;
using System.Collections.Generic;

namespace HlangInterpreter.HlangTypes
{
    /// <summary>
    /// Represents a function in Hlang
    /// </summary>
    public class HlangFunction : ICallable
    {

        public int ArgumentLength { get; set; }

        private Environment _closure;

        private Function _funcDeclaration;
        public HlangFunction(Function funcDeclaration, Environment closure)
        {
            _closure = closure;
            _funcDeclaration = funcDeclaration;
            ArgumentLength = funcDeclaration.Paramters.Count;
        }

        /// <summary>
        /// Function executed when the fucntion is called in hlang
        /// </summary>
        /// <param name="interpreter">Interpreter to execute code block</param>
        /// <param name="arguments">Arguments passed in</param>
        /// <returns></returns>
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            // Create a new environment and add the arguments to each paramter
            // Pass the closure (outter scope) to the new environment (functiions scope)
            Lib.Environment  env = new Lib.Environment(_closure);
            for (int i = 0; i < _funcDeclaration.Paramters.Count; i++)
            {
                env.Add(_funcDeclaration.Paramters[i].Lexeme, arguments[i]);
            }
            // execute code block and catch any return statements
            try
            {
                interpreter.ExecuteBlock(_funcDeclaration.Body, env);
            }
            catch (HlangReturn value)
            {
                return value.Value;
            }
            return null;
        }

        public override string ToString()
        {
            return $"Hglang function {_funcDeclaration.Name.Lexeme}";
        }
    }
}
