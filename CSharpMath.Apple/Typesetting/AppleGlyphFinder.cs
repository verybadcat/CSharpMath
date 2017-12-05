using System;
using CoreText;
using CSharpMath.FrontEnd;
using Foundation;
using TGlyph = System.UInt16;
namespace CSharpMath.Apple
{
  public class AppleGlyphFinder: IGlyphFinder<TGlyph>
  {
    private readonly CTFont _font;

    public AppleGlyphFinder(CTFont font) {
      _font = font;
    }
    public ushort FindGlyphForCharacterAtIndex(int index, string str)
    {
      throw new NotImplementedException();
      //var nsString = new NSString(str);
      //var range = nsString.Ra
    }

    public ushort[] FindGlyphs(string str)
    {
      throw new NotImplementedException();
    }
  }
}
