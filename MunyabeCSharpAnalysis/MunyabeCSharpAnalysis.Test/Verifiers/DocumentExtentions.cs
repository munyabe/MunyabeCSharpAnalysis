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
    /// <see cref="Document"/>の拡張メソッドを定義するクラスです。
    /// </summary>
    internal static class DocumentExtensions
    {
        /// <summary>
        /// 指定のコードアクションでドキュメントを修正します。
        /// </summary>
        /// <param name="document">修正するドキュメント</param>
        /// <param name="codeAction">ドキュメントに適用するコードアクション</param>
        /// <returns>修正後のドキュメント</returns>
        internal static Document ApplyFix(this Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        /// <summary>
        /// ドキュメントの診断結果を取得します。
        /// </summary>
        /// <param name="document">対象のドキュメント</param>
        /// <returns>診断結果</returns>
        internal static IEnumerable<Diagnostic> GetCompilerDiagnostics(this Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }

        /// <summary>
        /// ドキュメントのテキストを取得します。
        /// </summary>
        /// <param name="document">対象のドキュメント</param>
        /// <returns>ドキュメントのテキスト</returns>
        internal static string GetStringFromDocument(this Document document)
        {
            var simplifiedDoc = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
            var root = simplifiedDoc.GetSyntaxRootAsync().Result;
            root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }
    }
}
