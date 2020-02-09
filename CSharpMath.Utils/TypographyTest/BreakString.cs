using System;
using System.Text;

namespace CSharpMath.DevUtils.TypographyTest {
  internal static class BreakString {
    public static void Benchmark() {
      Console.OutputEncoding = Encoding.Unicode;
      const int length = 1000000;
      var s = new System.Diagnostics.Stopwatch();
      var b = new Typography.TextBreak.CustomBreaker();
      b.SetNewBreakHandler(_ => { });
      b.BreakWords("Initialize"); //Don't measure startup costs
      foreach (var c in new[] {
        '0', '3', ' ', 'a', 'r', '#', '.', '%', '\r', '\u3232', '\uFEFF', '0'
      }) {
        s.Restart();
        b.BreakWords(new string(c, length));
        s.Stop();
        Console.WriteLine("'{0}': {1}", c, s.Elapsed);
        GC.Collect();
      }
    }
  }
}
