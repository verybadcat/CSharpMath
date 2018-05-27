using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.SkiaSharp {
  [Flags]
  public enum SkiaTextAlignment : byte {
    //              LRTB
    TopLeft     = 0b1010,
    Top         = 0b0010,
    TopRight    = 0b0110,
    Left        = 0b1000,
    Centre      = 0b0000,
    Right       = 0b0100,
    BottomLeft  = 0b1001,
    Bottom      = 0b0001,
    BottomRight = 0b0101
  }
}