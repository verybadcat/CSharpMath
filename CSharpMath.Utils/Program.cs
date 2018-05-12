using System;

namespace CSharpMath.DevUtils {
  class Program {
    static void Main(string[] args) {
      SkiaSharp.OtfCodeBuilder.Build();

      Console.WriteLine("Finished executing the method(s) requested.");
      Console.WriteLine("Press Enter to continue...");
      Console.ReadLine();
    }
  }
}
