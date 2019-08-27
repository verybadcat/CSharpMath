using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ListDisplay = CSharpMath.Display.ListDisplay<CSharpMath.Tests.FrontEnd.TestFont, char>;

namespace CSharpMath.Editor.TestChecker {
  public class Checker {
    public static void Main() {
      ListDisplay ReadLaTeX(string message) {
        string input;
        ListDisplay value;
        do {
          Console.Write(message);
          input = Console.ReadLine();
          value = Tests.DisplayEditingTests.CreateDisplay(input);
        } while (input is null);
        return value;
      }
      int ReadInt(string message) {
        string input;
        int value;
        do {
          Console.Write(message);
          input = Console.ReadLine();
        } while (!int.TryParse(input, out value));
        return value;
      }

      var context = new GraphicsContext();
      while (true) {
        try {
          Console.Clear();
          var latex = ReadLaTeX("Input LaTeX: ");
          var x = ReadInt("Input Touch X (integer): ");
          var y = ReadInt("Input Touch Y (integer): ");
          Console.Clear();
          latex.Draw(context);
          var pos = Adjust(new Point(x, y));
          Console.SetCursorPosition(pos.X, pos.Y);
          Console.ReadKey();
        } catch (Exception e) {
          Console.Write(e);
          Console.ReadKey();
        }
      }
    }
    static ConsoleColor ClosestConsoleColor(byte r, byte g, byte b) {
      ConsoleColor ret = 0;
      double rr = r, gg = g, bb = b, delta = double.MaxValue;

      foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor))) {
        var n = Enum.GetName(typeof(ConsoleColor), cc);
        var c = Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
        var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
        if (t == 0.0)
          return cc;
        if (t < delta) {
          delta = t;
          ret = cc;
        }
      }
      return ret;
    }
    // Because CSharpMath uses the "normal mathematical" coordinate system internally, subtract p.Y
    public static Point Adjust(Point p) => new Point(p.X, Console.WindowHeight - p.Y);
    public static void ConsoleDrawRectangle(int width, int height, Point location, char tag = '\0', Structures.Color? borderColor = null) {
      location = Adjust(location);

      width -= 2; // Exclude borders
      height -= 2;

      if (borderColor is Structures.Color c)
        Console.ForegroundColor = ClosestConsoleColor(c.R, c.G, c.B);
      Console.SetCursorPosition(location.X, location.Y);
      Console.Write('╔');
      for (var i = 0; i < width; i++)
        Console.Write('═');
      Console.Write('╗');
      for (var y = location.Y + 1; y < location.Y + height; y++) {
        Console.SetCursorPosition(location.X, y);
        Console.Write('║');
        Console.SetCursorPosition(location.X + width + 1, y);
        Console.Write('║');
      }
      Console.SetCursorPosition(location.X, location.Y + height);
      Console.Write('╚');
      for (var i = 0; i < width; i++)
        Console.Write('═');
      Console.Write('╝');

      if (tag != '\0') {
        Console.SetCursorPosition(location.X + width / 2, location.Y + height / 2);
        Console.Write(tag);
      }

      Console.ResetColor();
    }
  }
}