using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display.Text {
  public class TextLine {
    /// <summary>Corresponds to CTLine in iOSMath</summary>
    public List<GlyphRun> Runs { get; private set; }
  }
}
