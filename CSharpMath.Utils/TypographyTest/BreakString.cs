using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.DevUtils.TypographyTest {
  internal static class BreakString {
    public static void Benchmark() {
      Console.OutputEncoding = Encoding.Unicode;
      const int length = 100;
      var s = new System.Diagnostics.Stopwatch();
      var b = new Typography.TextBreak.CustomBreaker();
      b.BreakWords("Initialize"); //Don't measure startup costs
      void TestChar(char c) {
        {
          s.Restart();
          b.BreakWords(new string(c, length));
          s.Stop();
          Console.WriteLine("'{0}': {1}", c, s.Elapsed);
        }
        GC.Collect();
      }
      foreach (var c in new[] { '0', '3', ' ', 'a', 'r', '#', '.', '%', '\r', '\u3232' })
        TestChar(c);
    }
  }
}
