using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using CSharpMath.Analyzers;

namespace CSharpMath.Analyzers.Test {
  [TestClass]
  public class UnitTest : CodeFixVerifier {

    //No diagnostics expected to show up
    [TestMethod]
    public void TestMethod1() {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    //Diagnostic and CodeFix both triggered and checked for
    [TestMethod]
    public void TestMethod2() {
      var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public static class Extensions {
            public string ToStringInvariant(this int i) =>
                i.ToString(null, System.Globalization.InvariantCulture);
        }
        class TypeName
        {
            public string Return() => 123.ToString();
        }
    }";
      var expected = new DiagnosticResult {
        Id = "CSM01",
        Message = String.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations =
              new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 39)
                  }
      };

      VerifyCSharpDiagnostic(test, expected);

      var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public static class Extensions {
            public string ToStringInvariant(this int i) =>
                i.ToString(null, System.Globalization.InvariantCulture);
        }
        class TypeName
        {
            public string Return() => 123.ToStringInvariant();
        }
    }";
      VerifyCSharpFix(test, fixtest);
    }

    protected override CodeFixProvider GetCSharpCodeFixProvider() {
      return new InvariantToStringCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
      return new InvariantToStringAnalyzer();
    }
  }
}
