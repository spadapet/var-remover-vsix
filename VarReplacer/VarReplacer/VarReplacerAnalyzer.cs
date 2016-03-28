using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VarReplacer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VarReplacerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "VarIsBad";
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
            context.RegisterSyntaxNodeAction(VarReplacerAnalyzer.AnalyzeField, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(VarReplacerAnalyzer.AnalyzeVariable, SyntaxKind.VariableDeclaration);
        }

        private static void AnalyzeField(SyntaxNodeAnalysisContext context)
        {
            FieldDeclarationSyntax node = (FieldDeclarationSyntax)context.Node;
            VarReplacerAnalyzer.AnalyzeDeclaration(context, node.Declaration);
        }

        private static void AnalyzeVariable(SyntaxNodeAnalysisContext context)
        {
            VariableDeclarationSyntax node = (VariableDeclarationSyntax)context.Node;
            VarReplacerAnalyzer.AnalyzeDeclaration(context, node);
        }

        private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context, VariableDeclarationSyntax node)
        {
            if (node != null && node.Type != null && node.Type.IsVar && !context.CancellationToken.IsCancellationRequested)
            {
                ITypeSymbol realType = null;
                VariableDeclaratorSyntax decl = node.Variables.FirstOrDefault();

                ExpressionSyntax expression = decl?.Initializer?.Value;
                if (expression != null)
                {
                    TypeInfo expressionType = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);
                    realType = expressionType.Type;
                }

                if (realType != null && !context.CancellationToken.IsCancellationRequested)
                {
                    Diagnostic diagnostic = Diagnostic.Create(VarReplacerAnalyzer.Rule, node.Type.GetLocation(), realType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
