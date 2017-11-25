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
      TextColor = new StringAttribute<Color>(textColor) ;
    } 
    public void AppendAttributedString(AttributedString other) {

    }
  }

  public static class AttributedStringExtensions {
    public static AttributedString Combine(AttributedString attr1, AttributedString attr2) {
      if (attr1 == null) {
        return attr2;
      }
      if (attr2 == null) {
        return attr1;
      }
      attr1.AppendAttributedString(attr2);
      return attr1;
    }
  }
}
