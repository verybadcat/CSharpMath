using System;
using System.IO;
using System.Text;

namespace CSharpMath.DevUtils {
  class Program {
    static void Main(string[] args) {
      //Rendering.OtfCodeBuilder.Build();
      //Console.WriteLine(new StringBuilder().AppendJoin(", ", TypographyTest.Get.Codepoints("𝑥")).ToString());
      //Console.WriteLine(TypographyTest.MeasureString.Measure("𝑥", 20));
      //var path = Path.Combine(Environment.GetEnvironmentVariable("HOMEPATH"), "Desktop");
      iosMathDemo.Builder.Build();
      //CSharpMathExamples.MirrorFromIos.Do();

      Console.WriteLine();
      Console.WriteLine("Finished executing the method(s) requested.");
      Console.WriteLine("Press Enter to continue...");
      Console.ReadLine();
    }
  }
}
