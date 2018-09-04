using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace CSharpMath.Analyzers.Test {
  [TestClass]
  public class CSM001InvariantToStringUnitTest : CodeFixVerifier {

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
        class TypeName
        {
            public string Return() => 123.ToString();
        }
    }";
      var expected = new DiagnosticResult {
        Id = "CSM001",
        Message = string.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations =
              new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 51)
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
        class TypeName
        {
            public string Return() => 123.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
        }
    }";
      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void TestMethod3() {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString();
        }
    }";
      var expected = new DiagnosticResult {
        Id = "CSM001",
        Message = string.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations =
        new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 64)
            }
      };
      VerifyCSharpDiagnostic(test, expected);


      var fixtest = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString(null, System.Globalization.CultureInfo.InvariantCulture);
        }
    }";
      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void TestMethod4() {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString() + "" "";
        }
    }";
      var expected = new DiagnosticResult {
        Id = "CSM001",
        Message = string.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations =
        new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 64)
            }
      };
      VerifyCSharpDiagnostic(test, expected);


      var fixtest = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString(null, System.Globalization.CultureInfo.InvariantCulture) + "" "";
        }
    }";
      VerifyCSharpFix(test, fixtest);
    }

    protected override CodeFixProvider GetCSharpCodeFixProvider() {
      return new CSM001InvariantToStringCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
      return new CSM001InvariantToStringAnalyzer();
    }
  }
}
