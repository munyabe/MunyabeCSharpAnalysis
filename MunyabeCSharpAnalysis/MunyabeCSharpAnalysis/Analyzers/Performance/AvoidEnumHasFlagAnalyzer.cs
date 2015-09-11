using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Munyabe.CSharp.Analysis.Analyzers.Performance
{
    /// <summary>
    /// <see cref="Enum.HasFlag"/>の呼び出しを検出するルールです。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidEnumHasFlagAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// ルールの識別子です。
        /// </summary>
        public const string DiagnosticId = "Performance.AvoidEnumHasFlag";

        /// <summary>
        /// ルールの説明です。
        /// </summary>
        private static DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            "Avoid Enum.HasFlag",
            "Avoid Enum.HasFlag prefer bit operator",
            "Performance",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        /// <summary>
        /// メソッドを実行するノードを解析します。
        /// </summary>
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (IsEnumHasFlag(methodSymbol))
            {
                var diagnostic = Diagnostic.Create(_descriptor, node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// <see cref="Enum.HasFlag"/>の呼び出しかどうかを判定します。
        /// </summary>
        private static bool IsEnumHasFlag(IMethodSymbol symbol)
        {
            return symbol != null
                && symbol.Name == "HasFlag"
                && symbol.ContainingType.SpecialType == SpecialType.System_Enum
                && !symbol.IsStatic
                && symbol.MethodKind == MethodKind.Ordinary;
        }
    }
}
