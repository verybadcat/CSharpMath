using Xunit;
using AngouriMath;

namespace CSharpMath.EvaluationTests {
  using Atom;
  
  public class LogicalOperatorPrecedenceTests {
    static MathList ParseLaTeX(string latex) =>
      LaTeXParser.MathListFromLaTeX(latex).Match(list => list, e => throw new Xunit.Sdk.XunitException(e));
    
    static Evaluation.MathItem ParseMath(string latex) =>
      Evaluation.Evaluate(ParseLaTeX(latex)).Match(entity => entity, e => throw new Xunit.Sdk.XunitException(e));
    
    static void AssertConversion(string input, string expectedConverted) {
      var math = ParseMath(input);
      Assert.NotNull(math);
      var actual = LaTeXParser.MathListToLaTeX(Evaluation.Visualize(math)).ToString();
      Assert.Equal(expectedConverted, actual);
    }

    [Theory(Skip = "Awaiting AngouriMath update")]
    // Test that ∧ binds tighter than ∨  
    [InlineData(@"a\land b\lor c", @"\left( a\land b\right) \lor c")]
    [InlineData(@"a\lor b\land c", @"a\lor \left( b\land c\right) ")]
    // Test that ∨ binds tighter than ⊕
    [InlineData(@"a\oplus b\land c", @"a\oplus \left( b\land c\right) ")]
    [InlineData(@"a\land b\oplus c", @"\left( a\land b\right) \oplus c")]
    // Test that → is right associative and lowest precedence
    [InlineData(@"a\to b\lor c", @"a\to \left( b\lor c\right) ")]
    [InlineData(@"a\lor b\to c", @"\left( a\lor b\right) \to c")]
    [InlineData(@"a\to b\to c", @"a\to \left( b\to c\right) ")]
    // Complex expression
    [InlineData(@"a\to b\land c\lor d", @"a\to \left( \left( b\land c\right) \lor d\right) ")]
    // Test ¬ has highest precedence
    [InlineData(@"\neg a\land b", @"\left( \neg a\right) \land b")]
    [InlineData(@"\neg a\lor\neg b", @"\left( \neg a\right) \lor \left( \neg b\right) ")]
    // Test relational operators bind tighter than logical
    [InlineData(@"a=b\land c=d", @"\left( a=b\right) \land \left( c=d\right) ")]
    [InlineData(@"a<b\lor c>d", @"\left( a<b\right) \lor \left( c>d\right) ")]
    public void LogicalOperatorPrecedence(string input, string expectedOutput) {
      AssertConversion(input, expectedOutput);
    }
  }
}
