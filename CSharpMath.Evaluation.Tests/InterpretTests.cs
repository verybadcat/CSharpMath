using System;
using System.Linq;
using Xunit;
namespace CSharpMath.EvaluationTests {
  public class InterpretTests {
    [Theory]
    [InlineData(@"", @"\color{red}\text{There is nothing to evaluate}")]
    [InlineData(@"1", @"\underline\mathrm{Input}\\1\\\\\underline\mathrm{Simplified}\\1\\\\\underline\mathrm{Value\ (100\ digits)}\\1")]
    [InlineData(@"1+", @"\color{red}\text{Missing right operand for +}")]
    [InlineData(@"1+2", @"\underline\mathrm{Input}\\1+2\\\\\underline\mathrm{Simplified}\\3\\\\\underline\mathrm{Value\ (100\ digits)}\\3")]
    [InlineData(@"1+\sqrt", @"\color{red}\text{Missing radicand}")]
    [InlineData(@"1+\sqrt2", @"\underline\mathrm{Input}\\1+\sqrt{2}\\\\\underline\mathrm{Simplified}\\1+\sqrt{2}\\\\\underline\mathrm{Value\ (100\ digits)}\\2.414213562373095048801688724209698078569671875376948073176679737990732478462107038850387534327641573")]
    [InlineData(@"1+\sqrt{2x}", @"\underline\mathrm{Input}\\1+\sqrt{2 x}\\\\\underline\mathrm{Simplified}\\1+\sqrt{2 x}\\\\\underline\mathrm{Expanded}\\1+\sqrt{2 x}\\\\\underline\mathrm{Factorized}\\1+\sqrt{2 x}")]
    [InlineData(@"1+\sqrt{2xy}", @"\underline\mathrm{Input}\\1+\sqrt{2 x y}\\\\\underline\mathrm{Simplified}\\1+\sqrt{2 x y}\\\\\underline\mathrm{Expanded}\\1+\sqrt{2 x y}\\\\\underline\mathrm{Factorized}\\1+\sqrt{2 x y}")]
    [InlineData(@"=1+\sqrt{2xy}", @"\color{red}\text{Missing left side of equation}")]
    [InlineData(@"1+\sqrt{2xy}=", @"\color{red}\text{Missing right side of equation}")]
    [InlineData(@"1+\sqrt{2xy}=3", @"\underline\mathrm{Input}\\1+\sqrt{2 x y}=3\\\\\underline\mathrm{Solutions}\\x\in \left\{ \frac{\frac{4}{y}}{2} \right\}\\y\in \left\{ \frac{4}{2 x} \right\}\\")]
    [InlineData(@"1=3", @"\underline\mathrm{Input}\\1=3\\\\\underline\mathrm{Result}\\\text{False}")]
    [InlineData(@"1=1", @"\underline\mathrm{Input}\\1=1\\\\\underline\mathrm{Result}\\\text{True}")]
    public void Interpret(string input, string expected) {
      var actual = Evaluation.Interpret(EvaluationTests.ParseLaTeX(input));
      Assert.Equal(expected, actual);
      Assert.NotEmpty(EvaluationTests.ParseLaTeX(actual));
    }
  }
}