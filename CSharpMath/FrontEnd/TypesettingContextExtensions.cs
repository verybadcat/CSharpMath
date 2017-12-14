using CSharpMath.Interfaces;
using CSharpMath.Display;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Display.Text;

namespace CSharpMath.FrontEnd {
  public static class TypesettingContextExtensions {
    public static MathListDisplay<TFont, TGlyph> CreateLine<TFont, TGlyph>(this TypesettingContext<TFont, TGlyph> context, IMathList list, TFont font, LineStyle style)
      where TFont: MathFont<TGlyph> {
      return Typesetter<TFont, TGlyph>.CreateLine(list, font, context, style);
      }
  }
}
