using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Display;
using CSharpMath.Display.Text;

namespace CSharpMath {
  public interface IPositionableDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>, ISettablePosition
    where TFont: MathFont<TGlyph> {
  }
}
