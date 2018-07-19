using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpMath.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class A0001TypographyNotImported : DiagnosticAnalyzer
    {
        public const string Id = "A0001";

        /// <summary>
        /// The path of the Typography folder
        /// </summary>
        public static readonly string Typography = ((Func<string>)(() => {
            var L = Directory.GetCurrentDirectory();
            while (Path.GetFileName(L) != nameof(CSharpMath)) L = Path.GetDirectoryName(L);
            return Path.Combine(L, "Typography");
        }))();

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(Id, "The Typography submodule must be imported.",
            "The Typography submodule was not imported. Execute 'git submodule update' in the platform shell at the CSharpMath repository to import it. (Git must be installed)",
            "File System", DiagnosticSeverity.Error, isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor Rule_ = new DiagnosticDescriptor("A0000", "Test",
            "This is a sanity check!",
            "Test", DiagnosticSeverity.Info, isEnabledByDefault: true);

        bool _reported = false;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule, Rule_); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCodeBlockAction(Analyze);
            context.RegisterCompilationAction(Analyze);
            _reported = false;
        }

        private void Analyze(CodeBlockAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule_, null));
            if (_reported) return;
            if(!Directory.EnumerateFileSystemEntries(Typography).Any()) context.ReportDiagnostic(Diagnostic.Create(Rule, null));
            _reported = true;
        }

        private void Analyze(CompilationAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule_, null));
            if (_reported) return;
            if(!Directory.EnumerateFileSystemEntries(Typography).Any()) context.ReportDiagnostic(Diagnostic.Create(Rule, null));
            _reported = true;
        }
    }
}
