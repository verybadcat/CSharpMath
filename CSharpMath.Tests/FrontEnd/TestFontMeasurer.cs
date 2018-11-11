using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display;

namespace CSharpMath.Tests.FrontEnd {
  public class TestFontMeasurer : IFontMeasurer<TestFont, char> {
    TestFontMeasurer() { }
    public static TestFontMeasurer Instance { get; } = new TestFontMeasurer();

    public int GetUnitsPerEm(TestFont font) => 1000;
  }
}
