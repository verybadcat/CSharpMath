using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface ISpace {
    float Length { get; }
    bool IsMu { get; }
    float ActualLength<TFont, TGlyph>(FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.MathFont<TGlyph>;
  }
}
