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

namespace CSharpMath.Editor.Tests {
  // Use the "CSharpMath.Editor Test Checker" project in the _Utils folder to visualize the test cases
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

    void Test(ListDisplay displayList, PointF point, MathListIndex expected) {
      var index = displayList.IndexForPoint(context, point);
      if (!expected.EqualsToIndex(index))
        //Xunit disallows custom user messages
        throw new NotEqual(expected, index, $"\nPoint: {point}");
    }

    // ^ Helper functions
    // v Actual test cases

    public static TestData FractionData =>
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
    [Theory, MemberData(nameof(FractionData))]
    public void FractionTest(PointF point, MathListIndex expected) => Test(Fraction, point, expected);

    public static TestData RegularData =>
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
        { new PointF(35, 0), MathListIndex.Level0Index(3) },
        { new PointF(35, 8), MathListIndex.Level0Index(3) },
        { new PointF(35, 40), MathListIndex.Level0Index(3) },
        { new PointF(35, -20), MathListIndex.Level0Index(3) },
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
    [Theory, MemberData(nameof(RegularData))]
    public void RegularTest(PointF point, MathListIndex expected) => Test(Regular, point, expected);

    public static TestData RegularPlusFractionData =>
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
        { new PointF(35, 0), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) },
        { new PointF(35, 8), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
        { new PointF(35, 40), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(1), MathListSubIndexType.Numerator) },
        { new PointF(35, -20), MathListIndex.IndexAtLocation(2, MathListIndex.Level0Index(1), MathListSubIndexType.Denominator) }
      };
    static readonly ListDisplay RegularPlusFraction = CreateDisplay(@"1+\frac{3}{2}");
    [Theory, MemberData(nameof(RegularPlusFractionData))]
    public void RegularPlusFractionTest(PointF point, MathListIndex expected) => Test(RegularPlusFraction, point, expected);

    public static TestData FractionPlusRegularData =>
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
    [Theory, MemberData(nameof(FractionPlusRegularData))]
    public void FractionPlusRegularTest(PointF point, MathListIndex expected) => Test(FractionPlusRegular, point, expected);

    public static TestData ExponentData =>
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
    [Theory, MemberData(nameof(ExponentData))]
    public void ExponentTest(PointF point, MathListIndex expected) => Test(Exponent, point, expected);

    // \frac a\frac bc\frac\frac123\sqrt d^e\sqrt[5]6\sqrt[6f]7_8\overline9\underline0
  }
}