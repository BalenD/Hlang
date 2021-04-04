﻿using HlangInterpreter.Expressions;
using HlangInterpreter.HelperInterfaces;
using HlangInterpreter.Lib;
using System.Collections.Generic;

namespace HlangInterpreter.Statements
{
    public class Class : Statement
    {
        public List<Function> Methods { get; set; }
        public Token Name { get; set; }
        public Variable ParentClass { get; set; }

        public Class(List<Function> methods, Token name, Variable parentClass)
        {
            Methods = methods;
            Name = name;
            ParentClass = parentClass;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitClassSTatement(this);
        }
    }
}
