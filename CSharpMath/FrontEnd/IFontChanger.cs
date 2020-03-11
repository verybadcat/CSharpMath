namespace CSharpMath.FrontEnd {
  /// <summary>Changes glyphs among the various font styles -- italic, regular, etc.
  /// If you are implementing a front end with a unicode font, you may be able to re-use
  /// UnicodeGlyphFinder for your implementation of this.</summary>
  public interface IFontChanger {
    int StyleCharacter(char c, Atoms.FontStyle fontStyle);
  }
}
namespace CSharpMath {
  partial class Extensions {
    public static string ChangeFont(this FrontEnd.IFontChanger fontChanger,
      string inputString, Atoms.FontStyle outputFontStyle) {
      var builder = new System.Text.StringBuilder();
      foreach (var c in inputString) {
        int unicode = fontChanger.StyleCharacter(c, outputFontStyle);
        builder.Append(char.IsSurrogate(c)
          ? ((char)unicode).ToStringInvariant() : char.ConvertFromUtf32(unicode));
      }
      return builder.ToString();
    }
  }
}