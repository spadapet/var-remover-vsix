using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

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
                string realName;
                if (diagnostic.Properties.TryGetValue("RealName", out realName) && !string.IsNullOrEmpty(realName))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Replace var with the actual type",
                            token => VarReplacerCodeFixProvider.VarReplaceAsync(
                                context.Document,
                                diagnostic.Location,
                                realName,
                                token),
                            equivalenceKey: VarReplacerAnalyzer.DiagnosticId),
                        diagnostic);
                }
            }
        }

        private static async Task<Document> VarReplaceAsync(
            Document document,
            Location location,
            string realName,
            CancellationToken token)
        {
            SourceText text;
            if (document.TryGetText(out text))
            {
                text = text.Replace(location.SourceSpan, realName);
                document = document.WithText(text);
            }

            return await Task.FromResult(document);
        }
    }
}
