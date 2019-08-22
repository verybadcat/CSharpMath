using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace CSharpMath.Analyzers {
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSM001InvariantToStringCodeFixProvider)), Shared]
  public class CSM001InvariantToStringCodeFixProvider : CodeFixProvider {
    private const string title = "Replace culture-sensitive ToString() with an culture-invariant one";

    public sealed override ImmutableArray<string> FixableDiagnosticIds {
      get { return ImmutableArray.Create(CSM001InvariantToStringAnalyzer.DiagnosticId); }
    }

    public sealed override FixAllProvider GetFixAllProvider() {
      // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
      return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      
      // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      // Find the type declaration identified by the diagnostic.
      var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ArgumentListSyntax>().First();

      // Register a code action that will invoke the fix.
      context.RegisterCodeFix(
          CodeAction.Create(
              title: title,
              createChangedSolution: c => ReplaceMethodCallAsync(context.Document, declaration, c),
              equivalenceKey: title),
          diagnostic);
    }

    private async Task<Solution> ReplaceMethodCallAsync(Document document, ArgumentListSyntax argList, CancellationToken cancellationToken) {
      // Get the symbol representing the type to be renamed.
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
      // Produce a new solution that has all references to that type renamed, including the declaration.
      var originalSolution = document.Project.Solution;
      var optionSet = originalSolution.Workspace.Options;
      _ = (semanticModel, optionSet);
      
      document = document.WithSyntaxRoot(new Rewriter(argList).Visit(await document.GetSyntaxRootAsync()));
      var newSolution = document.Project.Solution;
      //var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, null, newName, optionSet, cancellationToken).ConfigureAwait(false);

      // Return the new solution with the now-uppercase type name.
      return newSolution;
    }

    class Rewriter : CSharpSyntaxRewriter {
      public Rewriter(ArgumentListSyntax replaceNode) =>
        _replaceNode = replaceNode;
      readonly ArgumentListSyntax _replaceNode;
      public override SyntaxNode VisitArgumentList(ArgumentListSyntax node) {
      /*MemberAccessExpressionSyntax GenerateMemberAccess(params string[] identifiers) =>
          identifiers.Skip(2).Aggregate(
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(identifiers[0]), SyntaxFactory.IdentifierName(identifiers[1])),
            (acc, curr) => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, acc, SyntaxFactory.IdentifierName(curr)));*/
        return node.IsEquivalentTo(_replaceNode) ?
          node.WithArguments(SyntaxFactory.SeparatedList(new[] {
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
            SyntaxFactory.Argument(
              SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(nameof(System)),
                        SyntaxFactory.IdentifierName(nameof(System.Globalization))),
                    SyntaxFactory.IdentifierName(nameof(System.Globalization.CultureInfo))),
                SyntaxFactory.IdentifierName(nameof(System.Globalization.CultureInfo.InvariantCulture))))
          }))
          : node;
      }
    }
  }
}
