using System;
using System.Text;

namespace CSharpMath.DevUtils {
  class Program {
    static void Main(string[] args) {
      //SkiaSharp.OtfCodeBuilder.Build();
      //Console.WriteLine(new StringBuilder().AppendJoin(", ", TypographyTest.Get.Codepoints("𝑥")).ToString());
      Console.WriteLine(TypographyTest.MeasureString.Measure("𝑥", 20));

      Console.WriteLine();
      Console.WriteLine("Finished executing the method(s) requested.");
      Console.WriteLine("Press Enter to continue...");
      Console.ReadLine();
    }
  }
}
