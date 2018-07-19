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
    public class CSM0001TypographyNotImported : DiagnosticAnalyzer
    {
        /// <summary>
        /// The path of the Typography folder
        /// </summary>
        public static readonly string Typography = ((Func<string>)(() => {
            var L = Directory.GetCurrentDirectory();
            while (Path.GetFileName(L) != nameof(CSharpMath)) L = Path.GetDirectoryName(L);
            return Path.Combine(L, "Typography");
        }))();

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(nameof(CSM0001TypographyNotImported), "The Typography submodule must be imported.",
            "The Typography submodule was not imported. Execute 'git submodule update' in the platform shell at the CSharpMath repository to import it.",
            "File System", DiagnosticSeverity.Error, isEnabledByDefault: true);

        bool _reported = false;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterOperationBlockAction(Analyze);
            _reported = false;
        }

        private void Analyze(OperationBlockAnalysisContext context)
        {
            if (!Directory.Exists(Typography) && !_reported) { context.ReportDiagnostic(Diagnostic.Create(Rule, null)); _reported = true; }
        }
    }
}
