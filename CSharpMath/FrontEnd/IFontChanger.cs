using System;
namespace CSharpMath.FrontEnd
{
  using Enumerations;
  public interface IFontChanger
  {
    /// <summary>Changes glyphs among the various font styles -- italic, regular, etc. If you are
    /// implementing a front end with a unicode font, you may be able to re-use
    /// UnicodeGlyphFinder for your implementation of this.</summary>
    string ChangeFont(string inputString, FontStyle outputFontStyle);
  }
}
