using System.Collections.Generic;
using System.Linq;
namespace CSharpMath {
  partial class Evaluation {
    public static string Interpret(Atom.MathList mathList, System.Func<string, string>? errorLaTeX = null) {
      errorLaTeX ??= error => $@"\color{{red}}\text{{{error.Replace("%", @"\%")}}}";

      (IEnumerable<Atom.MathAtom> left, IEnumerable<Atom.MathAtom> right)? equation = null;
      for (var i = 0; i < mathList.Count; i++) {
        if (mathList[i] is Atom.Atoms.Relation { Nucleus: "=" })
          if (equation == null)
            equation = (mathList.Take(i), mathList.Skip(i + 1));
          else
            return errorLaTeX("Only 0 or 1 =s are supported");
      }
      if (equation is { } eq) {
        mathList = new Atom.MathList();
        mathList.Append(eq.left);
        mathList.Add(new Atom.Atoms.BinaryOperator("\u2212"));
        mathList.Append(eq.right);
        return MathListToEntity(mathList)
        .Match(entity => {
          var latex = new System.Text.StringBuilder();
          var variables = AngouriMath.MathS.GetUniqueVariables(entity);
          if (variables.Count == 0)
            return $@"\text{{{entity.Eval() == 0}}}";
          foreach (AngouriMath.VariableEntity variable in variables) {
            latex.Append(variable).Append(@"=");
            try {
              var solutions = entity.SolveEquation(variable);
              switch (solutions.Count) {
                case 0:
                  latex.Append(@"\emptyset");
                  break;
                case 1:
                  latex.Append(solutions[0].Latexise());
                  break;
                default:
                  latex.Append(@"\begin{cases}");
                  foreach (var solution in solutions)
                    latex.Append(solution.Latexise()).Append(@"\\");
                  // Remove last \\
                  latex.Remove(latex.Length - 2, 2).Append(@"\end{cases}");
                  break;
              }
              latex.Append(",");
            } catch (System.Exception e) {
              latex.Append(errorLaTeX(e.Message));
            }
          }
          // Remove last ,
          return latex.Remove(latex.Length - 1, 1).ToString();
        }, errorLaTeX);
      }

      return MathListToEntity(mathList)
      .Match(entity => {
        void TryOutput(System.Text.StringBuilder sb, System.Func<AngouriMath.Entity> getter) {
          sb.Append(@"\\=\ &");
          try {
            sb.Append(getter().Latexise());
          } catch (System.Exception e) {
            sb.Append(errorLaTeX(e.Message));
          }
        }
        var latex = new System.Text.StringBuilder("&");
        latex.Append(entity.Latexise());
        TryOutput(latex, entity.Simplify);
        TryOutput(latex, () => entity.Eval());
        return latex.ToString();
      }, errorLaTeX);
    }
  }
}
