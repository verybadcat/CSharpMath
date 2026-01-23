using System;
using System.Drawing;
using System.Text;
using ListDisplay = CSharpMath.Display.Displays.ListDisplay<CSharpMath.Core.BackEnd.TestFont, System.Text.Rune>;

namespace CSharpMath.Core {
  public class Checker {
    /// <summary>Whether you want to view e.g. fraction lines and radical lines
    /// despite viewing character positions with less clarity.</summary>
    public static readonly bool OutputLines = true;
    public static void Main() {
      static int ReadInt(string message) {
        string? input;
        int value;
        do {
          Console.Write(message);
          input = Console.ReadLine();
        } while (!int.TryParse(input, out value));
        return value;
      }
      var context = new GraphicsContext();
      // We need to output heavy box drawing characters, because the vertical light line displays as green lines at font size 16
      Console.OutputEncoding = Encoding.UTF8;
      string? latex = null;
      while (true) {
        try {
          Console.Title = "CSharpMath.Core.Example";
          Console.Clear();
          if (OperatingSystem.IsWindows())
            Console.SetBufferSize(Console.WindowWidth, Console.BufferHeight); // line wrapping
          Console.ResetColor();
          Console.WriteLine("Welcome to CSharpMath.Core.Example!");
          Console.WriteLine();
          Console.WriteLine("Usage:");
          Console.WriteLine("Input the test expression in LaTeX below, and input the click position.");
          Console.WriteLine("You can visualize the test case by looking at the cursor position.");
          Console.WriteLine();
          Console.WriteLine("Controls:");
          Console.WriteLine("Arrow keys: Moves the cursor.");
          Console.WriteLine("Q/W/E/A/D/Z/X/C: Moves the cursor with direction from S to the key.");
          Console.WriteLine("P: Re-inputs the cursor position.");
          Console.WriteLine("Enter: Moves on to another test case.");
          Console.WriteLine("");
          
          ListDisplay? display = null;
          void AssignDisplay() {
            Console.Write("Input LaTeX: ");
            if (latex is null) latex = Console.ReadLine()!;
            else Console.WriteLine(latex); // The P-key case
            BackEnd.TestTypesettingContext.CreateDisplay(latex)
            .Match(listDisplay => display = listDisplay, error => {
              Console.WriteLine(error);
              latex = null;
              AssignDisplay();
            });
          }
          AssignDisplay();
          if (display == null) throw new Atom.InvalidCodePathException("display is still not assigned");
          var x = ReadInt("Input Touch X (integer): ");
          var y = ReadInt("Input Touch Y (integer): ");
          Console.Clear();

          if(OperatingSystem.IsWindows())
            Console.SetBufferSize(10000, Console.BufferHeight); // no line wrapping
          display.Draw(context);
moveCursor:var pos = Adjust(new Rectangle(x, y, 0, 0));
          Console.Title = $"CSharpMath.Editor.Tests Visualizer - ({x}, {y}) in {latex}";
          Console.SetCursorPosition(pos.X, pos.Y);
          switch (Console.ReadKey(true).Key) {
            case ConsoleKey.Q: x--; y++; goto moveCursor;
            case ConsoleKey.W: case ConsoleKey.UpArrow: y++; goto moveCursor;
            case ConsoleKey.E: x++; y++; goto moveCursor;
            case ConsoleKey.A: case ConsoleKey.LeftArrow: x--; goto moveCursor;
            case ConsoleKey.D: case ConsoleKey.RightArrow: x++; goto moveCursor;
            case ConsoleKey.Z: x--; y--; goto moveCursor;
            case ConsoleKey.X: case ConsoleKey.DownArrow: y--; goto moveCursor;
            case ConsoleKey.C: x++; y--; goto moveCursor;
            case ConsoleKey.P: break;
            case ConsoleKey.Enter: latex = null; break;
            default: goto moveCursor;
          }
        } catch (Exception e) {
          Console.Write(e);
          Console.ReadKey();
        }
      }
    }
    public static void SetConsoleColor(Color? col) {
      if (col is Color color) {
        ConsoleColor ret = 0;
        double rr = color.R, gg = color.G, bb = color.B, delta = double.MaxValue;
        foreach (var cc in System.Linq.Enumerable.Cast<ConsoleColor>(Enum.GetValues(typeof(ConsoleColor)))) {
          var n = Enum.GetName(typeof(ConsoleColor), cc)!;
          // There's no "DarkYellow" in System.Drawing.Color
          var c = cc is ConsoleColor.DarkYellow ? System.Drawing.Color.Orange : System.Drawing.Color.FromName(n);
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
        Math.Clamp(rect.Left + 10 /* Out of range area */, 0, Console.BufferWidth - 1),
        /* Convert from CSharpMath internal "normal mathematical" coordinate system -> subtract bottom */
        Math.Clamp(Console.BufferHeight / 2 - rect.Bottom, 0, Console.BufferHeight - 1),
        rect.Width,
        rect.Height);
    public static void ConsoleDrawRectangle(Rectangle rect, Rune glyph, Color? color) {
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
        for (var y = rect.Y + 1; y <= rect.Y + innerRectHeight; y++) {
          Console.SetCursorPosition(rect.X, y);
          Console.Write('┃');
          Console.SetCursorPosition(rect.X + innerRectWidth + 1, y);
          Console.Write('┃');
        }
        Console.SetCursorPosition(rect.X, rect.Y + innerRectHeight + 1);
        Console.Write('┗');
        for (var i = 0; i < innerRectWidth; i++)
          Console.Write('━');
        Console.Write('┛');
      }
      if (glyph != new Rune()) {
        Console.SetCursorPosition(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        Console.Write(glyph.ToString());
      }
      Console.ResetColor();
    }
    public static void ConsoleFillRectangle(Rectangle rect, Color? color) {
      rect = Adjust(rect);
      SetConsoleColor(color);
      if (rect.Width > 0 && rect.Height > 0) {
        for (var i = 0; i < rect.Height; i++) {
          Console.SetCursorPosition(rect.X, rect.Y + i);
          for (var j = 0; j < rect.Width; j++)
            Console.Write('█');
        }
      }
      Console.ResetColor();
    }
    public static void ConsoleDrawHorizontal
      (int x1_, int y_, int x2_, int thickness, Color? color) {
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