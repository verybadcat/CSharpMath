using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System.Text;

namespace CSharpMath.Atoms {
  public class Accent : MathAtom, IAccent {
    public IMathList InnerList { get; set; }

    public Accent(string value): base(MathAtomType.Accent, value) {

    }

    public override string StringValue =>
      new StringBuilder(@"\accent")
      .AppendInBraces(Nucleus, NullHandling.LiteralNull)
      .AppendInBraces(InnerList, NullHandling.LiteralNull)
      .ToString();

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper) {
      return visitor.Visit(this, helper);
    }

    public Accent(Accent cloneMe, bool finalize): base(cloneMe, finalize) {
      InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
    }

    public bool EqualsAccent(Accent other) =>
      this.EqualsAtom(other) && InnerList.NullCheckingEquals(other.InnerList);

    public override bool Equals(object obj)
      => EqualsAccent(obj as Accent);

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode()
      + 71 * InnerList?.GetHashCode() ?? 1;
      }
    }
  }
}
