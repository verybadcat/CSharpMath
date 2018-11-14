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
using TestData = Xunit.TheoryData<System.Drawing.PointF, CSharpMath.Editor.MathListIndex>;

public class DisplayEditingTests {
  class EqualsException : Xunit.Sdk.EqualException {
    public EqualsException(object expected, object actual, string message) : base(expected, actual) =>
      UserMessage = message;
    public override string Message =>
      UserMessage + $"\nExpected: {Expected?.ToString() ?? "(null)"}\nActual: {Actual?.ToString() ?? "(null)"}";
  }

  public const float FontSize = 20;
  public static readonly TestFont Font = new TestFont(FontSize);
  private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;

  static ListDisplay CreateDisplay(string expr) => 
    Typesetter<TestFont, char>.CreateLine(MathLists.FromString(expr), Font, context, LineStyle.Display);

  void TestClosestPointForExpression(ListDisplay displayList, PointF point, MathListIndex expected) {
    var index = displayList.IndexForPoint(context, point);
    if (!expected.EqualsToIndex(index))
      //Xunit disallows custom user messages
      throw new EqualsException(expected, index, $"Point: {point}");
  }

  public static TestData FractionTestData =>
    new TestData {
      { new PointF(-10, 8), MathListIndex.Level0Index(0) },
      { new PointF(-10, 0), MathListIndex.Level0Index(0) },
      { new PointF(-10, 40), MathListIndex.Level0Index(0) },
      { new PointF(-10, -20), MathListIndex.Level0Index(0) },
      { new PointF(-2.5f, 8), MathListIndex.Level0Index(0) },
      { new PointF(-2.5f, 0), MathListIndex.Level0Index(0) },
      { new PointF(-2.5f, 40), MathListIndex.Level0Index(0) },
      { new PointF(-2.5f, -20), MathListIndex.Level0Index(0) },
      { new PointF(-1, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(-1, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(-1, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(-1, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(3, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(3, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(3, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(3, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(7, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
      { new PointF(7, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(7, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(7, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
      { new PointF(11, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
      { new PointF(11, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(11, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(11, -20), MathListIndex.Level0Index(1) },  // because it is below the height of the fraction

      { new PointF(12.5f, 8), MathListIndex.Level0Index(1) },
      { new PointF(12.5f, 0), MathListIndex.Level0Index(1) },
      { new PointF(12.5f, 40), MathListIndex.Level0Index(1) },
      { new PointF(12.5f, -20), MathListIndex.Level0Index(1) },
      { new PointF(20, 8), MathListIndex.Level0Index(1) },
      { new PointF(20, 0), MathListIndex.Level0Index(1) },
      { new PointF(20, 40), MathListIndex.Level0Index(1) },
      { new PointF(20, -20), MathListIndex.Level0Index(1) }
   };

  static readonly ListDisplay Fraction = CreateDisplay(@"\frac32");

  [Theory, MemberData(nameof(FractionTestData))]
  public void TestClosestPointFraction(PointF point, MathListIndex expected) =>
    TestClosestPointForExpression(Fraction, point, expected);

  public static TestData RegularTestData =>
    new TestData {
      { new PointF(-10, 8), MathListIndex.Level0Index(0) },
      { new PointF(-10, 0), MathListIndex.Level0Index(0) },
      { new PointF(-10, 40), MathListIndex.Level0Index(0) },
      { new PointF(-10, -20), MathListIndex.Level0Index(0) },
      { new PointF(0, 0), MathListIndex.Level0Index(0) },
      { new PointF(0, 8), MathListIndex.Level0Index(0) },
      { new PointF(0, 40), MathListIndex.Level0Index(0) },
      { new PointF(0, -20), MathListIndex.Level0Index(0) },
      { new PointF(10, 0), MathListIndex.Level0Index(1) },
      { new PointF(10, 8), MathListIndex.Level0Index(1) },
      { new PointF(10, 40), MathListIndex.Level0Index(1) },
      { new PointF(10, -20), MathListIndex.Level0Index(1) },
      { new PointF(15, 0), MathListIndex.Level0Index(1) },
      { new PointF(15, 8), MathListIndex.Level0Index(1) },
      { new PointF(15, 40), MathListIndex.Level0Index(1) },
      { new PointF(15, -20), MathListIndex.Level0Index(1) },
      { new PointF(25, 0), MathListIndex.Level0Index(2) },
      { new PointF(25, 8), MathListIndex.Level0Index(2) },
      { new PointF(25, 40), MathListIndex.Level0Index(2) },
      { new PointF(25, -20), MathListIndex.Level0Index(2) },
      { new PointF(35, 0), MathListIndex.Level0Index(2) },
      { new PointF(35, 8), MathListIndex.Level0Index(2) },
      { new PointF(35, 40), MathListIndex.Level0Index(2) },
      { new PointF(35, -20), MathListIndex.Level0Index(2) },
      { new PointF(45, 0), MathListIndex.Level0Index(3) },
      { new PointF(45, 8), MathListIndex.Level0Index(3) },
      { new PointF(45, 40), MathListIndex.Level0Index(3) },
      { new PointF(45, -20), MathListIndex.Level0Index(3) },
      { new PointF(55, 0), MathListIndex.Level0Index(3) },
      { new PointF(55, 8), MathListIndex.Level0Index(3) },
      { new PointF(55, 40), MathListIndex.Level0Index(3) },
      { new PointF(55, -20), MathListIndex.Level0Index(3) }
    };

  static readonly ListDisplay Regular = CreateDisplay(@"4+2");

  [Theory, MemberData(nameof(RegularTestData))]
  public void TestClosestPointRegular(PointF point, MathListIndex expected) =>
    TestClosestPointForExpression(Regular, point, expected);

  public static TestData RegularPlusFractionTestData =>
    new TestData {
      { new PointF(30, 0), MathListIndex.Level0Index(2) },
      { new PointF(30, 8), MathListIndex.Level0Index(2) },
      { new PointF(30, 40), MathListIndex.Level0Index(2) },
      { new PointF(30, -20), MathListIndex.Level0Index(2) },
      { new PointF(32, 0), MathListIndex.Level0Index(2) },
      { new PointF(32, 8), MathListIndex.Level0Index(2) },
      { new PointF(32, 40), MathListIndex.Level0Index(2) },
      { new PointF(32, -20), MathListIndex.Level0Index(2) },
      { new PointF(33, 0), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(33, 8), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(33, 40), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(33, -20), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(35, 0), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) },
      { new PointF(35, 8), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(35, 40), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Numerator) },
      { new PointF(35, -20), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(0), MathListSubIndexType.Denominator) }
    };

  static readonly ListDisplay RegularPlusFraction = CreateDisplay(@"1+\frac{3}{2}");

  [Theory, MemberData(nameof(RegularPlusFractionTestData))]
  public void TestClosestPointRegularPlusFraction(PointF point, MathListIndex expected) =>
      TestClosestPointForExpression(RegularPlusFraction, point, expected);

  public static TestData FractionPlusRegularTestData =>
    new TestData {
      { new PointF(15, 0), MathListIndex.Level0Index(1) },
      { new PointF(15, 8), MathListIndex.Level0Index(1) },
      { new PointF(15, 40), MathListIndex.Level0Index(1) },
      { new PointF(15, -20), MathListIndex.Level0Index(1) },
      { new PointF(13, 0), MathListIndex.Level0Index(1) },
      { new PointF(13, 8), MathListIndex.Level0Index(1) },
      { new PointF(13, 40), MathListIndex.Level0Index(1) },
      { new PointF(13, -20), MathListIndex.Level0Index(1) },
      { new PointF(11, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
      { new PointF(11, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(11, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(11, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
      { new PointF(9, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
      { new PointF(9, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(9, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
      { new PointF(9, -20) , MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) }
    };

  static readonly ListDisplay FractionPlusRegular = CreateDisplay(@"\frac32+1");

  [Theory, MemberData(nameof(FractionPlusRegularTestData))]
  public void TestClosestPointFractionPlusRegular(PointF point, MathListIndex expected) =>
    TestClosestPointForExpression(FractionPlusRegular, point, expected);

  public static TestData ExponentTestData =>
    new TestData {
      { new PointF(-10, 8), MathListIndex.Level0Index(0) },
      { new PointF(-10, 0), MathListIndex.Level0Index(0) },
      { new PointF(-10, 40), MathListIndex.Level0Index(0) },
      { new PointF(-10, -20), MathListIndex.Level0Index(0) },
      { new PointF(0, 0), MathListIndex.Level0Index(0) },
      { new PointF(0, 8), MathListIndex.Level0Index(0) },
      { new PointF(0, 40), MathListIndex.Level0Index(0) },
      { new PointF(0, -20), MathListIndex.Level0Index(0) },
      { new PointF(9, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      { new PointF(9, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      // The superscript is closer than the nucleus (and the touch boundaries overlap)
      { new PointF(9, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Superscript) },
      { new PointF(9, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      { new PointF(10, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      // The nucleus is closer and the touch boundaries overlap
      { new PointF(10, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      { new PointF(10, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Superscript) },
      { new PointF(10, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      { new PointF(11, 0), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      { new PointF(11, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Superscript) },
      { new PointF(11, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(0), MathListSubIndexType.Superscript) },
      { new PointF(11, -20), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus) },
      { new PointF(17, 0), MathListIndex.Level0Index(1) },
      { new PointF(17, 8), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Superscript) },
      { new PointF(17, 40), MathListIndex.IndexAtLocation(0, MathListIndex.Level0Index(1), MathListSubIndexType.Superscript) },
      { new PointF(17, -20), MathListIndex.Level0Index(1) },
      { new PointF(30, 0), MathListIndex.Level0Index(1) },
      { new PointF(30, 8), MathListIndex.Level0Index(1) },
      { new PointF(30, 40), MathListIndex.Level0Index(1) },
      { new PointF(30, -20), MathListIndex.Level0Index(1) },
    };

  static readonly ListDisplay Exponent = CreateDisplay("2^3");

  [Theory, MemberData(nameof(ExponentTestData))]
  public void TestClosestPointExponent(PointF point, MathListIndex expected) =>
    TestClosestPointForExpression(Exponent, point, expected);
}