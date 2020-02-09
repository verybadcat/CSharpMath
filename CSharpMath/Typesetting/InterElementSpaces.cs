namespace CSharpMath.TypesetterInternal {
  using static InterElementSpaceType;
  using Enumerations;
  internal enum InterElementSpaceType {
    Invalid = -1,
    None = 0,
    Thin,
    ///<summary>Thin if not in script mode, else none</summary> 
    NsThin,
    ///<summary>Medium if not in script mode, else none</summary> 
    NsMedium,
    ///<summary>Thick if not in script mode, else none</summary> 
    NsThick
  }
  internal static class InterElementSpaces {
    public static InterElementSpaceType[][] Spaces = {
    // RIGHT ordinary  operator  binary    relation open      close    punct    fraction       LEFT
      new[]{ None,     Thin,     NsMedium, NsThick, None,     None,    None,    NsThin   }, // ordinary
      new[]{ Thin,     Thin,     Invalid,  NsThick, None,     None,    None,    NsThin   }, // operator
      new[]{ NsMedium, NsMedium, Invalid,  Invalid, NsMedium, Invalid, Invalid, NsMedium }, // binary
      new[]{ NsThick,  NsThick,  Invalid,  None,    NsThick,  None,    None,    NsThick  }, // relation
      new[]{ None,     None,     Invalid,  None,    None,     None,    None,    None     }, // open
      new[]{ None,     Thin,     NsMedium, NsThick, None,     None,    None,    NsThin   }, // close
      new[]{ NsThin,   NsThin,   Invalid,  NsThin,  NsThin,   NsThin,  NsThin,  NsThin   }, // punct
      new[]{ NsThin,   Thin,     NsMedium, NsThick, NsThin,   None,    NsThin,  NsThin   }, // fraction
      new[]{ NsMedium, NsThin,   NsMedium, NsThick, None,     None,    None,    NsThin   }, // radical
    };

    public static int SpacingInMu(this InterElementSpaceType type, LineStyle _style) =>
      type switch
      {
        Invalid => -1,
        None => 0,
        Thin => 3,
        NsThin => _style < LineStyle.Script ? 3 : 0,
        NsMedium => _style < LineStyle.Script ? 4 : 0,
        NsThick => _style < LineStyle.Script ? 5 : 0,
        _ => throw new System.ComponentModel.InvalidEnumArgumentException
                (nameof(_style), (int)_style, typeof(LineStyle))
      };
  }
}