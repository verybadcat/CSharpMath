using CSharpMath.Atoms;
using CSharpMath.Display;
using CSharpMath.Enumerations;

namespace CSharpMath.FrontEnd {
  public static class TypesettingContextExtensions {
    public static ListDisplay<TFont, TGlyph> CreateLine<TFont, TGlyph>
      (this TypesettingContext<TFont, TGlyph> context, MathList list, TFont font, LineStyle style)
      where TFont : IFont<TGlyph> => Typesetter<TFont, TGlyph>.CreateLine(list, font, context, style);
    internal static string ChangeFont<TFont, TGlyph>
      (this TypesettingContext<TFont, TGlyph> context, string input, FontStyle outputFontStyle)
      where TFont : IFont<TGlyph> => context.FontChanger.ChangeFont(input, outputFontStyle);
  }
}
