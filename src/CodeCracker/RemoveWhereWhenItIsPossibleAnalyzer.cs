﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCracker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RemoveWhereWhenItIsPossibleAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CodeCracker.RemoveWhereWhenPossible";
        internal const string Title = "You should remove the 'Where' invokation when it is possible.";
        internal const string MessageFormat = "You can remove 'Where' moving the predicate to '{0}'.";
        internal const string Category = "Syntax";

        static readonly string[] supportedMethods = new[] {
            "First",
            "FirstOrDefault",
            "Last",
            "LastOrDefault",
            "Any",
            "Single",
            "SingleOrDefault",
            "Count"
        };

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var whereInvoke = (InvocationExpressionSyntax)context.Node;
            
            if (GetNameOfTheInvokedMethod(whereInvoke) != "Where")
            {
                return;
            }

            var nextMethodInvoke = whereInvoke.Parent.
                FirstAncestorOrSelf<InvocationExpressionSyntax>();


            var candidate = GetNameOfTheInvokedMethod(nextMethodInvoke);
            if (!supportedMethods.Contains(candidate))
            {
                return;
            }

            if (nextMethodInvoke.ArgumentList.Arguments.Any())
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, GetNameExpressionOfTheInvokedMethod(whereInvoke).GetLocation(), candidate);
            context.ReportDiagnostic(diagnostic);
        }

        internal static string GetNameOfTheInvokedMethod(InvocationExpressionSyntax invoke)
        {
            if (invoke == null)
            {
                return null;
            }

            var memberAccess = invoke.ChildNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .FirstOrDefault();

            return GetNameExpressionOfTheInvokedMethod(invoke)?.ToString();
        }

        internal static SimpleNameSyntax GetNameExpressionOfTheInvokedMethod(InvocationExpressionSyntax invoke)
        {
            if (invoke == null)
            {
                return null;
            }

            var memberAccess = invoke.ChildNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .FirstOrDefault();

            return memberAccess?.Name;
        }
    }
}
