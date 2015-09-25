using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelper
{
    /// <summary>
    /// <see cref="DiagnosticAnalyzer"/>によるコードの診断をテストする基底クラスです。
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        /// <summary>
        /// テスト対象の<see cref="DiagnosticAnalyzer"/>のインスタンスを取得します。
        /// </summary>
        protected virtual DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return null;
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証します。
        /// </summary>
        /// <param name="source">解析対象のソースコード</param>
        /// <param name="expected">診断結果の期待値</param>
        protected void VerifyDiagnostic(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(new[] { source }, GetDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証します。
        /// </summary>
        /// <param name="sources">解析対象のソースコード一覧</param>
        /// <param name="expected">診断結果の期待値</param>
        protected void VerifyDiagnostic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(sources, GetDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// <see cref="DiagnosticAnalyzer"/>によるコードの診断を検証する内部メソッドです。
        /// </summary>
        private void VerifyDiagnostics(string[] sources, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
        {
            var diagnostics = GetSortedDiagnostics(sources, analyzer);
            VerifyDiagnosticResults(diagnostics, analyzer, expected);
        }

        /// <summary>
        /// 診断結果を比較して、期待する結果かどうかを検証します。
        /// </summary>
        private static void VerifyDiagnosticResults(Diagnostic[] actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
        {
            int expectedCount = expectedResults.Length;
            int actualCount = actualResults.Length;

            if (expectedCount != actualCount)
            {
                string diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(analyzer, actualResults) : "    NONE.";

                Assert.IsTrue(false,
                    string.Format("Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n", expectedCount, actualCount, diagnosticsOutput));
            }

            for (int i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults[i];
                var expected = expectedResults[i];

                if (expected.Line == -1 && expected.Column == -1)
                {
                    if (actual.Location != Location.None)
                    {
                        Assert.IsTrue(false,
                            string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
                            FormatDiagnostics(analyzer, actual)));
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());
                    var additionalLocations = actual.AdditionalLocations;

                    if (additionalLocations.Count != expected.Locations.Length - 1)
                    {
                        Assert.IsTrue(false,
                            string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n    {2}\r\n",
                                expected.Locations.Length - 1, additionalLocations.Count,
                                FormatDiagnostics(analyzer, actual)));
                    }

                    for (int j = 0; j < additionalLocations.Count; ++j)
                    {
                        VerifyDiagnosticLocation(analyzer, actual, additionalLocations[j], expected.Locations[j + 1]);
                    }
                }

                if (actual.Id != expected.Id)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic id to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Id, actual.Id, FormatDiagnostics(analyzer, actual)));
                }

                if (actual.Severity != expected.Severity)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Severity, actual.Severity, FormatDiagnostics(analyzer, actual)));
                }

                if (actual.GetMessage() != expected.Message)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic message to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Message, actual.GetMessage(), FormatDiagnostics(analyzer, actual)));
                }
            }
        }

        /// <summary>
        /// 診断結果のソースコードの位置を検証する内部メソッドです。
        /// </summary>
        private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            Assert.IsTrue(actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")),
                string.Format("Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                    expected.Path, actualSpan.Path, FormatDiagnostics(analyzer, diagnostic)));

            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0)
            {
                if (actualLinePosition.Line + 1 != expected.Line)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(analyzer, diagnostic)));
                }
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0)
            {
                if (actualLinePosition.Character + 1 != expected.Column)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(analyzer, diagnostic)));
                }
            }
        }

        /// <summary>
        /// 診断結果を文字列に整えます。
        /// </summary>
        private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i].ToString());

                var analyzerType = analyzer.GetType();
                var rules = analyzer.SupportedDiagnostics;

                foreach (var rule in rules)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
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
                                analyzerType.Name,
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
