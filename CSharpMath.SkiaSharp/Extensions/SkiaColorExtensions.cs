using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Atoms;
using NColor = SkiaSharp.SKColor;

namespace CSharpMath.SkiaSharp {
  public static class SkiaColorExtensions {

    private static byte _fromHex(string hex)
      => Convert.ToByte(hex.Substring(0, 2), 16);
    public static NColor? From6DigitHexString(string hex) {
      if (hex == null) return null;
      hex = hex.RemovePrefix("#").RemovePrefix("0x");
      var red = _fromHex(hex);
      var green = _fromHex(hex.Substring(2));
      var blue = _fromHex(hex.Substring(4));
      return new NColor(red, green, blue);
    }


    public static NColor? ToNative(this MathColor mathColor)
      => From6DigitHexString(mathColor?.ColorString);
  }
}
