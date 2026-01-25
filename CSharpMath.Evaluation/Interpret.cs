using System.Linq;
using System.Text;

namespace CSharpMath {
  static partial class Evaluation {
    static StringBuilder AppendLaTeX(this StringBuilder sb, AngouriMath.Core.ILatexiseable latex) =>
      sb.Append(latex.Latexise());
    static StringBuilder AppendLaTeXHeader(this StringBuilder sb, string header, bool includeNewlineBefore = true) {
      if (includeNewlineBefore) sb.Append(@"\\\\");
      return sb.Append(@"\underline\mathrm{").Append(header).Append(@"}\\");
    }
    public static string Interpret(Atom.MathList mathList, System.Func<string, string>? errorLaTeX = null) {
      errorLaTeX ??= error => $@"\color{{red}}\text{{{Atom.LaTeXParser.EscapeAsLaTeX(error)}}}";

      var latex = new StringBuilder();
      (Atom.MathList left, Atom.MathList right)? equation = null;
      for (var i = 0; i < mathList.Count; i++) {
        if (mathList[i] is Atom.Atoms.Relation { Nucleus: "=" })
          if (equation == null)
            equation = (mathList.Slice(0, i), mathList.Slice(i + 1, mathList.Count - i - 1));
          else
            return errorLaTeX("Only 0 or 1 =s are supported");
      }
      if (equation is var (left, right)) {
        if (left.IsEmpty())
          return errorLaTeX("Missing left side of equation");
        if (right.IsEmpty())
          return errorLaTeX("Missing right side of equation");
        return Evaluate(left).Bind(x => (MathItem?)x).ExpectEntity("left side of equation").Bind(left =>
          Evaluate(right).Bind(x => (MathItem?)x).ExpectEntity("right side of equation").Bind(right => {
            latex.AppendLaTeXHeader("Input", false).AppendLaTeX(left).Append("=").AppendLaTeX(right);
            var entity = left - right;
            var variables = entity.Vars;
            if (variables.IsEmpty())
              latex.AppendLaTeXHeader("Result").Append($@"\text{{{entity.Evaled == 0}}}");
            else {
              latex.AppendLaTeXHeader("Solutions");
              foreach (AngouriMath.Entity.Variable variable in variables)
                latex.AppendLaTeX(variable).Append(@"\in ").AppendLaTeX(entity.SolveEquation(variable)).Append(@"\\");
            }
            return latex.ToString();
          })).Match(success => success, errorLaTeX);
      }
      return Evaluate(mathList)
        .Match(item => {
          latex.AppendLaTeXHeader("Input", false).AppendLaTeX(item);
          switch (item) {
            case MathItem.Entity { Content: var entity }:
              latex.AppendLaTeXHeader("Simplified").AppendLaTeX(entity.Simplify());
              if (entity.Evaled is AngouriMath.Entity.Number c)
                latex.AppendLaTeXHeader($@"Value\ ({AngouriMath.MathS.Settings.DecimalPrecisionContext.Value.Precision}\ digits)").AppendLaTeX(c);
              else {
                latex.AppendLaTeXHeader("Expanded").AppendLaTeX(entity.Expand());
                latex.AppendLaTeXHeader("Factorized").AppendLaTeX(entity.Factorize());
              }
              break;
            case MathItem.Comma _:
              break;
            default:
              throw new System.NotImplementedException(item.GetType().ToString());
          }
          return latex.ToString();
        }, errorLaTeX);
    }
  }
}
