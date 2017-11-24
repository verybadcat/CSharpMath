using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace CSharpMath {
  public static class ColorExtensions {

    private static byte _fromHex(string hex)
      => Convert.ToByte(hex.Substring(0, 2), 16);
    public static Color From6DigitHexString(string hex) {
      hex = hex.RemovePrefix("#").RemovePrefix("0x");
      var red = _fromHex(hex);
      var green = _fromHex(hex.Substring(2));
      var blue = _fromHex(hex.Substring(4));
      return Color.FromArgb(red, green, blue);
      
    }
  }
}
