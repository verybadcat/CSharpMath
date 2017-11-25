using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString {
    public string Text { get; set; }
    public int Length => Text.Length;
    public MathFont Font { get; set; }

    public StringAttribute<Color> TextColor { get; set; }
    public StringAttribute<float> Kern { get; set; } // or int? Not sure

    public AttributedString(string text = "") {
      Text = text;
    }
    public AttributedString(string text, Color textColor) {
      Text = text;
      TextColor = new StringAttribute(textColor) ;
    } 
    public void AppendAttributedString(AttributedString other) {

    }
  }
}
