using System.Drawing;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.ComTypes;

namespace CSharpMath.Structures {
  public static class ColorExtensions {
    //public override string ToString() =>
    //  PredefinedColors.TryGetBySecond(this, out var name)
    //  ? name : $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    public static Color? ParseColor(string? hexOrName) {
      if (hexOrName == null) return null;
      if (hexOrName.StartsWith("#", StringComparison.InvariantCulture))
        return FromHexString(hexOrName.Substring(1));
      if (hexOrName.StartsWith("0x", StringComparison.InvariantCulture))
        return FromHexString(hexOrName);
#pragma warning disable CA1308 // Normalize strings to uppercase
      string loweredName = hexOrName.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
      if (PredefinedColors.TryGetByFirst(loweredName.ToStringInvariant(), out var predefined))
        return predefined;
      return null;
      static Color? FromHexString(string hex) {
#pragma warning disable CA1305 // Specify IFormatProvider
        if (hex.Length > 7) {
          return Color.FromArgb(int.Parse(hex, NumberStyles.HexNumber));
        } else {
          var c = Color.FromArgb(int.Parse(hex, NumberStyles.HexNumber));
          return Color.FromArgb(c.R,c.G,c.B);
        }
        
#pragma warning restore CA1305 // Specify IFormatProvider
      }
    }
    public static string ToTexString(Color color) {
      if (PredefinedColors.TryGetBySecond(color, out var outString)) {
        return outString;
      } else {
#pragma warning disable CA1305 // Specify IFormatProvider
        string a = (color.A == 255) ? "" : color.A.ToString("X2");
        string r = color.R.ToString("X2");
        string g = color.G.ToString("X2");
        string b = color.B.ToString("X2");
#pragma warning restore CA1305 // Specify IFormatProvider
        return "#" + a + r + g + b;
      }

    }
    //https://en.wikibooks.org/wiki/LaTeX/Colors#Predefined_colors
    public static BiDictionary<string, Color> PredefinedColors { get; } =
      new BiDictionary<string, Color> {
      { "black", Color.FromArgb(0, 0, 0) },
      { "blue", Color.FromArgb(0, 0, 255) },
      { "brown", Color.FromArgb(150, 75, 0) },
      { "cyan", Color.FromArgb(0, 255, 255) },
      { "darkgray", Color.FromArgb(128, 128, 128) },
      { "gray", Color.FromArgb(169, 169, 169) },
      { "green", Color.FromArgb(0, 128, 0) },
      { "lightgray", Color.FromArgb(211, 211, 211) },
      { "lime", Color.FromArgb(0, 255, 0) },
      { "magenta", Color.FromArgb(255, 0, 255) },
      { "olive", Color.FromArgb(128, 128, 0) },
      { "orange", Color.FromArgb(255, 128, 0) },
      { "pink", Color.FromArgb(255, 192, 203) },
      { "purple", Color.FromArgb(128, 0, 128) },
      { "red", Color.FromArgb(255, 0,0) },
      { "teal", Color.FromArgb(0, 128, 128) },
      { "violet", Color.FromArgb(128, 0, 255) },
      { "white", Color.FromArgb(255, 255, 255) },
      { "yellow", Color.FromArgb(255, 255, 0) }
    };
  }
}