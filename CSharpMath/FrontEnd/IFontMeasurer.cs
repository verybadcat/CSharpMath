using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Display;

namespace CSharpMath.FrontEnd {
  public interface IFontMeasurer<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    /// <summary>A proportionality constant that is applied when
    /// reading from the Json table.</summary>
    int GetUnitsPerEm(TFont font);
  }
}
