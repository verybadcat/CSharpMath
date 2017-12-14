using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display;

namespace CSharpMath.Tests.FrontEnd {
  public class TestFontMeasurer : IFontMeasurer<MathFont<char>, char> {
    public int GetUnitsPerEm(MathFont<char> font) => 1000;
  }
}
