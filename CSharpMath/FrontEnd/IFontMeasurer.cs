using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IFontMeasurer<TMathFont, TGlyph>
    where TMathFont: MathFont<TGlyph> {
    /// <summary>A proportionality constant that is applied when
    /// reading from the Json table.</summary>
    int GetUnitsPerEm(TMathFont font);

  }
}
