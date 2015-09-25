using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelper
{
    /// <summary>
    /// <see cref="CodeFixProvider"/>によるコードの修正をテストする基底クラスです。
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// テスト対象の<see cref="CodeFixProvider"/>のインスタンスを取得します。
        /// </summary>
        protected virtual CodeFixProvider GetCodeFixProvider()
        {
            return null;
        }

        /// <summary>
        /// <see cref="CodeFixProvider"/>によるコードの修正を検証します。
        /// </summary>
        /// <param name="oldSource">修正前のソースコード</param>
        /// <param name="newSource">修正後のソースコード</param>
        /// <param name="codeFixIndex">修正する箇所か複数ある場合に、それを特定するインデックス</param>
        /// <param name="allowNewCompilerDiagnostics">修正後に他の警告がある場合、テストを失敗させる場合は<see langword="true"/></param>
        protected void VerifyFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            var analyzer = GetDiagnosticAnalyzer();
            var codeFixProvider = GetCodeFixProvider();

            var document = CreateProject(new[] { oldSource }).Documents.First();
            var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });
            var compilerDiagnostics = document.GetCompilerDiagnostics();
            var attempts = analyzerDiagnostics.Length;

            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    document = document.ApplyFix(actions.ElementAt((int)codeFixIndex));
                    break;
                }

                document = document.ApplyFix(actions.ElementAt(0));
                analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });

                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, document.GetCompilerDiagnostics());

                //check if applying the code fix introduced any new compiler diagnostics
                if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                {
                    // Format and get the compiler diagnostics again so that the locations make sense in the output
                    document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, document.GetCompilerDiagnostics());

                    Assert.IsTrue(false,
                        string.Format("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                            string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                            document.GetSyntaxRootAsync().Result.ToFullString()));
                }

                //check if there are analyzer diagnostics left after the code fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            //after applying all of the code fixes, compare the resulting string to the inputted one
            var actual = document.GetStringFromDocument();
            Assert.AreEqual(newSource, actual);
        }

        /// <summary>
        /// 2つの診断結果を比較して、新たに検出された診断結果を取得します。
        /// </summary>
        /// <param name="diagnostics">既存の診断結果</param>
        /// <param name="newDiagnostics">新しい診断結果</param>
        /// <returns>新たに検出された診断結果</returns>
        private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
        {
            var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
            var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

            int oldIndex = 0;
            int newIndex = 0;

            while (newIndex < newArray.Length)
            {
                if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
                {
                    ++oldIndex;
                    ++newIndex;
                }
                else
                {
                    yield return newArray[newIndex++];
                }
            }
        }
    }
}