﻿using HlangInterpreter.Statements;

namespace HlangInterpreter.HelperInterfaces
{
    /// <summary>
    /// Interface used to implement visitor pattern
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStatementVisitor<T>
    {
        T VisitPrintStatement(Print statement);
        T VisitExpressionStatement(Expression statement);
        T VisitBlockStatement(Block statement);
        T VisitIfStatement(If statement);
        T VisitWhileStatement(While statement);
        T VisitExportStatement(Export export);
        T VisitForeachStatement(ForEach statement);
        T VisitImportStatement(Import statement);
        T visitFunctionStatement(Function statement);
        T VisitReturnStatement(Return statement);
        T VisitBreakStatement(Break statement);
        T VisitClassSTatement(Class statement);
    }
}
