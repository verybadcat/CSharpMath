using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public class MathListTest {
    [Fact]
    public void TestCopy() {
      var list = new MathList();
      list.AddAtom(MathAtoms.Placeholder);
      list.AddAtom(MathAtoms.Times);
      list.AddAtom(MathAtoms.Divide);

      var list2 = new MathList();
      list2.AddAtom(MathAtoms.Divide);
      list2.AddAtom(MathAtoms.Times);
      //TODO: finish
    }
  }
}
