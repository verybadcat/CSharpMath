using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
  public readonly struct Color {
    public Color(byte r, byte g, byte b, byte a = 0xFF) : this() {
      R = r;
      G = g;
      B = b;
      A = a;
    }
    public Color(float r, float g, float b, float a = 1f)
      : this((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f)) { }

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
    public void Deconstruct(out float r, out float g, out float b) {
      r = Rf;
      g = Gf;
      b = Bf;
    }
    public void Deconstruct(out float r, out float g, out float b, out float a) {
      Deconstruct(out r, out g, out b);
      a = Af;
    }

    public override string ToString() {
      string ToString(byte b) => b.ToString("X").PadLeft(2, '0');
      return $"#{ToString(A)}{ToString(R)}{ToString(G)}{ToString(B)}";
    }

    private static byte _fromHex1(string hex, int index) //read one hex char -> byte
      => (byte)(Convert.ToInt32(hex.Substring(index, 1), 16) * 17);
    private static byte _fromHex2(string hex, int index) //read two hex chars -> byte
      => Convert.ToByte(hex.Substring(index, 2), 16);
    public static Color? FromHexString(string hex) {
      if (hex == null) return null;
      hex = hex.RemovePrefix("#").RemovePrefix("0x");
      switch (hex.Length) {
        case 3:
          return new Color(_fromHex1(hex, 0), _fromHex1(hex, 1), _fromHex1(hex, 2));
        case 4:
          return new Color(_fromHex1(hex, 1), _fromHex1(hex, 2), _fromHex1(hex, 3), _fromHex1(hex, 0));
        case 6:
          return new Color(_fromHex2(hex, 0), _fromHex2(hex, 2), _fromHex2(hex, 4));
        case 8:
          return new Color(_fromHex2(hex, 2), _fromHex2(hex, 4), _fromHex2(hex, 6), _fromHex2(hex, 0));
        default:
          return null;
      }
    }
  }
}