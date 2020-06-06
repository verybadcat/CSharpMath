using System.Linq;
using System.Text;
namespace CSharpMath {
  partial class Evaluation {
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
        return Evaluate(mathList)
        .Match(item => {
          switch (item) {
            case MathItem.Entity { Content: var entity }:
              const string variableDelimiter = @"\\";
              var variables = AngouriMath.MathS.Utils.GetUniqueVariables(entity).FiniteSet().Cast<AngouriMath.VariableEntity>();
              if (variables.IsEmpty())
                return $@"\text{{{entity.Eval() == 0}}}";
              var latex = new StringBuilder();
              foreach (AngouriMath.VariableEntity variable in variables) {
                latex.Append(variable).Append(@"\in ");
                try {
                  latex.Append(entity.SolveEquation(variable).Latexise());
                } catch (System.Exception e) {
                  latex.Append(errorLaTeX(e.Message));
                }
                latex.Append(variableDelimiter);
              }
              // Remove last variableDelimiter
              return latex.Remove(latex.Length - variableDelimiter.Length, variableDelimiter.Length).ToString();
            case MathItem.Comma _:
              return errorLaTeX("Comma equations are unsupported");
            case MathItem.Set _:
              return errorLaTeX("Set equations are unsupported");
            default:
              throw new System.NotImplementedException(item.GetType().ToString());
          }
        }, errorLaTeX);
      }

      return Evaluate(mathList)
      .Match(item => {
        var latex = new StringBuilder(@"\begin{aligned} &");
        latex.Append(item.Latexise());
        void TryOutput(string lineName, System.Func<AngouriMath.Core.Sys.Interfaces.ILatexiseable> get) {
          latex.Append(@"\\ \text{").Append(lineName).Append(@"} \colon & \;");
          try {
            latex.Append(get().Latexise());
          } catch (System.Exception e) {
            latex.Append(errorLaTeX(e.Message));
          }
        }
        switch (item) {
          case MathItem.Entity { Content: var entity }:
            TryOutput(nameof(entity.Simplify), entity.Simplify);
            TryOutput(nameof(entity.Expand), () => entity.Expand().Simplify());
            TryOutput("Factorize", entity.Collapse);
            TryOutput("Value", entity.Eval);
            foreach (AngouriMath.VariableEntity variable in AngouriMath.MathS.Utils.GetUniqueVariables(entity).FiniteSet())
              TryOutput(@"\mathnormal{\frac\partial{\partial " + variable.Latexise() + "}}",
                () => entity.Derive(variable).Simplify());
            // TryOutput("Alternate forms", () => entity.Alternate(5));
            break;
          case MathItem.Set _:
          case MathItem.Comma _:
            break;
          default:
            throw new System.NotImplementedException(item.GetType().ToString());
        }
        latex.Append(@"\end{aligned}");
        return latex.ToString();
      }, errorLaTeX);
    }
  }
}
