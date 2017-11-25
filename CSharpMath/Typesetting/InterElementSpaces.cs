using CSharpMath.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.TypesetterInternal {
  internal static class InterElementSpaces {
    public static InterElementSpaceType[][] Spaces;
    static InterElementSpaces() {
      Spaces = new InterElementSpaceType[][] {
           new InterElementSpaceType[] {InterElementSpaceType.None,     InterElementSpaceType.Thin,     InterElementSpaceType.NsMedium, InterElementSpaceType.NsThick, InterElementSpaceType.None,     InterElementSpaceType.None,    InterElementSpaceType.None,    InterElementSpaceType.NsThin},    // ordinary
           new InterElementSpaceType[] {InterElementSpaceType.Thin,     InterElementSpaceType.Thin,     InterElementSpaceType.Invalid,  InterElementSpaceType.NsThick, InterElementSpaceType.None,     InterElementSpaceType.None,    InterElementSpaceType.None,    InterElementSpaceType.NsThin},    // operator
           new InterElementSpaceType[] {InterElementSpaceType.NsMedium, InterElementSpaceType.NsMedium, InterElementSpaceType.Invalid,  InterElementSpaceType.Invalid, InterElementSpaceType.NsMedium, InterElementSpaceType.Invalid, InterElementSpaceType.Invalid, InterElementSpaceType.NsMedium},  // binary
           new InterElementSpaceType[] {InterElementSpaceType.NsThick,  InterElementSpaceType.NsThick,  InterElementSpaceType.Invalid,  InterElementSpaceType.None,    InterElementSpaceType.NsThick,  InterElementSpaceType.None,    InterElementSpaceType.None,    InterElementSpaceType.NsThick},   // relation
           new InterElementSpaceType[] {InterElementSpaceType.None,     InterElementSpaceType.None,     InterElementSpaceType.Invalid,  InterElementSpaceType.None,    InterElementSpaceType.None,     InterElementSpaceType.None,    InterElementSpaceType.None,    InterElementSpaceType.None},      // open
           new InterElementSpaceType[] {InterElementSpaceType.None,     InterElementSpaceType.Thin,     InterElementSpaceType.NsMedium, InterElementSpaceType.NsThick, InterElementSpaceType.None,     InterElementSpaceType.None,    InterElementSpaceType.None,    InterElementSpaceType.NsThin},    // close
           new InterElementSpaceType[] {InterElementSpaceType.NsThin,   InterElementSpaceType.NsThin,   InterElementSpaceType.Invalid,  InterElementSpaceType.NsThin,  InterElementSpaceType.NsThin,   InterElementSpaceType.NsThin,  InterElementSpaceType.NsThin,  InterElementSpaceType.NsThin},    // punct
           new InterElementSpaceType[] {InterElementSpaceType.NsThin,   InterElementSpaceType.Thin,     InterElementSpaceType.NsMedium, InterElementSpaceType.NsThick, InterElementSpaceType.NsThin,   InterElementSpaceType.None,    InterElementSpaceType.NsThin,  InterElementSpaceType.NsThin},    // fraction
           new InterElementSpaceType[] {InterElementSpaceType.NsMedium, InterElementSpaceType.NsThin,   InterElementSpaceType.NsMedium, InterElementSpaceType.NsThick, InterElementSpaceType.None,     InterElementSpaceType.None,    InterElementSpaceType.None,    InterElementSpaceType.NsThin}   // radical
      };

    }
  }
}
