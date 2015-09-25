using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace TestHelper
{
    /// <summary>
    /// Diagnostic Producer class with extra methods dealing with applying codefixes
    /// All methods are static
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// 指定のコードアクションでドキュメントを修正します。
        /// </summary>
        /// <param name="document">修正するドキュメント</param>
        /// <param name="codeAction">ドキュメントに適用するコードアクション</param>
        /// <returns>修正後のドキュメント</returns>
        private static Document ApplyFix(Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
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

        /// <summary>
        /// ドキュメントの診断結果を取得します。
        /// </summary>
        /// <param name="document">対象のドキュメント</param>
        /// <returns>診断結果</returns>
        private static IEnumerable<Diagnostic> GetCompilerDiagnostics(Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }

        /// <summary>
        /// ドキュメントのテキストを取得します。
        /// </summary>
        /// <param name="document">対象のドキュメント</param>
        /// <returns>ドキュメントのテキスト</returns>
        private static string GetStringFromDocument(Document document)
        {
            var simplifiedDoc = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
            var root = simplifiedDoc.GetSyntaxRootAsync().Result;
            root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }
    }
}

