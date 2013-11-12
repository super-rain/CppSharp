﻿using System.Linq;
using CppSharp.AST;

namespace CppSharp.Passes
{
    public class MoveFunctionToClassPass : TranslationUnitPass
    {
        public override bool VisitFunctionDecl(Function function)
        {
            if (!AlreadyVisited(function) && !function.Ignore && !(function.Namespace is Class)
                // HACK: there are bugs with operators generated by Q_DECLARE_OPERATORS_FOR_FLAGS, an incorrect argument type, to say the least
                && !function.IsOperator)
            {
                TranslationUnit unit = function.Namespace as TranslationUnit;
                Class @class;
                if (unit != null)
                {
                    @class = Driver.ASTContext.FindCompleteClass(
                        unit.FileNameWithoutExtension.ToLowerInvariant(), true);
                    if (@class != null)
                    {
                        MoveFunction(function, @class);
                        return base.VisitFunctionDecl(function);
                    }
                }
                @class = Driver.ASTContext.FindClass(
                    function.Namespace.Name, ignoreCase: true).FirstOrDefault();
                if (@class != null)
                {
                    MoveFunction(function, @class);
                }
            }
            return base.VisitFunctionDecl(function);
        }

        private static void MoveFunction(Function function, Class @class)
        {
            var method = new Method(function)
            {
                Namespace = @class,
                IsStatic = true
            };

            function.ExplicityIgnored = true;

            @class.Methods.Add(method);
        }
    }
}
