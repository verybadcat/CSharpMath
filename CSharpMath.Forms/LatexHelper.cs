using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CSharpMath.Forms {
  public static class LatexHelper {
    public static readonly string vphantom = SetColor("|", Color.Transparent);
    public static string SetColor(string latex, Color color) => @"{\color{" + color.ToHex() + "}{" + latex + "}}";
  }
}
