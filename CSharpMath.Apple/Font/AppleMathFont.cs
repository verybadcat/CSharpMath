using TGlyph = System.UInt16;
using Foundation;
using CoreGraphics;
using CoreText;
using CSharpMath.Display;
using System;

namespace CSharpMath.Apple
{
  /// <remarks>Corresponds to MTFont in iosMath.</remarks>
  public class AppleMathFont: MathFont<TGlyph>
  {
    public CGFont CgFont { get; private set; }
    public CTFont CtFont { get; private set; }
    public string Name { get; private set; }
    private AppleMathFont(float pointSize): base(pointSize){}
    internal AppleMathFont(string name, CGFont cgFont, float size): this(size)
    {
      Name = name;
      CgFont = cgFont;
      var transform = CGAffineTransform.MakeIdentity();
      CtFont = new CTFont(CgFont, size, transform);
    }

    public AppleMathFont(AppleMathFont cloneMe, float pointSize): this(pointSize) {
      Name = cloneMe.Name;
      CgFont= cloneMe.CgFont;
      CtFont = new CTFont(CgFont, pointSize, CGAffineTransform.MakeIdentity());
    }
  }
}
