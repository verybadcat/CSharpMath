using TGlyph = System.UInt16;
using Foundation;
using CoreGraphics;
using CoreText;
using CSharpMath.Display;

namespace CSharpMath.Apple
{
  /// <remarks>Corresponds to MTFont in iosMath.</remarks>
  public class AppleMathFont: MathFont<TGlyph>
  {
    public CGFont DefaultCgFont { get; private set; }
    public CTFont CtFont { get; private set; }
    public string Name { get; private set; }
    private AppleMathFont(float pointSize): base(pointSize){}
    public AppleMathFont(string name, float size): this(size)
    {
      Name = name;
      NSBundle bundle = NSBundle.MainBundle;
      string fontPath = bundle.PathForResource(name, "otf");
      using (CGDataProvider fontDataProvider = CGDataProvider.FromFile(fontPath))
      {
        DefaultCgFont = CGFont.CreateFromProvider(fontDataProvider);
      }
      CtFont = new CTFont(DefaultCgFont, size, CGAffineTransform.MakeIdentity());
    }

    public AppleMathFont(AppleMathFont cloneMe, float pointSize): this(pointSize) {
      Name = cloneMe.Name;
      DefaultCgFont = cloneMe.DefaultCgFont;
      CtFont = new CTFont(DefaultCgFont, pointSize, CGAffineTransform.MakeIdentity());
    }
  }
}
