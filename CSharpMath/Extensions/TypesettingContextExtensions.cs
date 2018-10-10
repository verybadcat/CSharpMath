using CSharpMath.Interfaces;
using CSharpMath.Display;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Display.Text;

namespace CSharpMath.FrontEnd {
  public static class TypesettingContextExtensions {
    public static ListDisplay<TFont, TGlyph> CreateLine<TFont, TGlyph>(this TypesettingContext<TFont, TGlyph> context, IMathList list, TFont font, LineStyle style)
      where TFont: IFont<TGlyph> {
      return Typesetter<TFont, TGlyph>.CreateLine(list, font, context, style);
      }
    internal static string ChangeFont<TFont, TGlyph>(this TypesettingContext<TFont, TGlyph> context, string input, FontStyle outputFontStyle) 
      where TFont: IFont<TGlyph> {
      var changer = context.FontChanger;
      return changer.ChangeFont(input, outputFontStyle);
    }
  }
}
