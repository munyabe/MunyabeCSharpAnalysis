using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Munyabe.CSharp.Analysis.Test.Bases
{
    /// <summary>
    /// <see cref="DiagnosticAnalyzer"/>によるコードの診断をテストする基底クラスです。
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        /// <summary>
        /// テスト対象の<see cref="DiagnosticAnalyzer"/>のインスタンスを取得します。
        /// </summary>
        protected abstract DiagnosticAnalyzer GetDiagnosticAnalyzer();

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証します。
        /// </summary>
        /// <param name="source">解析対象のソースコード</param>
        /// <param name="expected">診断結果の期待値</param>
        protected void VerifyDiagnostic(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnostic(new[] { source }, expected);
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証します。
        /// </summary>
        /// <param name="sources">解析対象のソースコード一覧</param>
        /// <param name="expected">診断結果の期待値</param>
        protected void VerifyDiagnostic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnosticInternal(CreateProject(sources), expected);
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証します。
        /// </summary>
        /// <param name="source">解析対象のソースコードファイル</param>
        /// <param name="expected">診断結果の期待値</param>
        protected void VerifyDiagnosticFromFile(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnosticFromFile(new[] { source }, expected);
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証します。
        /// </summary>
        /// <param name="sources">解析対象のソースコードファイル一覧</param>
        /// <param name="expected">診断結果の期待値</param>
        protected void VerifyDiagnosticFromFile(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnosticInternal(CreateProjectFromFile(sources), expected);
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証する内部メソッドです。
        /// </summary>
        private void VerifyDiagnosticInternal(Project project, params DiagnosticResult[] expected)
        {
            var analyzer = GetDiagnosticAnalyzer();
            var documents = project.Documents.ToArray();
            var diagnostics = GetSortedDiagnosticsFromDocuments(analyzer, documents);
            VerifyDiagnosticResults(diagnostics, analyzer, expected);
        }

        /// <summary>
        /// 診断結果を比較して、期待する結果かどうかを検証します。
        /// </summary>
        private static void VerifyDiagnosticResults(Diagnostic[] actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
        {
            var expectedCount = expectedResults.Length;
            var actualCount = actualResults.Length;

            Assert.AreEqual(expectedCount, actualCount,
                string.Format("Mismatch between number of diagnostics returned, expected '{0}' actual '{1}'\r\n\r\nDiagnostics:\r\n{2}", expectedCount, actualCount, FormatDiagnostics(analyzer, actualResults)));

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults[i];
                var expected = expectedResults[i];

                if (expected.Line == -1 && expected.Column == -1)
                {
                    Assert.AreEqual(Location.None, actual.Location,
                        string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}", FormatDiagnostics(analyzer, actual)));
                }
                else
                {
                    VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());
                    var additionalLocations = actual.AdditionalLocations;

                    Assert.AreEqual(expected.Locations.Length - 1, additionalLocations.Count,
                        string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n    {2}",
                            expected.Locations.Length - 1, additionalLocations.Count, FormatDiagnostics(analyzer, actual)));

                    for (var j = 0; j < additionalLocations.Count; ++j)
                    {
                        VerifyDiagnosticLocation(analyzer, actual, additionalLocations[j], expected.Locations[j + 1]);
                    }
                }

                const string MESSAGE_FORMAT = "Expected diagnostic {0} to be '{1}' was '{2}'\r\n\r\nDiagnostic:\r\n    {3}";
                Assert.AreEqual(expected.Id, actual.Id,
                    string.Format(MESSAGE_FORMAT, "id", expected.Id, actual.Id, FormatDiagnostics(analyzer, actual)));

                Assert.AreEqual(expected.Severity, actual.Severity,
                    string.Format(MESSAGE_FORMAT, "severity", expected.Severity, actual.Severity, FormatDiagnostics(analyzer, actual)));

                Assert.AreEqual(expected.Message, actual.GetMessage(),
                    string.Format(MESSAGE_FORMAT, "message", expected.Message, actual.GetMessage(), FormatDiagnostics(analyzer, actual)));
            }
        }

        /// <summary>
        /// 診断結果のソースコードの位置を検証する内部メソッドです。
        /// </summary>
        private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            const string MESSAGE_FORMAT = "Expected diagnostic to be {0} '{1}' was actually {0} '{2}'\r\n\r\nDiagnostic:\r\n    {3}";
            Assert.IsTrue(string.IsNullOrEmpty(expected.Path) || actualSpan.Path == expected.Path,
                string.Format(MESSAGE_FORMAT, "in file", expected.Path, actualSpan.Path, FormatDiagnostics(analyzer, diagnostic)));

            var actualLinePosition = actualSpan.StartLinePosition;

            if (actualLinePosition.Line > 0)
            {
                Assert.AreEqual(expected.Line, actualLinePosition.Line + 1,
                    string.Format(MESSAGE_FORMAT, "on line", expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(analyzer, diagnostic)));
            }

            if (actualLinePosition.Character > 0)
            {
                Assert.AreEqual(expected.Column, actualLinePosition.Character + 1,
                    string.Format(MESSAGE_FORMAT, "at column", expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(analyzer, diagnostic)));
            }
        }

        /// <summary>
        /// 診断結果を文字列に整えます。
        /// </summary>
        private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            if (diagnostics.Length == 0)
            {
                return "    NONE.\r\n";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i].ToString());

                var analyzerTypeName = analyzer.GetType().Name;

                foreach (var rule in analyzer.SupportedDiagnostics)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerTypeName, rule.Id);
                        }
                        else
                        {
                            Assert.IsTrue(location.IsInSource,
                                $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                            var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                            builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
                                "GetCSharpResultAt",
                                linePosition.Line + 1,
                                linePosition.Character + 1,
                                analyzerTypeName,
                                rule.Id);
                        }

                        if (i != diagnostics.Length - 1)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                        break;
                    }
                }
            }
            return builder.ToString();
        }
    }
}
