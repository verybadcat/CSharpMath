using System;
using Xunit;
using AngouriMath;

namespace CSharpMath.Evaluation.Tests {
  using Atom;
  public class MathSTests {
    MathList ParseLaTeX(string latex) =>
      LaTeXParser.MathListFromLaTeX(latex).Match(list => list, e => throw new Xunit.Sdk.XunitException(e));
    Entity ParseMath(string latex) =>
      MathS.FromMathList(ParseLaTeX(latex)).Match(entity => entity, e => throw new Xunit.Sdk.XunitException(e));
    void Test(string latex, string converted, string convertedLaTeX, string result) {
      var math = ParseMath(latex);
      Assert.Equal(converted, math.ToString());
      Assert.Equal(convertedLaTeX, LaTeXParser.MathListToLaTeX(MathS.ToMathList(converted)).ToString());
      Assert.Equal(result, math.Simplify().ToString());
    }
    [Theory]
    [InlineData("1")]
    [InlineData("1234")]
    [InlineData(" 1234")]
    [InlineData("1234 ")]
    [InlineData(" 1234 ")]
    [InlineData("1234 5678")]
    [InlineData(" 1234 5678")]
    [InlineData("1234 5678 ")]
    [InlineData(" 1234 5678 ")]
    public void Numbers(string number) =>
      Test(number, number.Replace(" ", null), number.Replace(" ", null), number.Replace(" ", null));
    [Theory]
    [InlineData("a", "a", "a", "a")]
    [InlineData("ab", "a * b", @"a\times b", "a * b")]
    [InlineData("abc", "a * b * c", @"a\times b\times c", "a * b * c")]
    [InlineData("3a", "3 * a", @"3\times a", "3 * a")]
    [InlineData("3ab", "3 * a * b", @"3\times a\times b", "3 * a * b")]
    [InlineData("3a3", "3 * a * 3", @"3\times a\times 3", "9 * a")]
    [InlineData("3aa", "3 * a * a", @"3\times a\times a", "3 * a ^ 2")]
    public void Variables(string latex, string converted, string convertedLaTeX, string result) =>
      Test(latex, converted, convertedLaTeX, result);

    [Theory]
    [InlineData("a + b", "a + b", @"a+b", "a + b")]
    [InlineData("a - b", "a - b", @"a-b", "a - b")]
    [InlineData("a * b", "a * b", @"a\times b", "a * b")]
    [InlineData(@"a\times b", "a * b", @"a\times b", "a * b")]
    [InlineData(@"a\cdot b", "a * b", @"a\times b", "a * b")]
    [InlineData(@"a / b", "a / b", @"\frac{a}{b}", "a / b")]
    [InlineData(@"a\div b", "a / b", @"\frac{a}{b}", "a / b")]
    [InlineData(@"\frac ab", "a / b", @"\frac{a}{b}", "a / b")]
    [InlineData("a + b + c", "a + b + c", @"a+b+c", "a + b + c")]
    [InlineData("a + b - c", "a + b - c", @"a+b-c", "a + b - c")]
    [InlineData("a + b * c", "a + b * c", @"a+b\times c", "a + b * c")]
    [InlineData("a + b / c", "a + b / c", @"a+\frac{b}{c}", "a + b / c")]
    [InlineData("a - b + c", "a - b + c", @"a-b+c", "a + c - b")]
    [InlineData("a - b - c", "a - b - c", @"a-b-c", "a - (b + c)")]
    [InlineData("a - b * c", "a - b * c", @"a-b\times c", "a - b * c")]
    [InlineData("a - b / c", "a - b / c", @"a-\frac{b}{c}", "a - b * c ^ (-1)")]
    [InlineData("a * b + c", "a * b + c", @"a\times b+c", "a * b + c")]
    [InlineData("a * b - c", "a * b - c", @"a\times b-c", "a * b - c")]
    [InlineData("a * b * c", "a * b * c", @"a\times b\times c", "a * b * c")]
    [InlineData("a * b / c", "a * b / c", @"\frac{a\times b}{c}", "a * b / c")]
    [InlineData("a / b + c", "a / b + c", @"\frac{a}{b}+c", "a / b + c")]
    [InlineData("a / b - c", "a / b - c", @"\frac{a}{b}-c", "a / b - c")]
    [InlineData("a / b * c", "a / b * c", @"\frac{a}{b}\times c", "a * c / b")]
    [InlineData("a / b / c", "a / b / c", @"\frac{\frac{a}{b}}{c}", "a * b ^ (-1) / c")]
    public void BinaryOperators(string latex, string converted, string convertedLaTeX, string result) =>
      Test(latex, converted, convertedLaTeX, result);
    [Theory]
    [InlineData("+a", "a", @"a", "a")]
    [InlineData("-a", "(-1) * a", @"-1\times a", "(-1) * a")]
    [InlineData("++a", "a", @"a", "a")]
    [InlineData("+-a", "(-1) * a", @"-1\times a", "(-1) * a")]
    [InlineData("-+a", "(-1) * a", @"-1\times a", "(-1) * a")]
    [InlineData("--a", "(-1) * (-1) * a", @"-1\times -1\times a", "a")]
    [InlineData("+++a", "a", @"a", "a")]
    [InlineData("---a", "(-1) * (-1) * (-1) * a", @"-1\times -1\times -1\times a", "(-1) * a")]
    [InlineData("a++a", "a + a", @"a+a", "2 * a")]
    [InlineData("a+-a", "a + (-1) * a", @"a+-1\times a", "0")]
    [InlineData("a-+a", "a - a", @"a-a", "0")]
    [InlineData("a--a", "a - (-1) * a", @"a--1\times a", "2 * a")]
    [InlineData("a+++a", "a + a", @"a+a", "2 * a")]
    [InlineData("a---a", "a - (-1) * (-1) * a", @"a--1\times -1\times a", "0")]
    [InlineData("a*+a", "a * a", @"a\times a", "a ^ 2")]
    [InlineData("a*-a", "a * (-1) * a", @"a\times -1\times a", "(-1) * a ^ 2")]
    [InlineData("a/+a", "a / a", @"\frac{a}{a}", "1")]
    [InlineData("a/-a", "a / ((-1) * a)", @"\frac{a}{-1\times a}", "(-1)")]
    public void UnaryOperators(string latex, string converted, string convertedLaTeX, string result) =>
      Test(latex, converted, convertedLaTeX, result);

    [Theory]
    [InlineData("2^2", "2 ^ 2", @"2^2", "4")]
    [InlineData("a^a", "a ^ a", @"a^a", "a ^ a")]
    [InlineData("a^{a+b}", "a ^ (a + b)", @"a^{a+b}", "a ^ (a + b)")]
    [InlineData("2^{3^4}", "2 ^ 3 ^ 4", @"2^{3^4}", "2.4178516392292583E+24")]
    [InlineData("4^{3^2}", "4 ^ 3 ^ 2", @"4^{3^2}", "262144")]
    [InlineData("4^3+2", "4 ^ 3 + 2", @"4^3+2", "66")]
    [InlineData("2+3^4", "2 + 3 ^ 4", @"2+3^4", "83")]
    [InlineData("4^3*2", "4 ^ 3 * 2", @"4^3\times 2", "128")]
    [InlineData("2*3^4", "2 * 3 ^ 4", @"2\times 3^4", "162")]
    [InlineData(@"{\frac 12}^4", "(1 / 2) ^ 4", @"\left( \frac{1}{2}\right) ^4", "0.0625")]
    [InlineData(@"{\sqrt 2}^4", "2 ^ (1 / 2) ^ 4", @"2^{\left( \frac{1}{2}\right) ^4}", "4.000000000000001")]
    public void Exponents(string latex, string converted, string convertedLaTeX, string result) =>
      Test(latex, converted, convertedLaTeX, result);

    [Fact(Skip = "Just Tesst")]
    public void Tesst() => System.Diagnostics.Debug.WriteLine(
      AngouriMath.MathS.Solve(new System.Collections.Generic.List<Entity> { AngouriMath.MathS.FromString("x+y-pi-z+w") },
        System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(AngouriMath.MathS.GetUniqueVariables(AngouriMath.MathS.FromString("x+y-pi-z+w")), v => (VariableEntity)v))));
  }
}
