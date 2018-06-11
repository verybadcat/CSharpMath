using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public readonly struct Glyph {
    public Glyph(Typography.OpenFont.Typeface typeface, Typography.OpenFont.Glyph info) : this() {
      Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));
      Info = info ?? throw new ArgumentNullException(nameof(info));
    }

    public Typography.OpenFont.Typeface Typeface { get; }
    public Typography.OpenFont.Glyph Info { get; }

    public void Deconstruct(out Typography.OpenFont.Typeface typeface, out Typography.OpenFont.Glyph glyph) { typeface = Typeface; glyph = Info; }

    public bool IsEmpty => Equals(Empty);
    public static readonly Glyph Empty = new Glyph();
  }
}
