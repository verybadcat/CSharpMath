using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
  public readonly struct Color {
    public Color(byte r, byte g, byte b, byte a = 0xFF) : this() {
      R = r;
      G = g;
      B = b;
    }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public void Deconstruct(out byte r, out byte g, out byte b) {
      r = R;
      g = G;
      b = B;
    }
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a) {
      r = R;
      g = G;
      b = B;
      a = A;
    }

    public static implicit operator (byte R, byte G, byte B)(Color c) => (c.R, c.G, c.B);
    public static implicit operator (byte R, byte G, byte B, byte A) (Color c) => (c.R, c.G, c.B, c.A);
    public static implicit operator Color((byte R, byte G, byte B) c) => new Color(c.R, c.G, c.B);
    public static implicit operator Color((byte R, byte G, byte B, byte A) c) => new Color(c.R, c.G, c.B, c.A);
  }
}