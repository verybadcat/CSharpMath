using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace CSharpMath.EvaluationTests {
  using Atom;

  /// <summary>
  /// <see cref="Evaluation.Visualize"/> throws <c>InvalidCodePathException</c> on any LaTeX it
  /// cannot read, and says why in its own source: "CSharpMath must handle all LaTeX coming from
  /// AngouriMath or a bug is present!". Nothing checked that, on either side — AngouriMath's own
  /// <c>Docs/Usage/Syntax.md</c> says the LaTeX round trip "is checked in someone else's
  /// repository", meaning this one. So this sweeps every node AngouriMath can print and asserts
  /// the LaTeX comes back through the parser. Each failure here is a crash waiting for a user.
  /// </summary>
  public class AngouriMathLatexSweepTests {
    /// <param name="Strict">
    /// False for nodes built by reflection: filling every argument with <c>x</c> can produce a node
    /// that is not well-formed, so a throw out of <c>Latexize</c> is not evidence of a defect. The
    /// hand-built shapes below are strict, because those are known-good expressions.
    /// </param>
    record Case(string Name, Entity Node, bool Strict);

    static IEnumerable<Case> Nodes() {
      var x = MathS.Var("x");
      var y = MathS.Var("y");
      foreach (var t in typeof(Entity).Assembly.GetTypes()
                 .Where(t => typeof(Entity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition)
                 .OrderBy(t => t.Name, StringComparer.Ordinal)) {
        Entity? built = null;
        foreach (var ctor in t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .OrderBy(c => c.GetParameters().Length)) {
          var ps = ctor.GetParameters();
          if (ps.Length is 0 or > 4) continue;
          if (!ps.All(p => typeof(Entity).IsAssignableFrom(p.ParameterType))) continue;
          try { built = (Entity)ctor.Invoke(ps.Select(_ => (object)x).ToArray()); break; } catch { }
        }
        if (built is not null) yield return new(t.Name, built, false);
      }
      // Shapes reflection cannot reach, listed by hand because they are exactly the ones whose
      // LaTeX is unusual.
      Case Strict(string name, Entity node) => new(name, node, true);
      yield return Strict("Matrix", MathS.Vector(1, 2, 3));
      yield return Strict("Matrix2x2", MathS.Matrix(new Entity[,] { { 1, 2 }, { 3, 4 } }));
      yield return Strict("Piecewise", MathS.Piecewise((x, x > 0), (y, y > 0)));
      yield return Strict("Integral", MathS.Integral(x, x));
      yield return Strict("IntegralRanged", MathS.Integral(x, x, 0, 1));
      yield return Strict("Derivative", MathS.Derivative(x, x));
      yield return Strict("Limit", MathS.Limit(x, x, 0));
      yield return Strict("Interval", new Entity.Set.Interval(0, true, 1, true));
      yield return Strict("FiniteSet", new Entity.Set.FiniteSet(1, 2, 3));
      yield return Strict("ConditionalSet", "{ x : x > 0 }".ToEntity());
      yield return Strict("Integers", "ZZ".ToEntity());
      yield return Strict("Reals", "RR".ToEntity());
      yield return Strict("Complexes", "CC".ToEntity());
      yield return Strict("Rationals", "QQ".ToEntity());
      yield return Strict("Booleans", "BB".ToEntity());
      yield return Strict("Rational", "3/2".ToEntity());
      yield return Strict("ComplexNumber", MathS.Numbers.Create(1, 2));
      yield return Strict("Factorial", MathS.Factorial(x));
      yield return Strict("Union", MathS.Union("A".ToEntity(), "B".ToEntity()));
      yield return Strict("Intersection", MathS.Intersection("A".ToEntity(), "B".ToEntity()));
      yield return Strict("SetMinus", MathS.SetSubtraction("A".ToEntity(), "B".ToEntity()));
      yield return Strict("In", "x in RR".ToEntity());
      yield return Strict("Provided", "x provided x > 0".ToEntity());
      yield return Strict("Apply", "apply(f, x)".ToEntity());
      yield return Strict("Lambda", "lambda(x, x^2)".ToEntity());
      yield return Strict("Domain", "domain(x, RR)".ToEntity());
      yield return Strict("Signum", MathS.Signum(x));
      yield return Strict("Abs", MathS.Abs(x));
      yield return Strict("Modulo", MathS.Mod(x, y));
      yield return Strict("EulerTotient", MathS.NumberTheory.Phi(x));
    }

    [Fact]
    public void EveryAngouriMathNodeLatexizesIntoSomethingCSharpMathCanParse() {
      var failures = new List<string>();
      var checkedCount = 0;
      foreach (var (name, node, strict) in Nodes()) {
        string latex;
        try {
          latex = node.Latexize();
        } catch (Exception e) {
          if (strict) failures.Add($"{name}: Latexize threw {e.GetType().Name}: {e.Message}");
          continue;
        }
        checkedCount++;
        LaTeXParser.MathListFromLaTeX(latex)
          .Match(_ => { }, err => failures.Add($"{name}: {latex}{Environment.NewLine}      -> {err}"));
      }
      // A guard that checks nothing would also report no failures.
      Assert.True(checkedCount > 50, $"only {checkedCount} nodes reached the parser");
      Assert.True(failures.Count == 0,
        $"checked {checkedCount} nodes, {failures.Count} unparseable:{Environment.NewLine}  "
        + string.Join(Environment.NewLine + "  ", failures));
    }
  }
}
