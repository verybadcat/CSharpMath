using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace CSharpMath.Maui {
  public static class LatexHelper {
    public static readonly string phantom = SetColor("|", Colors.Transparent);
    public static string SetColor(string latex, Color? color) => color != null ? @"\color{" + color.ToHex() + "}{" + latex + "}" : latex;
  }
}
