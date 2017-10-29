using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public class MathListTest {
    internal static void CheckClone(IMathAtom original, IMathAtom clone) {
      Assert.Equal(original, clone);
      Assert.False(ReferenceEquals(original, clone));
    }

    internal static void CheckClone(IMathList original, IMathList clone) {
      Assert.Equal(original, clone);
      Assert.False(ReferenceEquals(original, clone));
    }
    [Fact]
    public void TestCopy() {
      var list = new MathList();
      list.AddAtom(MathAtoms.Placeholder);
      list.AddAtom(MathAtoms.Times);
      list.AddAtom(MathAtoms.Divide);

      var list2 = new MathList();
      list2.AddAtom(MathAtoms.Divide);
      list2.AddAtom(MathAtoms.Times);

      var open = MathAtoms.Create(MathAtomType.Open, "(");
      open.Subscript = list;
      open.Superscript = list2;

      var clone = AtomCloner.Clone(open, false);
      CheckClone(open, clone);
      CheckClone(open.Superscript, clone.Superscript);
      CheckClone(open.Subscript, clone.Subscript);
    }

    [Fact]
    public void TestCopyFraction() {
      var atom = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = new MathList();
      list.AddAtom(atom);
      list.AddAtom(atom2);
      list.AddAtom(atom3);

      var list2 = new MathList();
      list2.AddAtom(atom3);
      list2.AddAtom(atom2);

      var frac = new Fraction(false) {
        Numerator = list,
        Denominator = list2,
        LeftDelimiter = "a",
        RightDelimiter = "b"
      };
    
      Assert.Equal(MathAtomType.Fraction, frac.AtomType);


      var copy = new Fraction(frac, false);
      CheckClone(copy, frac);
      CheckClone(copy.Numerator, frac.Numerator);
      Assert.False(copy.HasRule);
      Assert.Equal("a", copy.LeftDelimiter);
      Assert.Equal("b", copy.RightDelimiter);
    }

    [Fact]
    public void TestCopyRadical() {
      var atom = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = MathLists.WithAtoms(atom, atom2, atom3);
      var list2 = MathLists.WithAtoms(atom3, atom2);

      var radical = new Radical {
        Radicand = list,
        Degree = list2
      };

      var copy = new Radical(radical, false);

      CheckClone(copy, radical);
      CheckClone(copy.Radicand, radical.Radicand);
      CheckClone(copy.Degree, radical.Degree);
    }

    [Fact]
    public void TestCopyLargeOperator() {
      var large = new LargeOperator("lim", true);
      Assert.Equal(MathAtomType.LargeOperator, large.AtomType);
      Assert.True(large.Limits);

      var copy = new LargeOperator(large, false);
      CheckClone(copy, large);
      Assert.Equal(copy.Limits, large.Limits);
    }

    [Fact]
    public void TestCopyInner() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = MathLists.WithAtoms(atom1, atom2, atom3);

      var inner = new Inner {
        InnerList = list,
        LeftBoundary = MathAtoms.Create(MathAtomType.Boundary, "("),
        RightBoundary = MathAtoms.Create(MathAtomType.Boundary, ")")
      };

      Assert.Equal(MathAtomType.Inner, inner.AtomType);

      var copy = new Inner(inner, false);

      CheckClone(inner, copy);
      CheckClone(inner.InnerList, copy.InnerList);
      CheckClone(inner.LeftBoundary, copy.LeftBoundary);
      CheckClone(inner.RightBoundary, copy.RightBoundary);
    }
  }
}
