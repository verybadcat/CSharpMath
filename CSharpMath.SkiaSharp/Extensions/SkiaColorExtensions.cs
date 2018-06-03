using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Atoms;
using NColor = SkiaSharp.SKColor;

namespace CSharpMath.SkiaSharp {
  public static class SkiaColorExtensions {
    public static NColor? ToNative(this MathColor mathColor) {
      if (mathColor.ColorString == null) return null;
      return NColor.TryParse(mathColor.ColorString, out var c) ? c : default(NColor?);
    }
  }
}
