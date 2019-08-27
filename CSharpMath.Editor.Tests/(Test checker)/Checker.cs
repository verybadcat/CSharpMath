using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiaSharp;

namespace CSharpMath.Editor.Tests.TestChecker {
  public class Checker {
    public static void Main() {
      Console.WriteLine("Hi");
    }

    // https://stackoverflow.com/questions/33538527/display-a-image-in-a-console-application

    static uint[] cColors = { 0x000000, 0x000080, 0x008000, 0x008080, 0x800000, 0x800080, 0x808000, 0xC0C0C0, 0x808080, 0x0000FF, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0xFFFF00, 0xFFFFFF };

    public static void ConsoleWritePixel(SKColor cValue) {
      SKColor[] cTable = cColors.Select(x => new SKColor(0xFF000000 + x)).ToArray();
      var rList = new[] { '\u2591', '\u2592', '\u2593', '\u2588' }; // 1/4, 2/4, 3/4, 4/4
      var bestHit = new[] { 0, 0, 4, int.MaxValue }; //ForeColor, BackColor, Symbol, Score

      for (int rChar = rList.Length; rChar > 0; rChar--) {
        for (int cFore = 0; cFore < cTable.Length; cFore++) {
          for (int cBack = 0; cBack < cTable.Length; cBack++) {
            int R = (cTable[cFore].Red * rChar + cTable[cBack].Red * (rList.Length - rChar)) / rList.Length;
            int G = (cTable[cFore].Green * rChar + cTable[cBack].Green * (rList.Length - rChar)) / rList.Length;
            int B = (cTable[cFore].Blue * rChar + cTable[cBack].Blue * (rList.Length - rChar)) / rList.Length;
            int iScore = (cValue.Red - R) * (cValue.Red - R) + (cValue.Green - G) * (cValue.Green - G) + (cValue.Blue - B) * (cValue.Blue - B);
            if (!(rChar > 1 && rChar < 4 && iScore > 50000)) // rule out too weird combinations
            {
              if (iScore < bestHit[3]) {
                bestHit[3] = iScore; //Score
                bestHit[0] = cFore;  //ForeColor
                bestHit[1] = cBack;  //BackColor
                bestHit[2] = rChar;  //Symbol
              }
            }
          }
        }
      }
      Console.ForegroundColor = (ConsoleColor)bestHit[0];
      Console.BackgroundColor = (ConsoleColor)bestHit[1];
      Console.Write(rList[bestHit[2] - 1]);
    }


    public static void ConsoleWriteImage(SKBitmap source) {
      int sMax = 39;
      decimal percent = Math.Min(decimal.Divide(sMax, source.Width), decimal.Divide(sMax, source.Height));
      var dSize = new SKSizeI((int)(source.Width * percent), (int)(source.Height * percent));
      using (var bmpMax = source.Resize(new SKImageInfo(dSize.Width * 2, dSize.Height), SKFilterQuality.High))
        for (int i = 0; i < dSize.Height; i++) {
          for (int j = 0; j < dSize.Width; j++) {
            ConsoleWritePixel(bmpMax.GetPixel(j * 2, i));
            ConsoleWritePixel(bmpMax.GetPixel(j * 2 + 1, i));
          }
          Console.WriteLine();
        }
      Console.ResetColor();
    }
  }
}
