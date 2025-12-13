using System;
using System.Text;

namespace CSharpMath.Playground.TypographyTest {
  internal static class BreakString {
    public static void Benchmark() {
      Console.OutputEncoding = Encoding.Unicode;
      const int length = 1000000;
      var s = new System.Diagnostics.Stopwatch();
      var b = new Typography.TextBreak.CustomBreaker();
      var init = "Initialize".ToCharArray();
      b.BreakWords(init, 0, init.Length); //Don't measure startup costs
      foreach (var c in new[] {
        '0', '3', ' ', 'a', 'r', '#', '.', '%', '\r', '\u3232', '\uFEFF', '0'
      }) {
        s.Restart();
        var str = new string(c, length);
        var charArray = str.ToCharArray();
        b.BreakWords(charArray, 0, charArray.Length);
        s.Stop();
        Console.WriteLine("'{0}': {1}", c, s.Elapsed);
        GC.Collect();
      }
    }
  }
}
