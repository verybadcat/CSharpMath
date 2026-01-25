using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using System;
using System.Linq;
using Xunit;

namespace CSharpMath.Core.Tests {
  using Range = Atom.Range;
  public class MathListTest {
    internal static void CheckClone(MathAtom? original, MathAtom? clone) {
      Assert.Equal(original, clone);
      Assert.NotSame(original, clone);
    }
    internal static void CheckClone(MathList? original, MathList? clone) {
      Assert.Equal(original, clone);
      Assert.NotSame(original, clone);
    }

    [Fact]
    public void TestAdd() {
      var list = new MathList();
      Assert.Empty(list);
      var atom = LaTeXSettings.Placeholder;
      list.Add(atom);
      Assert.Single(list);
      Assert.Equal(atom, list[0]);
      var atom2 = LaTeXSettings.Times;
      list.Add(atom2);
      Assert.Equal(2, list.Count);
      Assert.Equal(atom2, list[1]);
    }

    [Fact]
    public void TestAddErrors() {
      var list = new MathList();
      Assert.Throws<ArgumentNullException>(() => list.Add(null!));
    }

    [Fact]
    public void TestInsert() {
      var list = new MathList();
      Assert.Empty(list);
      var atom = LaTeXSettings.Placeholder;
      list.Insert(0, atom);
      Assert.Single(list);
      Assert.Equal(atom, list[0]);
      var atom2 = new LargeOperator("+", false);
      list.Insert(0, atom2);
      Assert.Equal(2, list.Count);
      Assert.Equal(atom2, list[0]);
      Assert.Equal(atom, list[1]);
      var atom3 = new Variable("x");
      list.Insert(2, atom3);
      Assert.Equal(3, list.Count);
      Assert.Equal(atom2, list[0]);
      Assert.Equal(atom, list[1]);
      Assert.Equal(atom3, list[2]);
    }

    [Fact]
    public void TestAppend() {
      var list1 = new MathList();
      var atom1 = LaTeXSettings.Placeholder;
      var atom2 = new LargeOperator("+", false);
      var atom3 = new LargeOperator("-", false);
      list1.Add(atom1);
      list1.Add(atom2);
      list1.Add(atom3);

      var list2 = new MathList();
      var atom5 = LaTeXSettings.Times;
      var atom6 = LaTeXSettings.Divide;
      list2.Add(atom5);
      list2.Add(atom6);

      Assert.Equal(3, list1.Count);
      Assert.Equal(2, list2.Count);

      list1.Append(list2);
      Assert.Equal(5, list1.Count);
      Assert.Equal(atom5, list1[3]);
      Assert.Equal(atom6, list1[4]);
    }

    [Fact]
    public void TestRemoveAtomAtIndex() {
      var list = new MathList();
      var atom = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      list.Add(atom);
      list.Add(atom2);
      Assert.Equal(2, list.Count);
      list.RemoveAt(0);
      Assert.Single(list);
      Assert.Equal(atom2, list[0]);
      Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
      Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(2));
    }

    [Fact]
    public void TestRemoveAtomsInRange() {
      var list = new MathList();
      var atom = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      list.Add(atom);
      list.Add(atom2);
      list.Add(atom3);
      Assert.Equal(3, list.Count);
      list.RemoveAtoms(1, 2);
      Assert.Single(list);
      Assert.Equal(atom, list[0]);
      Assert.Throws<ArgumentException>(() => list.RemoveAtoms(1, 1));
    }

    [Fact]
    public void TestListCopy() {
      var list = new MathList();
      var atom = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      list.Add(atom);
      list.Add(atom2);
      list.Add(atom3);

      var list2 = list.Clone(false);
      CheckClone(list, list2);
    }

    [Fact]
    public void TestListCopyWithFusedItems() {
      var list = LaTeXParserTest.ParseLaTeX("12+x");

      var finalized = list.Clone(true);
      var fusedCount = finalized.Sum(atom => atom.FusedAtoms?.Count ?? 0);
      Assert.Equal(2, fusedCount);

      var copy = finalized.Clone(true);
      var fusedCopyCount = copy.Sum(atom => atom.FusedAtoms?.Count ?? 0);
      Assert.Equal(2, fusedCopyCount);
    }

    [Fact]
    public void TestListFinalizedCopy() {
      var input = @"-52x^{13+y}_{15-} + (-12.3 *)\frac{-12}{15.2}\int^\sqrt[!\ ]{=(}_0 \theta%:)" + "\n" + @",";
      var list = LaTeXParserTest.ParseLaTeX(input);
      Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => CheckListContents(list));
      Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => CheckListContents(list.Clone(false)));
      Assert.All(list, a => Assert.Equal(Range.Zero, a.IndexRange));
      var finalized = list.Clone(true);
      CheckListContents(finalized);
      CheckListContents(finalized.Clone(false));
      CheckListContents(finalized.Clone(true));

      static Action<MathAtom>
        CheckAtomNucleusAndRange<T>(string nucleus, int rangeIndex, int rangeLength) => a => {
          Assert.IsType<T>(a);
          Assert.Equal(nucleus, a.Nucleus);
          Assert.Equal(new Range(rangeIndex, rangeLength), a.IndexRange);
        };
      static void CheckListContents(MathList? list) {
        Assert.NotNull(list);
        Assert.Collection(list.Atoms,
          CheckAtomNucleusAndRange<UnaryOperator>("\u2212", 0, 1),
          CheckAtomNucleusAndRange<Number>("52", 1, 2),
          CheckAtomNucleusAndRange<Variable>("x", 3, 1),
          CheckAtomNucleusAndRange<BinaryOperator>("+", 4, 1),
          CheckAtomNucleusAndRange<Open>("(", 5, 1),
          CheckAtomNucleusAndRange<UnaryOperator>("\u2212", 6, 1),
          CheckAtomNucleusAndRange<Number>("12.3", 7, 4),
          CheckAtomNucleusAndRange<UnaryOperator>("*", 11, 1),
          CheckAtomNucleusAndRange<Close>(")", 12, 1),
          CheckAtomNucleusAndRange<Fraction>("", 13, 1),
          CheckAtomNucleusAndRange<LargeOperator>("∫", 14, 1),
          CheckAtomNucleusAndRange<Variable>("θ", 15, 1),
          // Comments are not given ranges as they won't affect typesetting
          CheckAtomNucleusAndRange<Comment>(":)", Range.UndefinedInt, Range.UndefinedInt),
          CheckAtomNucleusAndRange<Punctuation>(",", 16, 1)
        );
        Assert.Collection(list.Atoms[2].Superscript,
          CheckAtomNucleusAndRange<Number>("13", 0, 2),
          CheckAtomNucleusAndRange<BinaryOperator>("+", 2, 1),
          CheckAtomNucleusAndRange<Variable>("y", 3, 1)
        );
        Assert.Collection(list.Atoms[2].Subscript,
          CheckAtomNucleusAndRange<Number>("15", 0, 2),
          CheckAtomNucleusAndRange<BinaryOperator>("\u2212", 2, 1)
        );
        var fraction = Assert.IsType<Fraction>(list.Atoms[9]);
        Assert.Collection(fraction.Numerator,
          CheckAtomNucleusAndRange<UnaryOperator>("\u2212", 0, 1),
          CheckAtomNucleusAndRange<Number>("12", 1, 2)
        );
        Assert.Collection(fraction.Denominator,
          CheckAtomNucleusAndRange<Number>("15.2", 0, 4)
        );
        var integral = Assert.IsType<LargeOperator>(list.Atoms[10]);
        Assert.Collection(integral.Subscript,
          CheckAtomNucleusAndRange<Number>("0", 0, 1)
        );
        var radical = Assert.IsType<Radical>(Assert.Single(integral.Superscript));
        Assert.Collection(radical.Degree,
          CheckAtomNucleusAndRange<Punctuation>("!", 0, 1),
          CheckAtomNucleusAndRange<Ordinary>(" ", 1, 1)
        );
        Assert.Collection(radical.Radicand,
          CheckAtomNucleusAndRange<Relation>("=", 0, 1),
          CheckAtomNucleusAndRange<Open>("(", 1, 1)
        );
      }
    }
  }
}
