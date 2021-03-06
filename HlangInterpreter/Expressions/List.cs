﻿using HlangInterpreter.HelperInterfaces;
using HlangInterpreter.HlangTypes;

namespace HlangInterpreter.Expressions
{
    /// <summary>
    /// Represents a list assignment expression
    /// </summary>
    public class List : Expr
    {
        public HlangList<Expr> Values { get; set; }
        public List(HlangList<Expr> values)
        {
            Values = values;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitListExpr(this);
        }
    }
}
