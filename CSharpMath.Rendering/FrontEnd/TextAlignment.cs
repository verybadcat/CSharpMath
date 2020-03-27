namespace CSharpMath.Rendering.FrontEnd {
  [System.Flags]
  public enum TextAlignment : byte {
    //              LRTB
    TopLeft     = 0b1010,
    Top         = 0b0010,
    TopRight    = 0b0110,
    Left        = 0b1000,
    Center      = 0b0000,
    Right       = 0b0100,
    BottomLeft  = 0b1001,
    Bottom      = 0b0001,
    BottomRight = 0b0101
  }
}