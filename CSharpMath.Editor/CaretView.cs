using System;

namespace CSharpMath.Editor {
  using Structures;
  public readonly struct CaretHandle {
    public CaretHandle(float fontSize) {
      var scale = fontSize / CaretFontSize;
      Width = CaretHandleWidth * scale;
      Height = CaretHandleHeight * scale;
    }

    public static readonly TimeSpan InitialBlinkDelay = TimeSpan.FromSeconds(0.7);
    public static readonly TimeSpan BlinkRate = TimeSpan.FromSeconds(0.5);
    // The settings below make sense for the given font size. They are scaled appropriately when the fontsize changes.
    public const float CaretFontSize = 30;
    public const int CaretWidth = 3;
    public const int CaretAscent = 25;  // How much should te caret be above the baseline
    public const int CaretDescent = 7;  // How much should the caret be below the baseline
    public const int CaretHandleWidth = 15;
    public const int CaretHandleDescent = 8;
    public const int CaretHandleHeight = 20;
    public const int CaretHandleHitAreaSize = 44;

    public const int CaretHeight = CaretAscent + CaretDescent;

    public float Width { get; }
    public float Height { get; }
  }
}