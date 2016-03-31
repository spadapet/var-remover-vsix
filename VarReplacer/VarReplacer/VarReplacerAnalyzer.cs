using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

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
            DiagnosticSeverity.Error,
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
            context.RegisterSyntaxNodeAction(VarReplacerAnalyzer.AnalyzeVariable, SyntaxKind.VariableDeclaration);
        }

        private static void AnalyzeVariable(SyntaxNodeAnalysisContext context)
        {
            VariableDeclarationSyntax node = (VariableDeclarationSyntax)context.Node;
            VarReplacerAnalyzer.AnalyzeDeclaration(context, node);
        }

        private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context, VariableDeclarationSyntax node)
        {
            if (node != null &&
                node.Type != null &&
                node.Type.IsVar &&
                node.Variables != null &&
                node.Variables.Count == 1 &&
                !context.CancellationToken.IsCancellationRequested)
            {
                ITypeSymbol realType = null;
                ExpressionSyntax expression = node.Variables[0].Initializer?.Value;

                if (expression != null)
                {
                    TypeInfo expressionType = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);
                    realType = expressionType.Type;
                }

                if (realType != null && !realType.IsAnonymousType && !context.CancellationToken.IsCancellationRequested)
                {
                    string realName = realType.ToMinimalDisplayString(context.SemanticModel, node.Type.SpanStart);
                    IDictionary<string, string> props = new Dictionary<string, string>
                    {
                        {  "RealName", realName }
                    };

                    Diagnostic diagnostic = Diagnostic.Create(
                        VarReplacerAnalyzer.Rule,
                        node.Type.GetLocation(),
                        props.ToImmutableDictionary(),
                        realName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
