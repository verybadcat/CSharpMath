using CSharpMath.Interfaces;
using CSharpMath.Display;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Display.Text;

namespace CSharpMath.FrontEnd {
  public static class TypesettingContextExtensions {
    public static MathListDisplay CreateLine<TGlyph>(this TypesettingContext<TGlyph> context, IMathList list, MathFont<TGlyph> font, LineStyle style) {
      return Typesetter<TGlyph>.CreateLine(list, font, context, style);
      }
  }
}
