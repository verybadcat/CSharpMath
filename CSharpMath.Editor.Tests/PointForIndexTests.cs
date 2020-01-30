using System.Drawing;
using CSharpMath.Tests.FrontEnd;
using Xunit;

namespace CSharpMath.Editor.Tests {
  using static IndexForPointTests;
  // Use the "CSharpMath.Editor Test Checker" project in the _Utils folder to visualize the test cases
  using SubIndex = MathListSubIndexType;
  public class PointForIndexTests {
    void Test(string latex, PointF expected, MathListIndex index) {
      var display = CreateDisplay(latex);
      Assert.NotNull(display);
      var pr = display.PointForIndex(TestTypesettingContexts.Instance, index);
      System.Diagnostics.Debug.WriteLine(pr);
      CSharpMath.Tests.Approximately.Equal(expected, pr, 0.001);
    }

    public static TestData FractionData =>
      new TestData {
        { (0,0), 0 },
        { (0, -13.72), 0, (SubIndex.Denominator, 0) },
        { (0, 13.54), 0, (SubIndex.Numerator, 0) },
        { (10, -13.72), 0, (SubIndex.Denominator, 1) },
        { (10, 13.54), 0, (SubIndex.Numerator, 1) },
        { (10, 0), 1 },
      };
    [Theory, MemberData(nameof(FractionData))]
    public void Fraction(PointF point, MathListIndex expected) => Test(@"\frac32", point, expected);

    public static TestData RegularData =>
      new TestData {
        { (0, 0), 0 },
        { (14.444, 0), 1 },
        { (28.888, 0), 2 },
        { (38.888, 0), 3 },
      };
    [Theory, MemberData(nameof(RegularData))]
    public void Regular(PointF point, MathListIndex expected) => Test(@"4+2", point, expected);

    public static TestData RegularPlusFractionData =>
      new TestData {
        { (0, 0), 0 },
        { (14.444, 0), 1 },
        { (28.888, 0), 2 },
        { (28.888, -13.72), 2, (SubIndex.Denominator, 0) },
        { (28.888, 13.54), 2, (SubIndex.Numerator, 0) },
        { (38.888, -13.72), 2, (SubIndex.Denominator, 1) },
        { (38.888, 13.54), 2, (SubIndex.Numerator, 1) },
        { (38.888, 0), 3 },
      };
    [Theory, MemberData(nameof(RegularPlusFractionData))]
    public void RegularPlusFraction(PointF point, MathListIndex expected) =>
      Test(@"1+\frac{3}{2}", point, expected);

    public static TestData FractionPlusRegularData =>
      new TestData {
        { (0,0), 0 },
        { (10, -13.72), 0, (SubIndex.Denominator, 1) },
        { (10, 13.54), 0, (SubIndex.Numerator, 1) },
        { (14.4444, 0), 1 },
      };
    [Theory, MemberData(nameof(FractionPlusRegularData))]
    public void FractionPlusRegular(PointF point, MathListIndex expected) =>
      Test(@"\frac32+1", point, expected);

    public static TestData RadicalData =>
      new TestData {
        { (0, 0), 0 },
        { (10, 0), 0, (SubIndex.Radicand, 0) },
        { (20, 0), 0, (SubIndex.Radicand, 1) },
      };
    [Theory, MemberData(nameof(RadicalData))]
    public void Radical(PointF point, MathListIndex expected) =>
      Test(@"\sqrt2", point, expected);
    public static TestData RadicalDegreeData =>
      new TestData {
        { (0, 0), 0 },
        { (5.56, 8.736), 0, (SubIndex.Degree, 0) },
        { (12.56, 8.736), 0, (SubIndex.Degree, 1) },
        { (11.44, 0), 0, (SubIndex.Radicand, 0) },
        { (21.44, 0), 0, (SubIndex.Radicand, 1) },
      };
    [Theory, MemberData(nameof(RadicalDegreeData))]
    public void RadicalDegree(PointF point, MathListIndex expected) =>
      Test(@"\sqrt[3]2", point, expected);
    public static TestData ExponentData =>
      new TestData {
        { (0, 0), 0 },
        { (10, 0), 0, (SubIndex.BetweenBaseAndScripts, 1) },
        { (10, 7.26), 0, (SubIndex.Superscript, 0) },
        { (17, 0), 1 },
      };
    [Theory, MemberData(nameof(ExponentData))]
    public void Exponent(PointF point, MathListIndex expected) => Test("2^3", point, expected);

    public static TestData Issue46Data =>
      new TestData {
        { (57.777, 0), 4 },
        { (75.097, 0), 5 },
      };
    [Theory, MemberData(nameof(Issue46Data))] // https://github.com/verybadcat/CSharpMath/issues/46
    public void Issue46(PointF point, MathListIndex expected) => Test("2+x+x^y", point, expected);

    public static TestData ComplexData =>
      new TestData {
        { (0, 0), 0 },
        // \frac a\frac bc
        { (1.5, -23.5), 0, (SubIndex.Denominator, 0), (SubIndex.Denominator, 0) },
        { (1.5, -7.6), 0, (SubIndex.Denominator, 0), (SubIndex.Numerator, 0) },
        { (0, 13.54), 0, (SubIndex.Numerator, 0) },
        { (8.5, -23.5), 0, (SubIndex.Denominator, 0), (SubIndex.Denominator, 1) },
        { (8.5, -7.6), 0, (SubIndex.Denominator, 0), (SubIndex.Numerator, 1) },
        { (10, 13.54), 0, (SubIndex.Numerator, 1) },
        { (8.5, -16.6), 0, (SubIndex.Denominator, 1) },
        // \frac\frac123
        { (13.333, -13.72), 1, (SubIndex.Denominator, 0) },
        { (14.833, 10.6), 1, (SubIndex.Numerator, 0), (SubIndex.Denominator, 0) },
        { (14.833, 26.5), 1, (SubIndex.Numerator, 0), (SubIndex.Numerator, 0) },
        { (23.333, -13.72), 1, (SubIndex.Denominator, 1) },
        { (21.833, 17.5), 1, (SubIndex.Numerator, 1) },
        { (21.833, 26.5), 1, (SubIndex.Numerator, 0), (SubIndex.Numerator, 1) },
        // \sqrt d^e
        { (36.667, 0), 2, (SubIndex.Radicand, 0) },
        { (46.667, 0), 2, (SubIndex.Radicand, 1) },
        { (46.667, 13.478), 2, (SubIndex.Superscript, 0) },
        { (53.667, 13.478), 2, (SubIndex.Superscript, 1) },
        // \sqrt[5]6
        { (70.671, 0), 3, (SubIndex.Radicand, 0) },
        { (64.791, 8.736), 3, (SubIndex.Degree, 0) },
        { (71.791, 8.736), 3, (SubIndex.Degree, 1) },
        { (80.671, 0), 3, (SubIndex.Radicand, 1) },
        // \sqrt[f]g^{7_8}_{9^0}
        { (96.556, 0), 4, (SubIndex.Radicand, 0) },
        { (90.676, 8.736), 4, (SubIndex.Degree, 0) },
        { (97.676, 8.736), 4, (SubIndex.Degree, 1) },
        { (106.556, -6.8), 4, (SubIndex.Subscript, 0) },
        { (106.556, 0), 4, (SubIndex.Radicand, 1) },
        { (118.556, 10.02), 4, (SubIndex.Superscript, 0), (SubIndex.Subscript, 1) },
        { (113.556, -6.8), 4, (SubIndex.Subscript, 0), (SubIndex.BetweenBaseAndScripts, 1) },
        { (113.556, -2.754), 4, (SubIndex.Subscript, 0), (SubIndex.Superscript, 0) },
        { (118.556, -6.8), 4, (SubIndex.Subscript, 1) },
        { (118.556, -2.754), 4, (SubIndex.Subscript, 0), (SubIndex.Superscript, 1) },
        { (118.556, 0), 5 }
      };
    [Theory, MemberData(nameof(ComplexData))]
    public void Complex(PointF point, MathListIndex expected) => 
      Test(@"\frac a\frac bc\frac\frac123\sqrt d^e\sqrt[5]6\sqrt[f]g^{7_8}_{9^0}", point, expected);
    public static TestData SineData =>
      new TestData {
        { (0, 0), 0 },
        { (33.333, 0), 1 },
        { (43.333, 0), 2 },
      };
    [Theory, MemberData(nameof(SineData))]
    public void Sine(PointF point, MathListIndex expected) => Test(@"\sin\pi", point, expected);
  }
}