
using CSharpMath.Enumerations;
using System;

namespace CSharpMath.TypesetterInternal {
  internal enum InterElementSpaceType {
    Invalid = -1,
    None = 0,
    Thin,
    ///<summary>Thin but not in script mode</summary> 
    NsThin, 
    ///<summary>Medium and not in script mode</summary> 
    NsMedium,
    ///<summary>Thick and not in script mode</summary> 
    NsThick
  }

  internal static class InterElementSpaceTypeExtensions {
    public static int SpacingInMu(this InterElementSpaceType type, LineStyle _style) {
      bool belowScript = _style < LineStyle.Script;
      switch (type) {
        case InterElementSpaceType.Invalid:
          return -1;
        case InterElementSpaceType.None:
          return 0;
        case InterElementSpaceType.Thin:
          return 3;
        case InterElementSpaceType.NsThin:
          return belowScript ? 3 : 0;
        case InterElementSpaceType.NsMedium:
          return belowScript ? 4 : 0;
        case InterElementSpaceType.NsThick:
          return belowScript ? 5 : 0;
        default:
          throw new InvalidOperationException("Unknown space type");
      }
    }
  }
}
