using System;
using CSharpMath.Display.Text;
using TGlyph = System.UInt16;
using CoreText;

namespace CSharpMath.Apple {
  public class AppleMathFont : MathFont<TGlyph> {
    public string Name { get; }
    public CTFont MyCTFont { get; }
    public AppleMathFont(string name, float pointSize): base(pointSize) {
      Name = name;
      MyCTFont = new CTFont(name, pointSize);
    }
  }
}
