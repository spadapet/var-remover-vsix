using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VarReplacer
{
    [Shared]
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VarReplacerCodeFixProvider))]
    public class VarReplacerCodeFixProvider : CodeFixProvider
    {
        public VarReplacerCodeFixProvider()
        {
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(VarReplacerAnalyzer.DiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            IEnumerable<Diagnostic> diagnostics = context.Diagnostics.Where(s => s.Id == VarReplacerAnalyzer.DiagnosticId);

            foreach (Diagnostic diagnostic in diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Replace var with type",
                        token => VarReplacerCodeFixProvider.VarReplaceAsync(
                            context.Document,
                            diagnostic,
                            token),
                        equivalenceKey: string.Empty),
                    diagnostics);
            }
        }

        private static async Task<Document> VarReplaceAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken token)
        {
            return await Task.FromResult(document);
        }
    }
}
