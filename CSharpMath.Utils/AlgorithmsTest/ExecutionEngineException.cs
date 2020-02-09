using System;

namespace CSharpMath.DevUtils {
  static partial class AlgorithmsTest {
    public static string ExecutionEngineException() {
      try {
        return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Type).GetType()).ToString();
      }
#pragma warning disable CS0618 //I know this is obsolete but this is just for fun
      catch (ExecutionEngineException) {
#pragma warning restore CS0618
        Console.WriteLine("Caught ExecutionEngineException");
      } catch {
        Console.WriteLine("Caught something");
      } finally {
        Console.WriteLine("Finally?");
      }
      return "Exception was caught.";
    }
  }
}
