using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Structures;
using NColor = SkiaSharp.SKColor;

namespace CSharpMath.SkiaSharp {
  public static class SkiaColorExtensions {
    public static NColor? ToNative(this MathColor mathColor) {
      if (string.IsNullOrWhiteSpace(mathColor?.ColorString)) return null;
      return NColor.TryParse(mathColor.ColorString, out var c) ? c : default(NColor?);
    }
    public static NColor? ToNative(this Color? color) => color.HasValue ? new NColor(color.Value.R, color.Value.G, color.Value.B, color.Value.A) : default(NColor?);
  }
}
