using CSharpMath.Interfaces;
using CSharpMath.Display;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Display.Text;

namespace CSharpMath.FrontEnd {
  public static class TypesettingContextExtensions {
    public static MathListDisplay CreateLine<GlyphType>(this TypesettingContext<GlyphType> context, IMathList list, MathFont font, LineStyle style) {
      return Typesetter<GlyphType>.CreateLine(list, font, context, style);
      }
  }
}
