using CSharpMath.Display.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.FrontEnd {
  public interface IFontMeasurer {
    int GetUnitsPerEm(Font font);
  }
}
