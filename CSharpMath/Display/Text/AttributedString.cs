using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString {
    public string Text { get; set; }
    public int Length => Text.Length;
    public Font Font { get; set; }
  }
}
