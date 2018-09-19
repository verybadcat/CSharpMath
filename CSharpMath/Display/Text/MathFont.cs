using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public interface IMathFont<TGlyph> {
    float PointSize { get; }
  }
}
