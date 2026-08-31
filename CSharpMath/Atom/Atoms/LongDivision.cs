using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
namespace CSharpMath.Atom.Atoms {
  public sealed class LongDivisionStep {
    public string PartialDividend { get; }
    public int QuotientDigit { get; }
    public string Product { get; }
    public string Remainder { get; }
    public char BroughtDownDigit { get; }
    public int DecimalColumn { get; }
    public LongDivisionStep(string partialDividend, int quotientDigit, string product, string remainder, char broughtDownDigit, int decimalColumn) {
      PartialDividend = partialDividend ?? throw new ArgumentNullException(nameof(partialDividend));
      Product = product ?? throw new ArgumentNullException(nameof(product));
      Remainder = remainder ?? throw new ArgumentNullException(nameof(remainder));
      if (quotientDigit < 0 || quotientDigit > 9) throw new ArgumentOutOfRangeException(nameof(quotientDigit));
      if (broughtDownDigit < '0' || broughtDownDigit > '9') throw new ArgumentOutOfRangeException(nameof(broughtDownDigit));
      if (decimalColumn < 0) throw new ArgumentOutOfRangeException(nameof(decimalColumn));
      (PartialDividend, QuotientDigit, Product, Remainder, BroughtDownDigit, DecimalColumn) = (partialDividend, quotientDigit, product, remainder, broughtDownDigit, decimalColumn);
    }
  }
  /// <summary>Semantic representation of a bounded decimal long division.</summary>
  public sealed class LongDivision : MathAtom, IMathListContainer {
    // decimal is used deliberately to keep the operation bounded and AOT-safe.
    public const int MaxDigits = 28;
    public string Numerator { get; }
    public string Denominator { get; }
    public string QuotientText { get; }
    public string Remainder { get; }
    public IReadOnlyList<LongDivisionStep> Steps { get; }
    public LongDivision(string numerator, string denominator) : base() {
      Numerator = Validate(numerator, nameof(numerator)); Denominator = Validate(denominator, nameof(denominator));
      if (Denominator.TrimStart('0').Length == 0) throw new ArgumentException("Denominator must not be zero.", nameof(denominator));
      var n = BigInteger.Parse(Numerator, CultureInfo.InvariantCulture); var d = BigInteger.Parse(Denominator, CultureInfo.InvariantCulture);
      QuotientText = (n / d).ToString(CultureInfo.InvariantCulture); Remainder = (n % d).ToString(CultureInfo.InvariantCulture); Steps = BuildSteps(d);
    }
    static string Validate(string value, string name) { if (string.IsNullOrWhiteSpace(value) || value.Length > MaxDigits || value.Any(c => c < '0' || c > '9')) throw new ArgumentException("Expected a nonnegative decimal integer of at most 28 digits.", name); var t = value.TrimStart('0'); return t.Length == 0 ? "0" : t; }
    MathList Digits(string text) => new MathList(new Number(text));
    IReadOnlyList<LongDivisionStep> BuildSteps(BigInteger divisor) {
      var result = new List<LongDivisionStep>(); var partial = BigInteger.Zero;
      for (int i = 0; i < Numerator.Length; i++) { partial = partial * 10 + (Numerator[i] - '0'); var q = (int)(partial / divisor); var product = q * divisor; partial -= product; result.Add(new LongDivisionStep((partial + product).ToString(CultureInfo.InvariantCulture), q, product.ToString(CultureInfo.InvariantCulture), partial.ToString(CultureInfo.InvariantCulture), Numerator[i], i)); }
      return result.AsReadOnly();
    }
    internal Table CreateLayout() {
      var table = new Table { InterColumnSpacing = 1, InterRowAdditionalSpacing = 1 };
      table.SetCell(Digits(QuotientText), 0, 1);
      table.SetCell(Digits(Denominator), 1, 0);
      table.SetCell(new MathList(new LongDivisionHeader(Digits(Numerator))), 1, 1);
      table.SetAlignment(ColumnAlignment.Right, 0);
      table.SetAlignment(ColumnAlignment.Right, 1);

      var row = 2;
      foreach (var step in Steps.Where(step => step.QuotientDigit > 0)) {
        var trailingZeroes = new string('0', Numerator.Length - step.DecimalColumn - 1);
        var product = step.Product + trailingZeroes;
        table.SetCell(new MathList(new Underline(Digits(product))), row++, 1);

        // Bring down every remaining dividend digit so place value is retained.
        var runningText = step.Remainder + Numerator.Substring(step.DecimalColumn + 1);
        var running = BigInteger.Parse(runningText, CultureInfo.InvariantCulture)
          .ToString(CultureInfo.InvariantCulture);
        table.SetCell(Digits(running), row++, 1);
      }
      if (!Steps.Any(step => step.QuotientDigit > 0))
        table.SetCell(Digits(Remainder), row, 1);
      return table;
    }
    public IEnumerable<MathList> InnerLists => CreateLayout().Cells.SelectMany(r => r);
    public override bool ScriptsAllowed => false;
    public new LongDivision Clone(bool finalize) => (LongDivision)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new LongDivision(Numerator, Denominator);
    public override string DebugString => $@"\longdiv{{{Numerator}}}{{{Denominator}}}";
    public override bool Equals(object obj) => obj is LongDivision d && EqualsAtom(d) && Numerator == d.Numerator && Denominator == d.Denominator;
    public override int GetHashCode() => (base.GetHashCode(), Numerator, Denominator).GetHashCode();
  }

  // The long-division bar and closing delimiter have a font-specific junction
  // which cannot be represented by a generic Overline around an Inner.
  internal sealed class LongDivisionHeader : MathAtom, IMathListContainer {
    internal MathList Dividend { get; }
    internal LongDivisionHeader(MathList dividend) => Dividend = dividend ?? throw new ArgumentNullException(nameof(dividend));
    public IEnumerable<MathList> InnerLists => new[] { Dividend };
    public override bool ScriptsAllowed => false;
    protected override MathAtom CloneInside(bool finalize) => new LongDivisionHeader(Dividend.Clone(finalize));
    public override string DebugString => $@"\longdivheader{{{Dividend.DebugString}}}";
  }
}
