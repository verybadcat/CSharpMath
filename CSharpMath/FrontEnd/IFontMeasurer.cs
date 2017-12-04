using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IFontMeasurer<TGlyph> {
    /// <summary>A proportionality constant that is applied when
    /// reading from the Json table.</summary>
    int GetUnitsPerEm(MathFont<TGlyph> font);

  }
}
