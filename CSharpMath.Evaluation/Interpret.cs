using System.Collections.Generic;
using System.Linq;
namespace CSharpMath {
  partial class Evaluation {
    public static string Interpret(Atom.MathList mathList, System.Func<string, string>? errorLaTeX = null) {
      errorLaTeX ??= error => $@"\color{{red}}\text{{{error.Replace("%", @"\%")}}}";

      (Atom.MathList left, Atom.MathList right)? equation = null;
      for (var i = 0; i < mathList.Count; i++) {
        if (mathList[i] is Atom.Atoms.Relation { Nucleus: "=" })
          if (equation == null)
            equation = (mathList.Slice(0, i), mathList.Slice(i + 1, mathList.Count - i - 1));
          else
            return errorLaTeX("Only 0 or 1 =s are supported");
      }
      if (equation is { } eq) {
        mathList = new Atom.MathList {
          new Atom.Atoms.Inner(new Atom.Boundary("("), eq.left, new Atom.Boundary(")")),
          new Atom.Atoms.BinaryOperator("\u2212"),
          new Atom.Atoms.Inner(new Atom.Boundary("("), eq.right, new Atom.Boundary(")"))
        };
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
                  latex.Append(solutions[0].Latexise());
                  foreach (var solution in solutions.Skip(1))
                    latex.Append(@"\\").Append(solution.Latexise());
                  latex.Append(@"\end{cases}");
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
