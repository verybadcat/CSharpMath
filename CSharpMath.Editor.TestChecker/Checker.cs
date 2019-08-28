using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ListDisplay = CSharpMath.Display.ListDisplay<CSharpMath.Tests.FrontEnd.TestFont, char>;

namespace CSharpMath.Editor.TestChecker {
  public class Checker {
    /// <summary>Whether you want to view e.g. fraction lines and radical lines despite viewing character positions with less clarity.</summary>
    public static readonly bool OutputLines = false;

    public static void Main() {
      ListDisplay ReadLaTeX(string message) {
        string input;
        ListDisplay value;
        do {
          Console.Write(message);
          input = Console.ReadLine();
          value = Tests.ClosestPointTests.CreateDisplay(input);
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
      Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
      // We need lots of horizontal space, vertical not so much
      Console.SetBufferSize(10000, 500);
      // We need to output heavy box drawing characters, because the vertical light line displays as green lines at font size 16
      Console.OutputEncoding = Encoding.UTF8;
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
          // ConsoleDrawRectangle(Rectangle.Empty, 'O', Structures.Color.PredefinedColors["yellow"]); // Origin
          latex.Draw(context);
          var pos = Adjust(new Rectangle(x, y, 0, 0));
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
          var c = cc is ConsoleColor.DarkYellow ? Color.Orange : Color.FromName(n); // There's no "DarkYellow" in System.Drawing.Color
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
    static Rectangle Adjust(Rectangle rect) =>
      new Rectangle(
        Math.Clamp(rect.Left + 10 /* Out of range area */, 0, Console.BufferWidth),
        /* Convert from CSharpMath internal "normal mathematical" coordinate system -> subtract bottom */
        Math.Clamp(Console.BufferHeight / 2 - rect.Bottom, 0, Console.BufferHeight),
        rect.Width,
        rect.Height);
    public static void ConsoleDrawRectangle(Rectangle rect, char glyph, Structures.Color? color) {
      rect = Adjust(rect);

      var innerRectWidth = rect.Width - 2;
      var innerRectHeight = rect.Height - 2;

      SetConsoleColor(color);
      Console.SetCursorPosition(rect.X, rect.Y);
      if (rect.Width > 1 && rect.Height > 1) {
        Console.Write('┏');
        for (var i = 0; i < innerRectWidth; i++)
          Console.Write('━');
        Console.Write('┓');
        for (var y = rect.Y + 1; y < rect.Y + innerRectHeight; y++) {
          Console.SetCursorPosition(rect.X, y);
          Console.Write('┃');
          Console.SetCursorPosition(rect.X + innerRectWidth + 1, y);
          Console.Write('┃');
        }
        Console.SetCursorPosition(rect.X, rect.Y + innerRectHeight);
        Console.Write('┗');
        for (var i = 0; i < innerRectWidth; i++)
          Console.Write('━');
        Console.Write('┛');
      }

      if (glyph != '\0') {
        Console.SetCursorPosition(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        Console.Write(glyph);
      }

      Console.ResetColor();
    }
    public static void ConsoleDrawHorizontal(int x1_, int y_, int x2_, int thickness, Structures.Color? color) {
      var rect = Adjust(Rectangle.FromLTRB(x1_, y_ - thickness / 2, x2_, y_ + thickness / 2));
      SetConsoleColor(color);
      for (int i = 0; i < thickness; i++) {
        Console.SetCursorPosition(rect.Left, rect.Top + i);
        for (int ii = 0; ii < rect.Width; ii++)
          Console.Write('━');
      }
    }
  }
}