using CSharpMath.Atoms;
using CSharpMath.Enumerations;
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
    public void TestSubscript() {
      var str = "-52x^{13+y}_{15-} + (-12.3 *)\\frac{-12}{15.2}";
      var list = MathLists.FromString(str);
      var finalized = new MathList(list, true);
      MathListValidator.CheckListContents(finalized);
      var reFinalized = new MathList(finalized, true);
      MathListValidator.CheckListContents(reFinalized);
    }


  }
}
