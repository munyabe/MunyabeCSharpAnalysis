using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Munyabe.CSharp.Analysis.Analyzers.Performance;
using Munyabe.CSharp.Analysis.Test.Bases;

namespace Munyabe.CSharp.Analysis.Test.Analyzers.Performance
{
    /// <summary>
    /// <see cref="AvoidEnumHasFlagAnalyzer"/>をテストするクラスです。
    /// </summary>
    [TestClass]
    public class AvoidEnumHasFlagAnalyzerTest : CodeFixVerifier
    {
        [TestMethod]
        public void AnalyzeViolation()
        {
            var test = @"using System;

namespace AnalyzerSample.Test.Performance
{
    class AvoidEnumHasFlagAnalyzerTestTarget
    {
        public bool Method()
        {
            return TestEnum.Hoge.HasFlag(TestEnum.Fuga);
        }
    }

    [Flags]
    enum TestEnum
    {
        Hoge,
        Fuga
    }
}";

            var expected = new DiagnosticResult(
                AvoidEnumHasFlagAnalyzer.DiagnosticId,
                "Avoid Enum.HasFlag prefer bit operator",
                DiagnosticSeverity.Warning,
                new DiagnosticResultLocation("Test0.cs", 9, 20));

            VerifyDiagnostic(test, expected);
        }

        /// <inheritdoc />
        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new AvoidEnumHasFlagAnalyzer();
        }
    }
}
