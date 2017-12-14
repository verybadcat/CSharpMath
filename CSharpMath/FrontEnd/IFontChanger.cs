using System;
namespace CSharpMath.FrontEnd
{
  public interface IFontChanger<TGlyph>
  {
    TGlyph[] ChangeFont(TGlyph inputGlyphs, FontStyle outputFontStyle);
  }
}
