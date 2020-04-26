using System.Linq;
using System.Text;
namespace CSharpMath {
  partial class Evaluation {
    static void LatexiseAll(StringBuilder latex, AngouriMath.EntitySet entities) {
      switch (entities.Count) {
        case 0:
          latex.Append(@"\emptyset");
          break;
        case 1:
          latex.Append(entities[0].Latexise());
          break;
        default:
          latex.Append(@"\begin{cases}");
          latex.Append(entities[0].Latexise());
          foreach (var entity in entities.Skip(1))
            latex.Append(@"\\").Append(entity.Latexise());
          latex.Append(@"\end{cases}");
          break;
      }
    }
    public static string Interpret(Atom.MathList mathList, System.Func<string, string>? errorLaTeX = null) {
      errorLaTeX ??= error => $@"\color{{red}}\text{{{Atom.LaTeXParser.EscapeAsLaTeX(error)}}}";

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
          var latex = new StringBuilder();
          var variables = AngouriMath.MathS.GetUniqueVariables(entity);
          if (variables.Count == 0)
            return $@"\text{{{entity.Eval() == 0}}}";
          foreach (AngouriMath.VariableEntity variable in variables) {
            latex.Append(variable).Append(@"=");
            try {
              LatexiseAll(latex, entity.SolveEquation(variable));
            } catch (System.Exception e) {
              latex.Append(errorLaTeX(e.Message));
            }
            latex.Append(",");
          }
          // Remove last ,
          return latex.Remove(latex.Length - 1, 1).ToString();
        }, errorLaTeX);
      }

      return MathListToEntity(mathList)
      .Match(entity => {
        var latex = new StringBuilder(@"\begin{aligned} &");
        latex.Append(entity.Latexise());
        void TryOutput(string lineName, System.Action appendLaTeX) {
          latex.Append(@"\\ \text{").Append(lineName).Append(@"} \colon & \;");
          try {
            appendLaTeX();
          } catch (System.Exception e) {
            latex.Append(errorLaTeX(e.Message));
          }
        }
        TryOutput(nameof(entity.Simplify), () => latex.Append(entity.Simplify().Latexise()));
        TryOutput(nameof(entity.Expand), () => latex.Append(entity.Expand().Simplify().Latexise()));
        TryOutput("Factorize", () => latex.Append(entity.Collapse().Latexise()));
        TryOutput("Value", () => latex.Append(entity.Eval().ToString()));
        //TryOutput("Alternate forms", () => LatexiseAll(latex, entity.Alternate(5)));
        latex.Append(@"\end{aligned}");
        return latex.ToString();
      }, errorLaTeX);
    }
  }
}
