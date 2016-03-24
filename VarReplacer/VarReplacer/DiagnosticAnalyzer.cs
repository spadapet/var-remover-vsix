using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VarReplacer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VarReplacerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "VarReplacer";
        private const string Title = "Var replacer";
        private const string MessageFormat = "Use '{0}' instead of var";
        private const string Description = "Using var will hide the type from code reviewers, so always use the actual type name.";
        private const string Category = "Style";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            VarReplacerAnalyzer.DiagnosticId,
            VarReplacerAnalyzer.Title,
            VarReplacerAnalyzer.MessageFormat,
            VarReplacerAnalyzer.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: VarReplacerAnalyzer.Description);

        public VarReplacerAnalyzer()
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(VarReplacerAnalyzer.Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            string foo = string.Concat("", "");
            context.RegisterSyntaxNodeAction(VarReplacerAnalyzer.AnalyzeSyntaxNode, SyntaxKind.FieldDeclaration, SyntaxKind.VariableDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            // Diagnostic diagnostic = Diagnostic.Create(VarReplacerAnalyzer.Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            // context.ReportDiagnostic(diagnostic);
        }
    }
}
