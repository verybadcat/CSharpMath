using System;
using System.Collections.Generic;
using System.Drawing;
using Xunit;
using CSharpMath;
using CSharpMath.Atoms;
using CSharpMath.Editor;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Tests.FrontEnd;
using ListDisplay = CSharpMath.Display.ListDisplay<CSharpMath.Tests.FrontEnd.TestFont, char>;

namespace CSharpMath.Editor.Tests {
  // Use the "CSharpMath.Editor Test Checker" project in the _Utils folder to visualize the test cases
  using SubIndex = MathListSubIndexType;

  public class ClosestPointTests {
    public class TestData : TheoryData {
      // Format of test data
      public void Add((double x, double y) point,
        int index, params (SubIndex subType, int subIndex)[] subIndexRecursive) {
        MathListIndex mathListIndex;
        if (subIndexRecursive.Length == 0)
          mathListIndex = MathListIndex.Level0Index(index);
        else {
          mathListIndex = MathListIndex.Level0Index(subIndexRecursive[subIndexRecursive.Length - 1].subIndex);
          for (var i = subIndexRecursive.Length - 2; i >= 0; i--)
            mathListIndex = MathListIndex.IndexAtLocation(subIndexRecursive[i].subIndex, 
              subIndexRecursive[subIndexRecursive.Length + 1].subType, mathListIndex);
          mathListIndex = MathListIndex.IndexAtLocation(index, subIndexRecursive[0].subType, mathListIndex);
        }
        AddRow(new PointF((float)point.x, (float)point.y), mathListIndex);
      }
    }
    public static readonly TestFont Font = new TestFont(20);
    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;
    
    public static ListDisplay CreateDisplay(string expr) =>
      MathLists.FromString(expr) is Interfaces.IMathList l ?
      Typesetter<TestFont, char>.CreateLine(l, Font, context, LineStyle.Display) :
      null;

    void Test(ListDisplay displayList, PointF point, MathListIndex expected) {
      var actual = displayList.IndexForPoint(context, point);
      if (!expected.EqualsToIndex(actual))
        //Xunit disallows custom user messages
        Assert.True(expected == actual, $"\nPoint: {point}\n" +
          $"Expected: {expected?.ToString() ?? "(null)"}\nActual: {actual?.ToString() ?? "(null)"}");
    }

    // ^ Helper functions
    // v Actual test cases

    public static TestData FractionData =>
      new TestData {
        { (-10, -20), 0 },
        { (-10, 0), 0 },
        { (-10, 8), 0 },
        { (-10, 40), 0 },
        { (-2.5, -20), 0 },
        { (-2.5, 0), 0 },
        { (-2.5, 8), 0 },
        { (-2.5, 40), 0 },
        { (-1, -20), 0, (SubIndex.Denominator, 0) },
        { (-1, 0), 0, (SubIndex.Denominator, 0) },
        { (-1, 8), 0, (SubIndex.Numerator, 0) },
        { (-1, 40), 0, (SubIndex.Numerator, 0) },
        { (3, -20), 0, (SubIndex.Denominator, 0) },
        { (3, 0), 0, (SubIndex.Denominator, 0) },
        { (3, 8), 0, (SubIndex.Numerator, 0) },
        { (3, 40), 0, (SubIndex.Numerator, 0) },
        { (7, -20), 0, (SubIndex.Denominator, 1) },
        { (7, 0), 0, (SubIndex.Denominator, 1) },
        { (7, 8), 0, (SubIndex.Numerator, 1) },
        { (7, 40), 0, (SubIndex.Numerator, 1) },
        { (11, -20), 1 },  // because it is below the height of the fraction
        { (11, 0), 0, (SubIndex.Denominator, 1) },
        { (11, 8), 0, (SubIndex.Numerator, 1) },
        { (11, 40), 0, (SubIndex.Numerator, 1) },
        { (12.5, -20), 1 },
        { (12.5, 0), 1 },
        { (12.5, 8), 1 },
        { (12.5, 40), 1 },
        { (20, -20), 1 },
        { (20, 0), 1 },
        { (20, 8), 1 },
        { (20, 40), 1 },
       };
    static readonly ListDisplay Fraction = CreateDisplay(@"\frac32");
    [Theory, MemberData(nameof(FractionData))]
    public void FractionTest(PointF point, MathListIndex expected) => Test(Fraction, point, expected);

    public static TestData RegularData =>
      new TestData {
        { (-10, -20), 0 },
        { (-10, 0), 0 },
        { (-10, 8), 0 },
        { (-10, 40), 0 },
        { (0, -20), 0 },
        { (0, 0), 0 },
        { (0, 8), 0 },
        { (0, 40), 0 },
        { (10, -20), 1 },
        { (10, 0), 1 },
        { (10, 8), 1 },
        { (10, 40), 1 },
        { (15, -20), 1 },
        { (15, 0), 1 },
        { (15, 8), 1 },
        { (15, 40), 1 },
        { (25, -20), 2 },
        { (25, 0), 2 },
        { (25, 8), 2 },
        { (25, 40), 2 },
        { (35, -20), 3 },
        { (35, 0), 3 },
        { (35, 8), 3 },
        { (35, 40), 3 },
        { (45, -20), 3 },
        { (45, 0), 3 },
        { (45, 8), 3 },
        { (45, 40), 3 },
        { (55, -20), 3 },
        { (55, 0), 3 },
        { (55, 8), 3 },
        { (55, 40), 3 },
      };
    static readonly ListDisplay Regular = CreateDisplay(@"4+2");
    [Theory, MemberData(nameof(RegularData))]
    public void RegularTest(PointF point, MathListIndex expected) => Test(Regular, point, expected);

    public static TestData RegularPlusFractionData =>
      new TestData {
        { (26, -20), 2 },
        { (26, 0), 2 },
        { (26, 8), 2 },
        { (26, 40), 2 },
        { (28, -20), 2, (SubIndex.Denominator, 0) },
        { (28, 0), 2, (SubIndex.Denominator, 0) },
        { (28, 8), 2, (SubIndex.Numerator, 0) },
        { (28, 40), 2, (SubIndex.Numerator, 0) },
        { (33, -20), 2, (SubIndex.Denominator, 0) },
        { (33, 0), 2, (SubIndex.Denominator, 0) },
        { (33, 8), 2, (SubIndex.Numerator, 0) },
        { (33, 40), 2, (SubIndex.Numerator, 0) },
        { (35, -20), 2, (SubIndex.Denominator, 1) },
        { (35, 0), 2, (SubIndex.Denominator, 1) },
        { (35, 8), 2, (SubIndex.Numerator, 1) },
        { (35, 40), 2, (SubIndex.Numerator, 1) },
      };
    static readonly ListDisplay RegularPlusFraction = CreateDisplay(@"1+\frac{3}{2}");
    [Theory, MemberData(nameof(RegularPlusFractionData))]
    public void RegularPlusFractionTest(PointF point, MathListIndex expected) =>
      Test(RegularPlusFraction, point, expected);

    public static TestData FractionPlusRegularData =>
      new TestData {
        { (9, -20), 0, (SubIndex.Denominator, 1) },
        { (9, 0), 0, (SubIndex.Denominator, 1) },
        { (9, 8), 0, (SubIndex.Numerator, 1) },
        { (9, 40), 0, (SubIndex.Numerator, 1) },
        { (11, -20), 0, (SubIndex.Denominator, 1) },
        { (11, 0), 0, (SubIndex.Denominator, 1) },
        { (11, 8), 0, (SubIndex.Numerator, 1) },
        { (11, 40), 0, (SubIndex.Numerator, 1) },
        { (13, -20), 1 },
        { (13, 0), 1 },
        { (13, 8), 1 },
        { (13, 40), 1 },
        { (15, -20), 1 },
        { (15, 0), 1 },
        { (15, 8), 1 },
        { (15, 40), 1 },
      };
    static readonly ListDisplay FractionPlusRegular = CreateDisplay(@"\frac32+1");
    [Theory, MemberData(nameof(FractionPlusRegularData))]
    public void FractionPlusRegularTest(PointF point, MathListIndex expected) =>
      Test(FractionPlusRegular, point, expected);

    public static TestData ExponentData =>
      new TestData {
        { (-10, -20), 0 },
        { (-10, 0), 0 },
        { (-10, 8), 0 },
        { (-10, 40), 0 },
        { (0, -20), 0 },
        { (0, 0), 0 },
        { (0, 8), 0 },
        { (0, 40), 0 },
        { (9, -20), 0, (SubIndex.Nucleus, 1) },
        { (9, 0), 0, (SubIndex.Nucleus, 1) },
        { (9, 8), 0, (SubIndex.Nucleus, 1) },
        // The superscript is closer than the nucleus (and the touch boundaries overlap)
        { (9, 40), 0, (SubIndex.Superscript, 0) },
        { (10, -20), 0, (SubIndex.Nucleus, 1) },
        { (10, 0), 0, (SubIndex.Nucleus, 1) },
        // The nucleus is closer and the touch boundaries overlap
        { (10, 8), 0, (SubIndex.Nucleus, 1) },
        { (10, 40), 0, (SubIndex.Superscript, 0) },
        { (11, -20), 0, (SubIndex.Nucleus, 1) },
        { (11, 0), 0, (SubIndex.Nucleus, 1) },
        { (11, 8), 0, (SubIndex.Superscript, 0) },
        { (11, 40), 0, (SubIndex.Superscript, 0) },
        { (17, -20), 1 },
        { (17, 0), 1 },
        { (17, 8), 0, (SubIndex.Superscript, 1) },
        { (17, 40), 0, (SubIndex.Superscript, 1) },
        { (30, -20), 1 },
        { (30, 0), 1 },
        { (30, 8), 1 },
        { (30, 40), 1 },
      };
    static readonly ListDisplay Exponent = CreateDisplay("2^3");
    [Theory, MemberData(nameof(ExponentData))]
    public void ExponentTest(PointF point, MathListIndex expected) => Test(Exponent, point, expected);

    // https://github.com/verybadcat/CSharpMath/issues/49
    public static TestData Exponent2Data =>
      new TestData {
        { (55, 0), 1 },
        { (55, 20), 1 },
        { (55, 40), 1 },
  };
    static readonly ListDisplay Exponent2 = CreateDisplay("2^{x+y-4}");
    [Theory, MemberData(nameof(Exponent2Data))]
    public void Exponent2Test(PointF point, MathListIndex expected) => Test(Exponent2, point, expected);

    // https://github.com/verybadcat/CSharpMath/issues/46
    public static TestData Issue46Data =>
      new TestData {
        { (50, 10), 4 },
        { (90, 0), 5 },
        { (90, 20), 5 },
        { (90, 40), 5 },
      };
    static readonly ListDisplay Issue46 = CreateDisplay("2+x+x^y");
    [Theory, MemberData(nameof(Issue46Data))]
    public void Issue46Test(PointF point, MathListIndex expected) => Test(Issue46, point, expected);

    public static TestData ComplexData =>
      new TestData {
        { (-10, -20), 0 },
        { (-10, 0), 0 },
        { (-10, 8), 0 },
        { (-10, 40), 0 },
        // \frac a\frac bc
        { (0, -20), 0, (SubIndex.Denominator, 0), (SubIndex.Denominator, 0) },
        { (0, 0), 0, (SubIndex.Denominator, 0), (SubIndex.Numerator, 0) },
        { (0, 8), 0, (SubIndex.Numerator, 0) },
        { (0, 40), 0, (SubIndex.Numerator, 0) },
        { (9, -20), 0, (SubIndex.Denominator, 1) },
        { (9, 0), 0, (SubIndex.Denominator, 0), (SubIndex.Numerator, 1) },
        { (9, 8), 0, (SubIndex.Numerator, 1) },
        { (9, 40), 0, (SubIndex.Numerator, 1) },
        { (10, -20), 0, (SubIndex.Numerator, 1) },
        { (10, 0), 0, (SubIndex.Denominator, 0), (SubIndex.Numerator, 1) },
        { (10, 8), 0, (SubIndex.Numerator, 1) },
        { (10, 40), 0, (SubIndex.Numerator, 1) },
        // v WIP!! Fails currently 
        { (11, -20), 0, (SubIndex.Nucleus, 1) },
        { (11, 0), 0, (SubIndex.Nucleus, 1) },
        { (11, 8), 0, (SubIndex.Superscript, 0) },
        { (11, 40), 0, (SubIndex.Superscript, 0) },
        { (17, -20), 1 },
        { (17, 0), 1 },
        { (17, 8), 0, (SubIndex.Superscript, 1) },
        { (17, 40), 0, (SubIndex.Superscript, 1) },
        { (30, -20), 1 },
        { (30, 0), 1 },
        { (30, 8), 1 },
        { (30, 40), 1 },
      };
    static readonly ListDisplay Complex =
      CreateDisplay(@"\frac a\frac bc\frac\frac123\sqrt d^e\sqrt[5]6\sqrt[6]7_8\overline9\underline0");
    [Theory, MemberData(nameof(ComplexData), Skip = "Not yet done...")]
    public void ComplexTest(PointF point, MathListIndex expected) => Test(Complex, point, expected);
  }
}