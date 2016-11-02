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
            var expected = new DiagnosticResult(AvoidEnumHasFlagAnalyzer.DiagnosticId, new DiagnosticResultLocation(9, 20));
            VerifyDiagnosticFromFile(@"Analyzers\Performance\AvoidEnumHasFlagAnalyzerTarget.cs", expected);
        }

        /// <inheritdoc />
        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new AvoidEnumHasFlagAnalyzer();
        }
    }
}
