using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Munyabe.CSharp.Analysis.Test.Bases
{
    /// <summary>
    /// Class for turning strings into documents and getting the diagnostics on them
    /// All methods are static
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        internal const string DefaultFilePathPrefix = "Test";
        internal const string TestProjectName = "TestProject";

        /// <summary>
        /// 指定の<see cref="DiagnosticAnalyzer"/>でドキュメントを診断した結果を取得します。
        /// </summary>
        /// <param name="analyzer">テスト対象の<see cref="DiagnosticAnalyzer"/></param>
        /// <param name="documents">解析対象のドキュメント一覧</param>
        /// <returns>ソースコードの位置でソートされた診断結果の一覧</returns>
        protected static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, Document[] documents)
        {
            var projects = new HashSet<Project>();
            foreach (var document in documents)
            {
                projects.Add(document.Project);
            }

            var diagnostics = new List<Diagnostic>();
            foreach (var project in projects)
            {
                var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
                var diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
                foreach (var diag in diags)
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else
                    {
                        for (int i = 0; i < documents.Length; i++)
                        {
                            var document = documents[i];
                            var tree = document.GetSyntaxTreeAsync().Result;
                            if (tree == diag.Location.SourceTree)
                            {
                                diagnostics.Add(diag);
                            }
                        }
                    }
                }
            }

            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        /// <summary>
        /// 指定の C# のソースコードを含んだプロジェクトを作成します。
        /// </summary>
        /// <param name="sources">プロジェクトに含めるソースコードの一覧</param>
        /// <returns>作成したプロジェクト</returns>
        protected static Project CreateProject(string[] sources)
        {
            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);
            var solution = CreateSolution(projectId);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = DefaultFilePathPrefix + count + ".cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }
            return solution.GetProject(projectId);
        }

        /// <summary>
        /// 指定の C# のソースコードファイルを含んだプロジェクトを作成します。
        /// </summary>
        /// <param name="files">プロジェクトに含めるソースコードファイルの一覧</param>
        /// <returns>作成したプロジェクト</returns>
        protected static Project CreateProjectFromFile(string[] files)
        {
            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);
            var solution = CreateSolution(projectId);

            foreach (var filePath in files)
            {
                var documentId = DocumentId.CreateNewId(projectId, debugName: filePath);

                using (var reader = new StreamReader(filePath))
                {
                    solution = solution.AddDocument(documentId, filePath, SourceText.From(reader.BaseStream));
                }
            }

            return solution.GetProject(projectId);
        }

        /// <summary>
        /// ソリューションを作成します。
        /// </summary>
        private static Solution CreateSolution(ProjectId projectId)
        {
            return new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, CSharpSymbolsReference)
                .AddMetadataReference(projectId, CodeAnalysisReference);
        }
    }
}

