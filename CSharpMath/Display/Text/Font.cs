using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display.Text {
  public class Font {
    internal FontMathTable MathTable { get; }
    public float PointSize { get; private set; }
    public FontStyle Style { get; private set; }
  }
}
