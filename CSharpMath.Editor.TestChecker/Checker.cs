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
        } while (value is null);
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
      Console.SetBufferSize(10000, 100); // We need lots of horizontal space, vertical not so much
      while (true) {
        try {
          Console.Clear();
          Console.ResetColor();
          Console.WriteLine("Welcome to the CSharpMath.Editor Test Checker!");
          Console.WriteLine();
          Console.WriteLine("Usage:");
          Console.WriteLine("Input the test expression in LaTeX below, and input the click position.");
          Console.WriteLine("You can visualize the test case by looking at the cursor position.");
          Console.WriteLine("Once you're done, you can press any key to move on to another test case.");
          Console.WriteLine("");
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
    public static void SetConsoleColor(Structures.Color? col) {
      if (col is Structures.Color color) {
        ConsoleColor ret = 0;
        double rr = color.R, gg = color.G, bb = color.B, delta = double.MaxValue;

        foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor))) {
          var n = Enum.GetName(typeof(ConsoleColor), cc);
          var c = Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
          var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
          if (t < delta) {
            delta = t;
            ret = cc;
          }
        }

        Console.ForegroundColor = ret;
      }
      else Console.ResetColor();
    }
    // Because CSharpMath uses the "normal mathematical" coordinate system internally, subtract p.Y
    // + 10 to create an "out of range" area
    static Point Adjust(Point p) {
      var (x, y) = (p.X + 10, Console.WindowHeight - p.Y);
      return new Point(x < 0 ? 0 : x, y < 0 ? 0 : y);
    }
    public static void ConsoleDrawRectangle(int width, int height, Point location, char glyph, Structures.Color? color) {
      location = Adjust(location);

      var innerRectWidth = width - 2;
      var innerRectHeight = height - 2;

      SetConsoleColor(color);
      Console.SetCursorPosition(location.X, location.Y);
      Console.Write('╔');
      for (var i = 0; i < innerRectWidth; i++)
        Console.Write('═');
      Console.Write('╗');
      for (var y = location.Y + 1; y < location.Y + innerRectHeight; y++) {
        Console.SetCursorPosition(location.X, y);
        Console.Write('║');
        Console.SetCursorPosition(location.X + innerRectWidth + 1, y);
        Console.Write('║');
      }
      Console.SetCursorPosition(location.X, location.Y + innerRectHeight);
      Console.Write('╚');
      for (var i = 0; i < innerRectWidth; i++)
        Console.Write('═');
      Console.Write('╝');

      if (glyph != '\0') {
        Console.SetCursorPosition(location.X + width / 2, location.Y + innerRectHeight / 2);
        Console.Write(glyph);
      }

      Console.ResetColor();
    }
  }
}