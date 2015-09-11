using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Munyabe.CSharp.Analysis.Analyzers.Performance;
using TestHelper;

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

            var expected = new DiagnosticResult
            {
                Id = AvoidEnumHasFlagAnalyzer.DiagnosticId,
                Message = string.Format("Avoid Enum.HasFlag prefer bit operator"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 20)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        /// <inheritdoc />
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidEnumHasFlagAnalyzer();
        }
    }
}
