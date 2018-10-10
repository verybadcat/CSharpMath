using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public interface IFont<TGlyph> {
    float PointSize { get; }
  }
}
