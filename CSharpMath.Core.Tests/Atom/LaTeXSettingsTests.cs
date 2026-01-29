using System.Linq;
using Xunit;
namespace CSharpMath.Core.Tests {
  using CSharpMath.Atom;
  using CSharpMath.Atom.Atoms;
  using static LaTeXParserTest;
  using Atoms = CSharpMath.Atom.Atoms;
  [CollectionDefinition("No parallelization because of LaTeXSettings mutation", DisableParallelization = true)]
  [Collection("No parallelization because of LaTeXSettings mutation")]
  public class LaTeXSettingsTests {
    [Fact]
    public void ForAsciiHandlesAllInputs() {
      for (char i = '\0'; i <= sbyte.MaxValue; i++)
        switch (i) {
          case '\\': // The command character is handled specially
          case '$': // Unimplemented
          case '#': // Unimplemented
          case '~': // Unimplemented
            Assert.DoesNotContain(LaTeXSettings.Commands, kvp => kvp.Key == i.ToString());
            break;
          default:
            Assert.Contains(LaTeXSettings.Commands, kvp => kvp.Key == i.ToString());
            break;
        }
    }
    [Fact]
    public void TestCustom() {
      var input = @"\lcm(a,b)";
      var builder = new LaTeXParser(input);
      var (list, error) = builder.Build();
      Assert.Null(list);
      Assert.Equal(@"Invalid command \lcm", error);

      LaTeXSettings.CommandSymbols.Add(@"\lcm", new LargeOperator("lcm", false));
      list = ParseLaTeX(input);
      Assert.Collection(list,
        CheckAtom<LargeOperator>("lcm"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"\lcm (a,b)", LaTeXParser.MathListToLaTeX(list).ToString());

      LaTeXSettings.CommandSymbols.Add(@"lcm", new LargeOperator("lcm", false));
      LaTeXSettings.CommandSymbols.Add(@"lcm12", new LargeOperator("lcm12", false));
      LaTeXSettings.CommandSymbols.Add(@"lcm1234", new LargeOperator("lcm1234", false));
      LaTeXSettings.CommandSymbols.Add(@"lcm1235", new LargeOperator("lcm1235", false));

      // Does not match custom atoms added above
      list = ParseLaTeX("lc(a,b)");
      Assert.Collection(list,
        CheckAtom<Variable>("l"),
        CheckAtom<Variable>("c"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"lc(a,b)", LaTeXParser.MathListToLaTeX(list).ToString());

      // Baseline for lookup as a non-command (not starting with \)
      list = ParseLaTeX("lcm(a,b)");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("lcm"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"\lcm (a,b)", LaTeXParser.MathListToLaTeX(list).ToString());

      // Originally in https://github.com/verybadcat/CSharpMath/pull/143,
      // the non-command dictionary of LaTeXCommandDictionary were implemented with a trie.
      // With the above LaTeXSettings.CommandSymbols.Add calls, it would have looked like:
      // [l] -> l[cm] -> lcm[12] -> @lcm12[3] -> lcm123[4]
      //                                    ^--> lcm123[5]
      // where [square brackets] denote added characters compared to previous node
      // and the @at sign denotes the node without an atom to provide
      // Here we ensure that all behaviours of the trie carry over to the new SortedSet implementation

      // Test lookup fallbacks when trie node key (lcm12) does not fully match input (lcm1)
      list = ParseLaTeX("lcm1(a,b)");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("lcm"),
        CheckAtom<Number>("1"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"\lcm 1(a,b)", LaTeXParser.MathListToLaTeX(list).ToString());

      // Test lookup success for trie node between above case and below case
      list = ParseLaTeX("lcm12(a,b)");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("lcm12"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"lcm12(a,b)", LaTeXParser.MathListToLaTeX(list).ToString());

      // Test lookup fallbacks when trie node key (lcm123) fully matches input (lcm123) but has no atoms to provide
      list = ParseLaTeX("lcm123(a,b)");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("lcm12"),
        CheckAtom<Number>("3"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"lcm123(a,b)", LaTeXParser.MathListToLaTeX(list).ToString());

      // Add a new shorter entry to ensure that the longest key matches instead of the last one
      LaTeXSettings.CommandSymbols.Add(@"lcm123", new LargeOperator("lcm123", false));
      list = ParseLaTeX("lcm1234(a,b)");
      Assert.Collection(list,
        CheckAtom<LargeOperator>("lcm1234"),
        CheckAtom<Open>("("),
        CheckAtom<Variable>("a"),
        CheckAtom<Punctuation>(","),
        CheckAtom<Variable>("b"),
        CheckAtom<Close>(")")
      );
      Assert.Equal(@"lcm1234(a,b)", LaTeXParser.MathListToLaTeX(list).ToString());
    }
    [Fact]
    public void CommandForAtomIgnoresInnerLists() {
      var atom = new Atoms.Accent("\u0308", new MathList(new Atoms.Number("1")));
      atom.Superscript.Add(new Atoms.Number("4"));
      atom.Subscript.Add(new Atoms.Variable("x"));
      Assert.Equal(@"\ddot", LaTeXSettings.CommandForAtom(atom));
    }
    [Fact]
    public void AtomForCommandGeneratesACopy() {
      var atom = LaTeXSettings.AtomForCommand(@"\int");
      Assert.NotNull(atom);
      atom.IndexRange = Range.NotFound;
      var atom2 = LaTeXSettings.AtomForCommand(@"\int");
      Assert.NotNull(atom2);
      Assert.Equal(Range.Zero, atom2.IndexRange);
    }
  }
}