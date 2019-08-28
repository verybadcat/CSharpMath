using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  /// <summary>Entry point for all ways of creating MathList objects. Much of the
  /// actual "meat" is in MathListBuilder.</summary> 
  public static class MathLists {
    public static MathList WithAtoms(List<IMathAtom> atoms) {
      var r = new MathList();
      foreach (var atom in atoms) {
        r.Add(atom);
      }
      return r;
    }

    public static MathList WithAtoms(params IMathAtom[] atoms) {
      var r = new MathList();
      foreach (var atom in atoms) {
        r.Add(atom);
      }
      return r;
    }

    public static (IMathList builtList, string error) BuildResultFromString(string str) {
      var builder = new MathListBuilder(str);
      var list = builder.Build();
      var error = builder.Error;
      return (list, error);
    }

    public static IMathList FromString(string str) {
      var builder = new MathListBuilder(str);
      return builder.Build();
    }
  }
}
