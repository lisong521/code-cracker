﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using CodeCracker;
using Xunit;

namespace CodeCracker.Test
{
    public class RethrowExceptionTests : CodeFixVerifier
    {
        private const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Foo()
            {
                try { }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }";

        [Fact]
        public void WhenThrowingOriginalExceptionAnalyzerCreatesDiagnostic()
        {
            var expected = new DiagnosticResult
            {
                Id = RethrowExceptionAnalyzer.DiagnosticId,
                Message = "Don't throw the same exception you caught, you lose the original stack trace.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void WhenThrowingOriginalExceptionAndApplyingThrowNewExceptionFix()
        {

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Foo()
            {
                try { }
                catch (Exception ex)
                {
                    throw new Exception(""some reason to rethrow"", ex);
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);
        }

        [Fact]
        public void WhenThrowingOriginalExceptionAndApplyingRethrowFix()
        {

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Foo()
            {
                try { }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest, 1);
        }


        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RethrowExceptionCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RethrowExceptionAnalyzer();
        }
    }
}