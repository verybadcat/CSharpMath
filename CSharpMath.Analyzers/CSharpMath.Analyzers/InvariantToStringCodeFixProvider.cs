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
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvariantToStringCodeFixProvider)), Shared]
  public class InvariantToStringCodeFixProvider : CodeFixProvider {
    private const string title = "Replace culture-sensitive ToString() with an culture-invariant one";

    public sealed override ImmutableArray<string> FixableDiagnosticIds {
      get { return ImmutableArray.Create(InvariantToStringAnalyzer.DiagnosticId); }
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
      var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

      // Register a code action that will invoke the fix.
      context.RegisterCodeFix(
          CodeAction.Create(
              title: title,
              createChangedSolution: c => ReplaceMethodCallAsync(context.Document, declaration, c),
              equivalenceKey: title),
          diagnostic);
    }

    private async Task<Solution> ReplaceMethodCallAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken) {
      // Compute new uppercase name.
      var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

      // Get the symbol representing the type to be renamed.
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

      // Produce a new solution that has all references to that type renamed, including the declaration.
      var originalSolution = document.Project.Solution;
      var optionSet = originalSolution.Workspace.Options;
      document = document.WithSyntaxRoot(new Rewriter(memberAccess).Visit(await document.GetSyntaxRootAsync()));
      var newSolution = document.Project.Solution;
      //var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, null, newName, optionSet, cancellationToken).ConfigureAwait(false);

      // Return the new solution with the now-uppercase type name.
      return newSolution;
    }

    class Rewriter : CSharpSyntaxRewriter {
      public Rewriter(MemberAccessExpressionSyntax replaceNode) =>
        _replaceNode = replaceNode;
      MemberAccessExpressionSyntax _replaceNode;
      public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
        return node == _replaceNode ? node.WithName(node.Name.WithIdentifier(SyntaxFactory.Identifier("ToStringInvariant"))) : node;
      }
    }
  }
}
