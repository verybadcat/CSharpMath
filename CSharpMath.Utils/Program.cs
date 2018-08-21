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
      //iosMathDemo.Builder.Build();
      //CSharpMathExamples.MirrorFromIos.Do();
      try {
        System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Type).GetType()).ToString();
      }
#pragma warning disable CS0618 //I know this is obsolete but this is just for fun
      catch (ExecutionEngineException) {
#pragma warning restore CS0618
        Console.WriteLine("Caught ExecutionEngineException");
      }
      catch {
        Console.WriteLine("Caught something");
      }
      finally {
        Console.WriteLine("Finally?");
      }
      Console.WriteLine();
      Console.WriteLine("Finished executing the method(s) requested.");
      Console.WriteLine("Press Enter to continue...");
      Console.ReadLine();
    }
  }
}
