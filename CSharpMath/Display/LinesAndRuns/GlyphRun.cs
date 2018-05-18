using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  /// <summary>Corresponds to CTRun in iOSMath.</summary> 
  public class GlyphRun {
    string Text { get; set; }
    (byte r, byte g, byte b) TextColor { get; set; }
  }
}
