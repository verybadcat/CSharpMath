using System.Drawing;

namespace CSharpMath.Structures {
  static class ColorExtensions {
    //public override string ToString() =>
    //  PredefinedColors.TryGetBySecond(this, out var name)
    //  ? name : $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    //public static Color? Create(ReadOnlySpan<char> hexOrName, bool extraSweet = true) {
    //  if (hexOrName == null) return null;
    //  if (extraSweet && (hexOrName.StartsWithInvariant("#") || hexOrName.StartsWithInvariant("0x")))
    //    return FromHexString(hexOrName.RemovePrefix("#").RemovePrefix("0x"));
    //  Span<char> loweredName = stackalloc char[hexOrName.Length];
    //  hexOrName.ToLowerInvariant(loweredName);
    //  if (PredefinedColors.TryGetByFirst(loweredName.ToString(), out var predefined))
    //    return predefined;
    //  return null;
    //  static Color? FromHexString(ReadOnlySpan<char> hex) {
    //    static int? CharToByte(char c) =>
    //      c >= '0' && c <= '9' ? c - '0' :
    //      c >= 'A' && c <= 'F' ? c - 'A' + 10 :
    //      c >= 'a' && c <= 'f' ? c - 'a' + 10 :
    //      new int?();
    //    static byte? FromHex1(ReadOnlySpan<char> hex, int index) =>
    //      //read one hex char -> byte
    //      (byte?)(CharToByte(hex[index]) * 17);
    //    static byte? FromHex2(ReadOnlySpan<char> hex, int index) =>
    //      //read two hex chars -> byte
    //      (byte?)(CharToByte(hex[index]) * 16 + CharToByte(hex[index + 1]));
    //    return hex.Length switch
    //    {
    //      3 => (FromHex1(hex, 0), FromHex1(hex, 1), FromHex1(hex, 2)) switch
    //      {
    //        (byte r, byte g, byte b) => new Color(r, g, b),
    //        _ => new Color?()
    //      },
    //      4 => (FromHex1(hex, 0), FromHex1(hex, 1), FromHex1(hex, 2), FromHex1(hex, 3)) switch
    //      {
    //        (byte a, byte r, byte g, byte b) => new Color(r, g, b, a),
    //        _ => new Color?()
    //      },
    //      6 => (FromHex2(hex, 0), FromHex2(hex, 2), FromHex2(hex, 4)) switch
    //      {
    //        (byte r, byte g, byte b) => new Color(r, g, b),
    //        _ => new Color?()
    //      },
    //      8 => (FromHex2(hex, 0), FromHex2(hex, 2), FromHex2(hex, 4), FromHex2(hex, 6)) switch
    //      {
    //        (byte a, byte r, byte g, byte b) => new Color(r, g, b, a),
    //        _ => new Color?()
    //      },
    //      _ => null,
    //    };
    //  }
    //}
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