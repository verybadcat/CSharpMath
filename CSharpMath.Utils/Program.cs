using System;
using System.IO;
using System.Text;

namespace CSharpMath.DevUtils {
  class Program {
    static void Main(string[] args) {
      //Console.WriteLine(new StringBuilder().AppendJoin(", ", TypographyTest.Get.Codepoints("𝑥")).ToString());
      //Console.WriteLine(TypographyTest.MeasureString.Measure("𝑥", 20));
      //var path = Path.Combine(Environment.GetEnvironmentVariable("HOMEPATH"), "Desktop");
      //iosMathDemo.Builder.Build();
      //CSharpMathExamples.MirrorFromIos.Do();

      //Rendering.FontReferenceCodeBuilder.Build();
      unsafe { new Span<byte>(new byte[6]).CopyTo(new byte[3]); }


      Console.WriteLine();
      Console.WriteLine("Finished executing the method(s) requested.");
      Console.WriteLine("Press Enter to continue...");
      Console.ReadLine();
    }

    static async System.Threading.Tasks.Task<string> Crash() {
      try {
        return await System.Threading.Tasks.Task.Run(() =>
          System.Runtime.Serialization.FormatterServices.GetUninitializedObject
            (typeof(Type).GetType()).ToString());
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
      return "Exception was caught.";
    }
  }
}
