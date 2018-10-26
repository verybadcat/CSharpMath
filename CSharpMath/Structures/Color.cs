using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
#warning Replace this with TColor
  public readonly struct Color {
    public Color(byte r, byte g, byte b, byte a = 0xFF) : this() {
      R = r;
      G = g;
      B = b;
      A = a;
    }
    public Color(float rf, float gf, float bf, float af = 1f)
      : this((byte)(rf * 255f), (byte)(gf * 255f), (byte)(bf * 255f), (byte)(af * 255f)) { }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }
    public float Rf => R / 255f;
    public float Gf => G / 255f;
    public float Bf => B / 255f;
    public float Af => A / 255f;

    public void Deconstruct(out byte r, out byte g, out byte b) {
      r = R;
      g = G;
      b = B;
    }
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a) {
      Deconstruct(out r, out g, out b);
      a = A;
    }
    public void Deconstruct(out float rf, out float gf, out float bf) {
      rf = Rf;
      gf = Gf;
      bf = Bf;
    }
    public void Deconstruct(out float rf, out float gf, out float bf, out float af) {
      Deconstruct(out rf, out gf, out bf);
      af = Af;
    }

    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object obj) => obj is Color c ? Equals(c) : false;
    public override int GetHashCode() => unchecked(R * 13 + G * 37 + B * 113 + A * 239);

    public override string ToString() {
      string ToString(byte b) => b.ToString("X").PadLeft(2, '0');
      return $"#{ToString(A)}{ToString(R)}{ToString(G)}{ToString(B)}";
    }

    public static Color? Create(ReadOnlySpan<char> hexOrName, bool extraSweet = true) {
      if (hexOrName == null) return null;
      if (extraSweet && (hexOrName.StartsWithInvariant("#") || hexOrName.StartsWithInvariant("0x"))) return FromHexString(hexOrName);
      if (hexOrName.Length <= 100) { //Never gonna spill the stack
        Span<char> loweredName = stackalloc char[hexOrName.Length];
        hexOrName.ToLowerInvariant(loweredName);
        if (((ReadOnlySpan<char>)loweredName).TryAccessDictionary(PredefinedColors, out var predefined)) return predefined;
      }
      return null;
    }
    private static int? _charToByte(char c) =>
      c >= '0' && c <= '9' ? c - '0' :
      c >= 'A' && c <= 'F' ? c - 'A' + 10 :
      c >= 'a' && c <= 'f' ? c - 'a' + 10 :
      new int?();
    private static byte? _fromHex1(ReadOnlySpan<char> hex, int index) => //read one hex char -> byte
      (byte?)(_charToByte(hex[index]) * 17);
    private static byte? _fromHex2(ReadOnlySpan<char> hex, int index) => //read two hex chars -> byte
      (byte?)(_charToByte(hex[index]) * 16 + _charToByte(hex[index + 1]));
    private static Color? FromHexString(ReadOnlySpan<char> hex) {
      hex = hex.RemovePrefix("#").RemovePrefix("0x");
      byte? r, g, b, a;
      switch (hex.Length) {
        case 3:
          r = _fromHex1(hex, 0);
          g = _fromHex1(hex, 1);
          b = _fromHex1(hex, 2);
          if ((r ^ g ^ b) is null) return null;
          return new Color(r.GetValueOrDefault(), g.GetValueOrDefault(), b.GetValueOrDefault());
        case 4:
          a = _fromHex1(hex, 0);
          r = _fromHex1(hex, 1);
          g = _fromHex1(hex, 2);
          b = _fromHex1(hex, 3);
          if ((r ^ g ^ b ^ a) is null) return null;
          return new Color(r.GetValueOrDefault(), g.GetValueOrDefault(), b.GetValueOrDefault(), a.GetValueOrDefault());
        case 6:
          r = _fromHex2(hex, 0);
          g = _fromHex2(hex, 2);
          b = _fromHex2(hex, 4);
          if ((r ^ g ^ b) is null) return null;
          return new Color(r.GetValueOrDefault(), g.GetValueOrDefault(), b.GetValueOrDefault());
        case 8:
          a = _fromHex2(hex, 0);
          r = _fromHex2(hex, 2);
          g = _fromHex2(hex, 4);
          b = _fromHex2(hex, 6);
          if ((r ^ g ^ b ^ a) is null) return null;
          return new Color(r.GetValueOrDefault(), g.GetValueOrDefault(), b.GetValueOrDefault(), a.GetValueOrDefault());
        default:
          return null;
      }
    }
    //https://en.wikibooks.org/wiki/LaTeX/Colors#Predefined_colors
    /// <summary>
    /// Names containing non-English characters or longer than 100 characters will be IGNORED.
    /// </summary>
    public static BiDictionary<string, Color> PredefinedColors { get; } = new BiDictionary<string, Color> {
      { "black", new Color(0, 0, 0) },
      { "blue", new Color(0, 0, 255) },
      { "brown", new Color(150, 75, 0) },
      { "cyan", new Color(0, 255, 255) },
      { "darkgray", new Color(128, 128, 128) },
      { "gray", new Color(169, 169, 169) },
      { "green", new Color(0, 128, 0) },
      { "lightgray", new Color(211, 211, 211) },
      { "lime", new Color(0, 255, 0) },
      { "magenta", new Color(255, 0, 255) },
      { "olive", new Color(128, 128, 0) },
      { "orange", new Color(255, 128, 0) },
      { "pink", new Color(255, 192, 203) },
      { "purple", new Color(128, 0, 128) },
      { "red", new Color(255, 0,0) },
      { "teal", new Color(0, 128, 128) },
      { "violet", new Color(128, 0, 255) },
      { "white", new Color(255, 255, 255) },
      { "yellow", new Color(255, 255, 0) }
    };
  }
}