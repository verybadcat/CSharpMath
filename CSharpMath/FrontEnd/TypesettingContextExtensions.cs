using CSharpMath.Interfaces;
using CSharpMath.Display;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Display.Text;

namespace CSharpMath.FrontEnd {
  public static class TypesettingContextExtensions {
    public static MathListDisplay CreateLine<TMathFont, TGlyph>(this TypesettingContext<TMathFont, TGlyph> context, IMathList list, TMathFont font, LineStyle style)
      where TMathFont: MathFont<TGlyph> {
      return Typesetter<TMathFont, TGlyph>.CreateLine(list, font, context, style);
      }
  }
}
