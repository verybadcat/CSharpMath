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
using TestData = Xunit.TheoryData<float, float, CSharpMath.Editor.MathListIndex>;

namespace CSharpMath.Editor.Tests {
  // Use the "CSharpMath.Editor Test Checker" project in the _Utils folder to visualize the test cases
  using Type = MathListSubIndexType;
  using static MathListIndex;
  public class ClosestPointTests {
    class NotEqual : Xunit.Sdk.EqualException {
      public NotEqual(object expected, object actual, string message) : base(expected, actual) => UserMessage = message;
      public override string Message =>
        UserMessage + $"\nExpected: {Expected?.ToString() ?? "(null)"}\nActual: {Actual?.ToString() ?? "(null)"}";
    }

    public static readonly TestFont Font = new TestFont(20);
    private static readonly TypesettingContext<TestFont, char> context = TestTypesettingContexts.Instance;

    public static ListDisplay CreateDisplay(string expr) =>
      MathLists.FromString(expr) is Interfaces.IMathList l ?
      Typesetter<TestFont, char>.CreateLine(l, Font, context, LineStyle.Display) :
      null;

    void Test(ListDisplay displayList, float x, float y, MathListIndex expected) {
      var point = new PointF(x, y);
      var index = displayList.IndexForPoint(context, point);
      if (!expected.EqualsToIndex(index))
        //Xunit disallows custom user messages
        throw new NotEqual(expected, index, $"\nPoint: {point}");
    }

    // ^ Helper functions
    // v Actual test cases

    public static TestData FractionData =>
      new TestData {
        { -10, -20, Level0Index(0) },
        { -10, 0, Level0Index(0) },
        { -10, 8, Level0Index(0) },
        { -10, 40, Level0Index(0) },
        { -2.5f, -20, Level0Index(0) },
        { -2.5f, 0, Level0Index(0) },
        { -2.5f, 8, Level0Index(0) },
        { -2.5f, 40, Level0Index(0) },
        { -1, -20, IndexAtLocation(0, Type.Denominator, Level0Index(0)) },
        { -1, 0, IndexAtLocation(0, Type.Denominator, Level0Index(0)) },
        { -1, 8, IndexAtLocation(0, Type.Numerator, Level0Index(0)) },
        { -1, 40, IndexAtLocation(0, Type.Numerator, Level0Index(0)) },
        { 3, -20, IndexAtLocation(0, Type.Denominator, Level0Index(0)) },
        { 3, 0, IndexAtLocation(0, Type.Denominator, Level0Index(0)) },
        { 3, 8, IndexAtLocation(0, Type.Numerator, Level0Index(0)) },
        { 3, 40, IndexAtLocation(0, Type.Numerator, Level0Index(0)) },
        { 7, -20, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 7, 0, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 7, 8, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 7, 40, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 11, -20, Level0Index(1) },  // because it is below the height of the fraction
        { 11, 0, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 11, 8, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 11, 40, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },

        { 12.5f, -20, Level0Index(1) },
        { 12.5f, 0, Level0Index(1) },
        { 12.5f, 8, Level0Index(1) },
        { 12.5f, 40, Level0Index(1) },
        { 20, -20, Level0Index(1) },
        { 20, 0, Level0Index(1) },
        { 20, 8, Level0Index(1) },
        { 20, 40, Level0Index(1) },
       };
    static readonly ListDisplay Fraction = CreateDisplay(@"\frac32");
    [Theory, MemberData(nameof(FractionData))]
    public void FractionTest(float x, float y, MathListIndex expected) => Test(Fraction, x, y, expected);

    public static TestData RegularData =>
      new TestData {
        { -10, -20, Level0Index(0) },
        { -10, 0, Level0Index(0) },
        { -10, 8, Level0Index(0) },
        { -10, 40, Level0Index(0) },
        { 0, -20, Level0Index(0) },
        { 0, 0, Level0Index(0) },
        { 0, 8, Level0Index(0) },
        { 0, 40, Level0Index(0) },
        { 10, -20, Level0Index(1) },
        { 10, 0, Level0Index(1) },
        { 10, 8, Level0Index(1) },
        { 10, 40, Level0Index(1) },
        { 15, -20, Level0Index(1) },
        { 15, 0, Level0Index(1) },
        { 15, 8, Level0Index(1) },
        { 15, 40, Level0Index(1) },
        { 25, -20, Level0Index(2) },
        { 25, 0, Level0Index(2) },
        { 25, 8, Level0Index(2) },
        { 25, 40, Level0Index(2) },
        { 35, -20, Level0Index(3) },
        { 35, 0, Level0Index(3) },
        { 35, 8, Level0Index(3) },
        { 35, 40, Level0Index(3) },
        { 45, -20, Level0Index(3) },
        { 45, 0, Level0Index(3) },
        { 45, 8, Level0Index(3) },
        { 45, 40, Level0Index(3) },
        { 55, -20, Level0Index(3) },
        { 55, 0, Level0Index(3) },
        { 55, 8, Level0Index(3) },
        { 55, 40, Level0Index(3) },
      };
    static readonly ListDisplay Regular = CreateDisplay(@"4+2");
    [Theory, MemberData(nameof(RegularData))]
    public void RegularTest(float x, float y, MathListIndex expected) => Test(Regular, x, y, expected);

    public static TestData RegularPlusFractionData =>
      new TestData {
        { 30, -20, Level0Index(2) },
        { 30, 0, Level0Index(2) },
        { 30, 8, Level0Index(2) },
        { 30, 40, Level0Index(2) },
        { 32, -20, Level0Index(2) },
        { 32, 0, Level0Index(2) },
        { 32, 8, Level0Index(2) },
        { 32, 40, Level0Index(2) },
        { 33, -20, IndexAtLocation(2, Type.Denominator, Level0Index(0)) },
        { 33, 0, IndexAtLocation(2, Type.Denominator, Level0Index(0)) },
        { 33, 8, IndexAtLocation(2, Type.Numerator, Level0Index(0)) },
        { 33, 40, IndexAtLocation(2, Type.Numerator, Level0Index(0)) },
        { 35, -20, IndexAtLocation(2, Type.Denominator, Level0Index(1)) },
        { 35, 0, IndexAtLocation(2, Type.Denominator, Level0Index(1)) },
        { 35, 8, IndexAtLocation(2, Type.Numerator, Level0Index(1)) },
        { 35, 40, IndexAtLocation(2, Type.Numerator, Level0Index(1)) },
      };
    static readonly ListDisplay RegularPlusFraction = CreateDisplay(@"1+\frac{3}{2}");
    [Theory, MemberData(nameof(RegularPlusFractionData))]
    public void RegularPlusFractionTest(float x, float y, MathListIndex expected) => Test(RegularPlusFraction, x, y, expected);

    public static TestData FractionPlusRegularData =>
      new TestData {
        { 9, -20, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 9, 0, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 9, 8, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 9, 40, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 11, -20, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 11, 0, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 11, 8, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 11, 40, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 13, -20, Level0Index(1) },
        { 13, 0, Level0Index(1) },
        { 13, 8, Level0Index(1) },
        { 13, 40, Level0Index(1) },
        { 15, -20, Level0Index(1) },
        { 15, 0, Level0Index(1) },
        { 15, 8, Level0Index(1) },
        { 15, 40, Level0Index(1) },
      };
    static readonly ListDisplay FractionPlusRegular = CreateDisplay(@"\frac32+1");
    [Theory, MemberData(nameof(FractionPlusRegularData))]
    public void FractionPlusRegularTest(float x, float y, MathListIndex expected) => Test(FractionPlusRegular, x, y, expected);

    public static TestData ExponentData =>
      new TestData {
        { -10, -20, Level0Index(0) },
        { -10, 0, Level0Index(0) },
        { -10, 8, Level0Index(0) },
        { -10, 40, Level0Index(0) },
        { 0, -20, Level0Index(0) },
        { 0, 0, Level0Index(0) },
        { 0, 8, Level0Index(0) },
        { 0, 40, Level0Index(0) },
        { 9, -20, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 9, 0, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 9, 8, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        // The superscript is closer than the nucleus (and the touch boundaries overlap)
        { 9, 40, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 10, -20, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 10, 0, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        // The nucleus is closer and the touch boundaries overlap
        { 10, 8, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 10, 40, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 11, -20, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 11, 0, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 11, 8, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 11, 40, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 17, -20, Level0Index(1) },
        { 17, 0, Level0Index(1) },
        { 17, 8, IndexAtLocation(0, Type.Superscript, Level0Index(1)) },
        { 17, 40, IndexAtLocation(0, Type.Superscript, Level0Index(1)) },
        { 30, -20, Level0Index(1) },
        { 30, 0, Level0Index(1) },
        { 30, 8, Level0Index(1) },
        { 30, 40, Level0Index(1) },
      };
    static readonly ListDisplay Exponent = CreateDisplay("2^3");
    [Theory, MemberData(nameof(ExponentData))]
    public void ExponentTest(float x, float y, MathListIndex expected) => Test(Exponent, x, y, expected);

    public static TestData ComplexData =>
      new TestData {
        { -10, -20, Level0Index(0) },
        { -10, 0, Level0Index(0) },
        { -10, 8, Level0Index(0) },
        { -10, 40, Level0Index(0) },
        // \frac a\frac bc
        { 0, -20, IndexAtLocation(0, Type.Denominator, IndexAtLocation(0, Type.Denominator, Level0Index(0))) },
        { 0, 0, IndexAtLocation(0, Type.Denominator, IndexAtLocation(0, Type.Numerator, Level0Index(0))) },
        { 0, 8, IndexAtLocation(0, Type.Numerator, Level0Index(0)) },
        { 0, 40, IndexAtLocation(0, Type.Numerator, Level0Index(0)) },
        { 9, -20, IndexAtLocation(0, Type.Denominator, Level0Index(1)) },
        { 9, 0, IndexAtLocation(0, Type.Denominator, IndexAtLocation(0, Type.Numerator, Level0Index(1))) },
        { 9, 8, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        { 9, 40, IndexAtLocation(0, Type.Numerator, Level0Index(1)) },
        // v WIP!! Fails currently
        { 10, -20, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 10, 0, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        // The nucleus is closer and the touch boundaries overlap
        { 10, 8, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 10, 40, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 11, -20, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 11, 0, IndexAtLocation(0, Type.Nucleus, Level0Index(1)) },
        { 11, 8, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 11, 40, IndexAtLocation(0, Type.Superscript, Level0Index(0)) },
        { 17, -20, Level0Index(1) },
        { 17, 0, Level0Index(1) },
        { 17, 8, IndexAtLocation(0, Type.Superscript, Level0Index(1)) },
        { 17, 40, IndexAtLocation(0, Type.Superscript, Level0Index(1)) },
        { 30, -20, Level0Index(1) },
        { 30, 0, Level0Index(1) },
        { 30, 8, Level0Index(1) },
        { 30, 40, Level0Index(1) },
      };
    static readonly ListDisplay Complex =
      CreateDisplay(@"\frac a\frac bc\frac\frac123\sqrt d^e\sqrt[5]6\sqrt[6f]7_8\overline9\underline0");
    [Theory, MemberData(nameof(ComplexData))]
    public void ComplexTest(float x, float y, MathListIndex expected) => Test(Complex, x, y, expected);
  }
}