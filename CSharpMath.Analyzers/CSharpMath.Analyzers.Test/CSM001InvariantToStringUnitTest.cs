using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace CSharpMath.Analyzers.Test {
  [TestClass]
  public class CSM001InvariantToStringUnitTest : CodeFixVerifier {
    //No diagnostics expected to show up
    [TestMethod]
    public void TestMethod1() => VerifyCSharpDiagnostic(@"");

    //Diagnostic and CodeFix both triggered and checked for
    [TestMethod]
    public void TestMethod2() {
      const string test = @"
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
      VerifyCSharpDiagnostic(test, (DiagnosticResult)new DiagnosticResult {
        Id = "CSM001",
        Message = string.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations = new[] { new DiagnosticResultLocation("Test0.cs", 13, 51) }
      });
      VerifyCSharpFix(test, @"
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
    }");
    }

    [TestMethod]
    public void TestMethod3() {
      const string test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString();
        }
    }";
      VerifyCSharpDiagnostic(test, new DiagnosticResult {
        Id = "CSM001",
        Message = string.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 64) }
      });
      VerifyCSharpFix(test, @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString(null, System.Globalization.CultureInfo.InvariantCulture);
        }
    }");
    }

    [TestMethod]
    public void TestMethod4() {
      const string test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString() + "" "";
        }
    }";
      VerifyCSharpDiagnostic(test, new DiagnosticResult {
        Id = "CSM001",
        Message = string.Format("A culture-invariant ToString() is available"),
        Severity = DiagnosticSeverity.Warning,
        Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 64) }
      });
      VerifyCSharpFix(test, @"
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Return() => int.Parse(""123"").ToString(null, System.Globalization.CultureInfo.InvariantCulture) + "" "";
        }
    }");
    }
    protected override CodeFixProvider GetCSharpCodeFixProvider() =>
      new CSM001InvariantToStringCodeFixProvider();
    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
      new CSM001InvariantToStringAnalyzer();
  }
}
