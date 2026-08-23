using System;
using System.Linq;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using Xunit;

namespace CSharpMath.Core.AtomTests {
  // Tests for the features ported from iosMath's 2026 commits.
  public class IosMath2026PortTests {
    static MathList ParseLaTeX(string latex) {
      var builder = new LaTeXParser(latex);
      var (mathList, error) = builder.Build();
      Assert.Null(error);
      Assert.NotNull(mathList);
      return mathList;
    }
    static string RoundTrip(string latex) => LaTeXParser.MathListToLaTeX(ParseLaTeX(latex)).ToString();
    static Action<MathAtom> CheckAtom<T>(string nucleus) where T : MathAtom =>
      atom => {
        var typed = Assert.IsType<T>(atom);
        Assert.Equal(nucleus, typed.Nucleus);
      };

    #region Fraction family (\dfrac \tfrac \dbinom \tbinom \cfrac) — iosMath 58b1f8d
    [Fact]
    public void DfracParsesWithDisplayStyleOverride() {
      var list = ParseLaTeX(@"\dfrac1c");
      var frac = Assert.IsType<Fraction>(Assert.Single(list));
      Assert.True(frac.HasRule);
      Assert.Equal(FractionStyle.Display, frac.StyleOverride);
      Assert.Equal(@"\frac{\displaystyle{1}}{\displaystyle{c}}", RoundTrip(@"\dfrac1c"));
    }
    [Fact]
    public void TfracParsesWithTextStyleOverride() {
      var frac = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"\tfrac1c")));
      Assert.True(frac.HasRule);
      Assert.Equal(FractionStyle.Text, frac.StyleOverride);
      Assert.Equal(@"\frac{\textstyle{1}}{\textstyle{c}}", RoundTrip(@"\tfrac1c"));
    }
    [Fact]
    public void DbinomAndTbinomParse() {
      var dbinom = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"\dbinom{n}{k}")));
      Assert.False(dbinom.HasRule);
      Assert.Equal(FractionStyle.Display, dbinom.StyleOverride);
      Assert.Equal(new Boundary("("), dbinom.LeftDelimiter);
      var tbinom = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"\tbinom{n}{k}")));
      Assert.False(tbinom.HasRule);
      Assert.Equal(FractionStyle.Text, tbinom.StyleOverride);
    }
    [Fact]
    public void CfracParsesWithAlignment() {
      var cfrac = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"\cfrac[l]{a}{b}")));
      Assert.True(cfrac.HasRule);
      Assert.True(cfrac.IsContinuedFraction);
      Assert.Equal(FractionAlignment.Left, cfrac.NumeratorAlignment);
      var right = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"\cfrac[r]{a}{b}")));
      Assert.Equal(FractionAlignment.Right, right.NumeratorAlignment);
      // Invalid alignment is a parse error.
      var builder = new LaTeXParser(@"\cfrac[zzz]{a}{b}");
      Assert.NotNull(builder.Build().Error);
    }
    [Fact]
    public void PlainFracKeepsAutoStyle() {
      var frac = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"\frac1c")));
      Assert.Equal(FractionStyle.Auto, frac.StyleOverride);
      Assert.False(frac.IsContinuedFraction);
      Assert.Equal(@"\frac{1}{c}", RoundTrip(@"\frac1c"));
    }
    #endregion

    #region Over/under stack commands — iosMath 43626a4 + 94d8edf
    [Theory]
    [InlineData("overrightarrow", "→", true)]
    [InlineData("overleftarrow", "←", true)]
    [InlineData("overleftrightarrow", "↔", true)]
    [InlineData("underrightarrow", "→", false)]
    [InlineData("underleftarrow", "←", false)]
    [InlineData("underleftrightarrow", "↔", false)]
    [InlineData("overbrace", "⏞", true)]
    public void StretchyStackCommandsParse(string command, string glyph, bool hasOver) {
      var stack = Assert.IsType<Stack>(Assert.Single(ParseLaTeX($@"\{command}{{x}}")));
      Assert.Equal(typeof(Ordinary), stack.DisplayClassType);
      Assert.Single(stack.InnerList);
      if (hasOver) {
        var extensible = Assert.IsType<StackConstruction.Extensible>(stack.Over);
        Assert.Equal(glyph, extensible.Glyph);
        Assert.Null(stack.Under);
      } else {
        var extensible = Assert.IsType<StackConstruction.Extensible>(stack.Under);
        Assert.Equal(glyph, extensible.Glyph);
        Assert.Null(stack.Over);
      }
      Assert.Equal($@"\{command}{{x}}", RoundTrip($@"\{command}{{x}}"));
    }
    // \underbrace keeps its pre-port UnderAnnotation registration (its _ attaches an
    // under-list), which the iosMath Stack port does not replace.
    [Fact]
    public void UnderbraceStaysAnUnderAnnotation() {
      var underbrace = Assert.IsType<UnderAnnotation>(Assert.Single(ParseLaTeX(@"\underbrace{x}")));
      Assert.Equal("\u23df", underbrace.Nucleus);
    }
    [Fact]
    public void StackrelForcesRelationClass() {
      var stack = Assert.IsType<Stack>(Assert.Single(ParseLaTeX(@"\stackrel{?}{=}")));
      Assert.Equal(typeof(Relation), stack.DisplayClassType);
      var overRow = Assert.IsType<StackConstruction.MathListRow>(stack.Over);
      Assert.Single(overRow.List);
      Assert.Equal(@"\stackrel{?}{=}", RoundTrip(@"\stackrel{?}{=}"));
    }
    [Fact]
    public void StackbinForcesBinaryClass() {
      var stack = Assert.IsType<Stack>(Assert.Single(ParseLaTeX(@"\stackbin{x}{+}")));
      Assert.Equal(typeof(BinaryOperator), stack.DisplayClassType);
      Assert.Equal(@"\stackbin{x}{+}", RoundTrip(@"\stackbin{x}{+}"));
    }
    [Theory]
    [InlineData(@"\overset{!}{=}", typeof(Relation))]   // lone Rel base inherits Relation
    [InlineData(@"\overset{a}{+}", typeof(BinaryOperator))] // lone Bin base inherits Binary
    [InlineData(@"\overset{a}{x}", typeof(Ordinary))]
    public void OversetInheritsBaseClass(string latex, System.Type expectedClass) {
      var stack = Assert.IsType<Stack>(Assert.Single(ParseLaTeX(latex)));
      Assert.Equal(expectedClass, stack.DisplayClassType);
    }
    [Fact]
    public void UndersetParsesWithUnderRow() {
      var stack = Assert.IsType<Stack>(Assert.Single(ParseLaTeX(@"\underset{b}{x}")));
      Assert.NotNull(stack.Under);
      Assert.Null(stack.Over);
      Assert.Equal(typeof(Ordinary), stack.DisplayClassType);
      Assert.Equal(@"\underset{b}{x}", RoundTrip(@"\underset{b}{x}"));
    }
    [Fact]
    public void OversetCanonicalizesToStackrelOrStackbinOnRoundTrip() {
      // A lone Bin/Rel base canonicalizes to the dedicated command.
      Assert.Equal(@"\stackrel{!}{=}", RoundTrip(@"\overset{!}{=}"));
      Assert.Equal(@"\stackbin{a}{+}", RoundTrip(@"\overset{a}{+}"));
    }
    #endregion

    #region Box family: phantom/smash/lap + cancel — iosMath 9f53483 + d49f251
    [Theory]
    [InlineData(@"\phantom{x}", true, true, true, false)]
    [InlineData(@"\hphantom{x}", true, false, false, false)]
    [InlineData(@"\vphantom{x}", false, true, true, false)]
    [InlineData(@"\smash{x}", true, false, false, true)]
    public void PhantomFamilyParses(string latex, bool kW, bool kH, bool kD, bool draw) {
      var box = Assert.IsType<Box>(Assert.Single(ParseLaTeX(latex)));
      Assert.Equal(kW, box.KeepWidth);
      Assert.Equal(kH, box.KeepHeight);
      Assert.Equal(kD, box.KeepDepth);
      Assert.Equal(draw, box.DrawChild);
      Assert.Equal(StrikeStyle.None, box.StrikeStyle);
    }
    [Fact]
    public void MathStrutSynthesizesParen() {
      var box = Assert.IsType<Box>(Assert.Single(ParseLaTeX(@"\mathstrut")));
      Assert.False(box.KeepWidth);
      Assert.True(box.KeepHeight);
      Assert.True(box.KeepDepth);
      var paren = Assert.IsType<Open>(Assert.Single(box.InnerList));
      Assert.Equal("(", paren.Nucleus);
      // Lossy round trip: \mathstrut serializes as \vphantom{(}
      Assert.Equal(@"\vphantom{(}", RoundTrip(@"\mathstrut"));
    }
    [Fact]
    public void SmashOptionalArgSelectsAxes() {
      var smashT = Assert.IsType<Box>(Assert.Single(ParseLaTeX(@"\smash[t]{x}")));
      Assert.False(smashT.KeepHeight);
      Assert.True(smashT.KeepDepth);
      var smashB = Assert.IsType<Box>(Assert.Single(ParseLaTeX(@"\smash[b]{x}")));
      Assert.True(smashB.KeepHeight);
      Assert.False(smashB.KeepDepth);
      // Unknown optional value degrades to plain smash without error.
      var smashQ = Assert.IsType<Box>(Assert.Single(ParseLaTeX(@"\smash[q]{x}")));
      Assert.False(smashQ.KeepHeight);
      Assert.False(smashQ.KeepDepth);
    }
    [Theory]
    [InlineData(@"\llap{x}", BoxHAlign.Right)]
    [InlineData(@"\rlap{x}", BoxHAlign.Left)]
    [InlineData(@"\clap{x}", BoxHAlign.Center)]
    [InlineData(@"\mathllap{x}", BoxHAlign.Right)]
    [InlineData(@"\mathrlap{x}", BoxHAlign.Left)]
    [InlineData(@"\mathclap{x}", BoxHAlign.Center)]
    public void LapsParse(string latex, BoxHAlign align) {
      var box = Assert.IsType<Box>(Assert.Single(ParseLaTeX(latex)));
      Assert.False(box.KeepWidth);
      Assert.True(box.DrawChild);
      Assert.Equal(align, box.HAlign);
      // Single-atom groups serialize bare; laps keep their command since the inner
      // atom is wrapped by the box itself.
      Assert.Contains(RoundTrip(latex), new[] { @"x", @"\llap{x}", @"\rlap{x}", @"\clap{x}" });
    }
    [Theory]
    [InlineData(@"\cancel{x}", StrikeStyle.Forward)]
    [InlineData(@"\bcancel{x}", StrikeStyle.Backward)]
    [InlineData(@"\xcancel{x}", StrikeStyle.Cross)]
    [InlineData(@"\sout{x}", StrikeStyle.Horizontal)]
    public void CancelFamilyParses(string latex, StrikeStyle strike) {
      var box = Assert.IsType<Box>(Assert.Single(ParseLaTeX(latex)));
      Assert.True(box.KeepWidth && box.KeepHeight && box.KeepDepth && box.DrawChild);
      Assert.Equal(strike, box.StrikeStyle);
      Assert.Equal(latex, RoundTrip(latex));
    }
    [Fact]
    public void CancelAtEofYieldsEmptyBox() {
      var box = Assert.IsType<Box>(Assert.Single(ParseLaTeX(@"\cancel")));
      Assert.Equal(StrikeStyle.Forward, box.StrikeStyle);
      Assert.Empty(box.InnerList);
    }
    #endregion

    #region Modular arithmetic macros — iosMath 035a9e2 + e278be3 + 4091668
    [Theory]
    [InlineData(@"\pmod{n}", 1)]
    [InlineData(@"\mod{n}", 1)]
    [InlineData(@"\pod{n}", 1)]
    [InlineData(@"x\implies y", 0)]
    [InlineData(@"x\iff y", 0)]
    [InlineData(@"\varliminf", 0)]
    public void MacrosRoundTrip(string latex, int argc) {
      var macro = Assert.Single(ParseLaTeX(latex), a => a is Macro) as Macro;
      Assert.NotNull(macro);
      Assert.Equal(argc, macro.Arguments.Count);
      Assert.NotEmpty(LaTeXSettings.BuiltinMacros[macro.Command].template);
      // Zero-arg macros keep a trailing space so the name terminates; the others
      // round-trip byte-identically.
      Assert.Equal(latex, RoundTrip(latex).TrimEnd());
    }
    [Fact]
    public void PmodExpandsToParenthesizedMod() {
      var finalized = ParseLaTeX(@"\pmod{n}").Clone(true);
      // Expansion: mkern8mu ( m o d mkern6mu n ) — \mathrm{mod} contributes three
      // roman atoms, so the flat expansion is 8 atoms wide.
      Assert.DoesNotContain(finalized, a => a is Macro);
      Assert.Equal(8, finalized.Count);
      Assert.IsType<Space>(finalized[0]);
      Assert.IsType<Open>(finalized[1]);
      Assert.IsType<Close>(finalized[7]);
    }
    [Fact]
    public void MacroScriptsTransferToExpansion() {
      var finalized = ParseLaTeX(@"\pmod{n}^2").Clone(true);
      // The superscript lands on the closing paren of the expansion.
      var last = Assert.IsType<Close>(finalized.Last);
      Assert.Single(last.Superscript);
    }
    [Fact]
    public void MissingMacroArgumentIsError() {
      Assert.NotNull(new LaTeXParser(@"\pmod").Build().Error);
      Assert.NotNull(new LaTeXParser(@"\mod").Build().Error);
      Assert.NotNull(new LaTeXParser(@"\pod").Build().Error);
    }
    [Fact]
    public void BmodStaysABinaryOperator() {
      // \bmod is a plain symbol table entry, not a macro. After finalize the leading
      // number fuses and the trailing number follows the binary op.
      var finalized = ParseLaTeX(@"17 \bmod 5").Clone(true);
      Assert.Collection(finalized,
        CheckAtom<Number>("17"),
        atom => {
          var mod = Assert.IsType<BinaryOperator>(atom);
          Assert.Equal("mod", mod.Nucleus);
        },
        CheckAtom<Number>("5"));
    }
    #endregion

    #region Spacing commands in math mode — iosMath f644371
    [Fact]
    public void KernAcceptsEmAndMuInMathMode() {
      var kern = Assert.IsType<Space>(Assert.Single(ParseLaTeX(@"\kern1em")));
      Assert.True(kern.IsMu);
      Assert.Equal(18, kern.Length); // em == 18mu
      var mkern = Assert.IsType<Space>(Assert.Single(ParseLaTeX(@"\mkern3mu")));
      Assert.Equal(3, mkern.Length);
    }
    [Fact]
    public void HspaceParsesInBraces() {
      var space = Assert.IsType<Space>(Assert.Single(ParseLaTeX(@"\hspace{1em}")));
      Assert.Equal(18, space.Length);
      var negative = Assert.IsType<Space>(Assert.Single(ParseLaTeX(@"\hspace{-0.5em}")));
      Assert.Equal(-9, negative.Length);
    }
    [Fact]
    public void NamedAmsmathSpacesParse() {
      foreach (var (command, length) in new[] {
        (@"\thinspace", 3f), (@"\medspace", 4f), (@"\thickspace", 5f),
        (@"\negthinspace", -3f), (@"\negmedspace", -4f), (@"\negthickspace", -5f),
      }) {
        var space = Assert.IsType<Space>(Assert.Single(ParseLaTeX(command)));
        Assert.True(space.IsMu);
        Assert.Equal(length, space.Length);
      }
    }
    #endregion

    #region Environments: smallmatrix/gathered/alignedat/array rules — iosMath 8d52b86/dd76eb5/cc62444
    [Fact]
    public void SmallmatrixUsesScriptCells() {
      var table = Assert.IsType<Table>(Assert.Single(
        ParseLaTeX(@"\begin{smallmatrix} x & y \\ z & w \end{smallmatrix}")));
      Assert.Equal("smallmatrix", table.Environment);
      Assert.Equal(LineStyle.Script, table.CellStyle);
      Assert.Equal(5, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(@"\begin{smallmatrix}x&y\\ z&w\end{smallmatrix}",
        RoundTrip(@"\begin{smallmatrix} x & y \\ z & w \end{smallmatrix}"));
    }
    [Fact]
    public void GatheredInheritsStyle() {
      var table = Assert.IsType<Table>(Assert.Single(
        ParseLaTeX(@"\begin{gathered} x \\ y \end{gathered}")));
      Assert.Equal("gathered", table.Environment);
      Assert.Null(table.CellStyle); // inherits surrounding style
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(@"\begin{gathered}x\\ y\end{gathered}",
        RoundTrip(@"\begin{gathered} x \\ y \end{gathered}"));
    }
    [Fact]
    public void AlignedatReadsPairCountArgument() {
      var table = Assert.IsType<Table>(Assert.Single(
        ParseLaTeX(@"\begin{alignedat}{2} 10&x +& 3&y \\ 3&x +& 13&y \end{alignedat}")));
      Assert.Equal("alignedat", table.Environment);
      Assert.Equal(4, table.NColumns);
      Assert.Equal(ColumnAlignment.Right, table.GetAlignment(0));
      Assert.Equal(ColumnAlignment.Left, table.GetAlignment(1));
      Assert.Equal(ColumnAlignment.Right, table.GetAlignment(2));
      Assert.Equal(ColumnAlignment.Left, table.GetAlignment(3));
      Assert.Contains("{2}", RoundTrip(@"\begin{alignedat}{2} 10&x +& 3&y \\ 3&x +& 13&y \end{alignedat}"));
    }
    [Fact]
    public void AlignedatValidatesColumnCount() {
      // 3 columns != 2n for any n
      Assert.NotNull(new LaTeXParser(@"\begin{alignedat}{2} a&b&c \end{alignedat}").Build().Error);
      // Missing argument
      Assert.NotNull(new LaTeXParser(@"\begin{alignedat} a & b \end{alignedat}").Build().Error);
    }
    [Fact]
    public void ArrayParsesVerticalLines() {
      var table = Assert.IsType<Table>(Assert.Single(
        ParseLaTeX(@"\begin{array}{|r|c|l|} 10 & = & 7 + 3 \end{array}")));
      Assert.Equal("array", table.Environment);
      Assert.Equal(new[] { 1, 1, 1, 1 }, table.VerticalLines);
      Assert.Equal(ColumnAlignment.Right, table.GetAlignment(0));
      Assert.Equal(ColumnAlignment.Center, table.GetAlignment(1));
      Assert.Equal(ColumnAlignment.Left, table.GetAlignment(2));
      Assert.Equal(@"\begin{array}{|r|c|l|}10&=&7+3\end{array}",
        RoundTrip(@"\begin{array}{|r|c|l|} 10 & = & 7 + 3 \end{array}"));
    }
    [Fact]
    public void ArrayParsesHorizontalLines() {
      var table = Assert.IsType<Table>(Assert.Single(
        ParseLaTeX(@"\begin{array}{c} \hline a \\ \hline b \end{array}")));
      // The first \hline is boundary 0 (above row 0); the second lands on boundary 1
      // once the \\ advances the row counter.
      Assert.Equal(new[] { 1, 1, 0 }, table.HorizontalLines);
      Assert.Equal(2, table.NRows);
    }
    [Fact]
    public void HlineOutsideArrayIsError() {
      Assert.NotNull(new LaTeXParser(@"\hline a").Build().Error);
    }
    [Fact]
    public void ArrayToleratesExtraCells() {
      // Pre-port behavior: extra cells are dropped rather than erroring.
      Assert.Null(new LaTeXParser(@"\begin{array}{c} a & b \end{array}").Build().Error);
    }
    #endregion

    #region TeX-faithful brace grouping — iosMath 086d345
    [Fact]
    public void BracesCreateOrdGroups() {
      var group = Assert.IsType<Group>(ParseLaTeX(@"5{3+4}")[1]);
      Assert.Equal(3, group.InnerList.Count);
      Assert.Equal("5{3+4}", RoundTrip(@"5{3+4}"));
    }
    [Fact]
    public void ScriptsAttachToWholeGroup() {
      var group = Assert.IsType<Group>(Assert.Single(ParseLaTeX(@"{x}^2")));
      Assert.Single(group.Superscript);
    }
    [Fact]
    public void ScriptStyleDoesNotLeakPastGroup() {
      // The whole point of issue #177: z must not be scriptstyle.
      var list = ParseLaTeX(@"x{\scriptstyle y}z");
      Assert.Equal(3, list.Count);
      Assert.IsType<Group>(list[1]);
      var z = Assert.IsType<Variable>(list[2]);
      Assert.Equal(FontStyle.Default, z.FontStyle);
      Assert.Equal(@"x{\scriptstyle y}z", RoundTrip(@"x{\scriptstyle y}z"));
    }
    [Fact]
    public void OverTransformReplacesTheGroup() {
      // {a \over b} becomes a bare fraction (TeX group-transformation semantics).
      var frac = Assert.IsType<Fraction>(Assert.Single(ParseLaTeX(@"{a \over b}")));
      Assert.True(frac.HasRule);
      Assert.Equal(@"\frac{a}{b}", RoundTrip(@"{a \over b}"));
    }
    [Fact]
    public void NestedGroupsSurvive() {
      // The outer braces wrap a single Group atom, which serializes bare, so the
      // inner group's braces are what survive; both levels parse as Groups.
      var list = ParseLaTeX(@"{{a+b}}");
      var outer = Assert.IsType<Group>(Assert.Single(list));
      var inner = Assert.IsType<Group>(Assert.Single(outer.InnerList));
      Assert.Equal(3, inner.InnerList.Count);
      Assert.IsType<Variable>(inner.InnerList[0]);
    }
    #endregion

    #region Recursion cap + primes — iosMath ef6ac9f (primes pre-exist via the symbol table)
    [Fact]
    public void DeepNestingIsAParseErrorNotACrash() {
      const int depth = 1000;
      var input = new string('{', depth) + "1" + new string('}', depth);
      Assert.NotNull(new LaTeXParser(input).Build().Error);
    }
    [Fact]
    public void ModerateNestingStillParses() {
      const int depth = 20;
      var input = new string('{', depth) + "1" + new string('}', depth);
      Assert.Null(new LaTeXParser(input).Build().Error);
    }
    // Primes were already supported in CSharpMath via '''-style symbol table entries
    // (′ ″ ‴ ⁗), so iosMath's parser branch was not ported.
    [Fact]
    public void PrimesParseToPrimeGlyphs() {
      var single = ParseLaTeX(@"f'");
      Assert.Equal("′", single[^1].Nucleus);
      var doublePrime = ParseLaTeX(@"f''");
      Assert.Equal("″", doublePrime[^1].Nucleus);
      var triple = ParseLaTeX(@"f'''");
      Assert.Equal("‴", triple[^1].Nucleus);
    }
    #endregion

    #region Color validation + textcolor — iosMath 3471a4f/c27737a + REN-7
    [Fact]
    public void TextcolorIsAnAliasOfColor() {
      var colored = Assert.IsType<Colored>(Assert.Single(ParseLaTeX(@"\textcolor{#FF0000}{x}")));
      Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), colored.Color);
      // The serializer emits the canonical name for pure red.
      Assert.Equal(@"\color{red}{x}", RoundTrip(@"\textcolor{#FF0000}{x}"));
    }
    [Fact]
    public void RgbShorthandExpands() {
      var colored = Assert.IsType<Colored>(Assert.Single(ParseLaTeX(@"\color{#f00}x")));
      Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), colored.Color);
    }
    [Fact]
    public void InvalidColorsAreErrors() {
      Assert.NotNull(new LaTeXParser(@"\color{notacolor}{x}").Build().Error);
      Assert.NotNull(new LaTeXParser(@"\color{#gg0000}{x}").Build().Error); // non-hex
      Assert.NotNull(new LaTeXParser(@"\color{#ff00}{x}").Build().Error); // wrong length
    }
    [Fact]
    public void ValidNamedColorsStillParse() {
      var colored = Assert.IsType<Colored>(Assert.Single(ParseLaTeX(@"\color{red}x")));
      Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), colored.Color);
    }
    #endregion

    #region New symbols — iosMath 48f6fca + 19d98d3
    [Theory]
    [InlineData(@"\lt", "<")]
    [InlineData(@"\gt", ">")]
    [InlineData(@"\restriction", "↾")]
    [InlineData(@"\dotsc", "…")]
    public void NewAliasesParse(string command, string nucleus) {
      var atom = Assert.Single(ParseLaTeX(command));
      Assert.Equal(nucleus, atom.Nucleus);
    }
    #endregion

    #region Sqrt EOF safety — iosMath da9abdd
    [Fact]
    public void LoneSqrtAtEndDoesNotCrash() {
      var radical = Assert.IsType<Radical>(Assert.Single(ParseLaTeX(@"\sqrt")));
      Assert.Empty(radical.Radicand);
      Assert.True(radical.Degree.IsEmpty());
      Assert.Equal(@"\sqrt{}", RoundTrip(@"\sqrt"));
    }
    [Fact]
    public void SqrtInGroupDoesNotCrash() {
      Assert.Null(new LaTeXParser(@"{\sqrt}").Build().Error);
    }
    #endregion
  }
}
