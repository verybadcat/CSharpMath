using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms.Extension {
  internal interface I_ExtensionAtom : IMathAtom { }
  public abstract class _ExtensionAtom<T> : MathAtom, I_ExtensionAtom where T : _ExtensionAtom<T>, new() {
    internal _ExtensionAtom(MathAtomType type) : base(type, string.Empty) { }
    //protected _ExtensionAtom(_ExtensionAtom<T> cloneMe, bool finalize) : base(cloneMe, finalize) { }

    protected abstract void CopyPropertiesFrom(T oldAtom);
    public T Clone(bool finalize) {
      var newAtom = new T();
      newAtom.CopyPropertiesFrom((T)this);
      return newAtom;
    }

    public sealed override TAtom Accept<TAtom, THelper>(IMathAtomVisitor<TAtom, THelper> visitor, THelper helper) {
      return visitor is AtomCloner && helper is bool finalize ?
        (TAtom)(object)Clone(finalize) :
        base.Accept(visitor, helper);
    }
  }
}
