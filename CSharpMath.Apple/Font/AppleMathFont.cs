using CoreGraphics;
using CoreText;
using CSharpMath.Display;

namespace CSharpMath.Apple {
  /// <remarks>Corresponds to MTFont in iosMath.</remarks>
  public struct AppleMathFont : IFont<ushort> {
    public float PointSize { get; }
    public CGFont CgFont { get; private set; }
    public CTFont CtFont { get; private set; }
    public string Name { get; private set; }
    internal AppleMathFont(string name, CGFont cgFont, float size) {
      PointSize = size;
      Name = name;
      CgFont = cgFont;
      CtFont = new CTFont(CgFont, size, CGAffineTransform.MakeIdentity());
    }
    public AppleMathFont(AppleMathFont cloneMe, float pointSize) {
      PointSize = pointSize;
      Name = cloneMe.Name;
      CgFont = cloneMe.CgFont;
      CtFont = new CTFont(CgFont, pointSize, CGAffineTransform.MakeIdentity());
    }
  }
}
