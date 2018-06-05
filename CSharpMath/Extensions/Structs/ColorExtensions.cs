using CSharpMath.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace CSharpMath {
  public static class ColorExtensions {

    private static byte _fromHex(string hex, int index, int length)
      => Convert.ToByte(hex.Substring(index, length), 16);
    public static Color? FromHexString(string hex) {
      if (hex == null) return null;
      hex = hex.RemovePrefix("#").RemovePrefix("0x");
      if (hex.Length == 3)
        return (_fromHex(hex, 0, 1), _fromHex(hex, 1, 1), _fromHex(hex, 2, 1));
      else if (hex.Length == 6) {
        var red = _fromHex(hex, 0, 2);
        var green = _fromHex(hex, 2, 2);
        var blue = _fromHex(hex, 4, 2);
        return (red, green, blue);
      } else return null;
    }

  }
}
