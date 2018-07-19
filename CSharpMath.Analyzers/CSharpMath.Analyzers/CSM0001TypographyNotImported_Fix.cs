using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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

namespace CSharpMath.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSM0001TypographyNotImported_Fix)), Shared]
    public class CSM0001TypographyNotImported_Fix : CodeFixProvider
    {
        private const string title = "Pull the Typography submodule using git via the platform shell.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(nameof(CSM0001TypographyNotImported));

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => FixAsync(context.Document.Project.Solution, c), 
                    equivalenceKey: title),
                context.Diagnostics.First());
            return Task.CompletedTask;
        }

        private Task<Solution> FixAsync(Solution solution, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Solution>();

            var process = new Process {
                StartInfo = { FileName = "git", Arguments = "submodule update", WorkingDirectory = CSM0001TypographyNotImported.Typography },
                EnableRaisingEvents = true
            };
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(solution);
                process.Dispose();
            };
            process.Start();

            return tcs.Task;
        }
    }
}
