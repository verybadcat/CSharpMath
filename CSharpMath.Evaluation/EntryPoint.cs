using System;
namespace CSharpMath.Evaluation {
  public class EntryPoint {
    public static void Main() {
      while (true) {
        try {
          Console.Write("Enter LaTeX: ");
          var entity =
            MathS.ToMathSEntity(
              Atom.LaTeXParser.MathListFromLaTeX(Console.ReadLine())
              .Match(e => e, e => throw new Exception(e))
            ).Match(e => e, e => throw new Exception(e));
          Console.Write("Input: ");
          Console.WriteLine(entity);
          Console.Write("Output: ");
          Console.WriteLine(entity.Simplify());
        } catch (Exception e) {
          Console.WriteLine(e);
        }
      }
    }
  }
}
