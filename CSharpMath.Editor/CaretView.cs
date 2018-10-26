using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Editor {
  using Structures;
  public struct CaretHandle {
    public static readonly TimeSpan InitialBlinkDelay = TimeSpan.FromSeconds(0.7);
    public static readonly TimeSpan BlinkRate = TimeSpan.FromSeconds(0.5);
    // The settings below make sense for the given font size. They are scaled appropriately when the fontsize changes.
    public const float CaretFontSize = 30;
    public const int CaretAscent = 25;  // How much should te caret be above the baseline
    public const int CaretWidth = 3;
    public const int CaretDescent = 7;  // How much should the caret be below the baseline
    public const int CaretHandleWidth = 15;
    public const int CaretHandleDescent = 8;
    public const int CaretHandleHeight = 20;
    public const int CaretHandleHitAreaSize = 44;

    public const int CaretHeight = CaretAscent + CaretDescent;

    public Color Color { get; set; }
    public RectangleF Frame { get; set; }
    public SizeF Bounds { get => Frame.Size; set => Frame = new RectangleF(Frame.Location, value); }
    public PointF Position { get => Frame.Location; set => Frame = new RectangleF(value, Frame.Size); }

    public Color ActualColor => new Color(Color.R, Color.G, Color.B, (byte)(Color.A * 3 / 5));
    public PointF InitialPoint => new PointF(Bounds.Width / 2, 0);
    public PointF NextPoint1 => new PointF(Bounds.Width, Bounds.Height / 4);
    public PointF NextPoint2 => new PointF(Bounds.Width, Bounds.Height);
    public PointF NextPoint3 => new PointF(0, Bounds.Height);
    public PointF FinalPoint => new PointF(0, Bounds.Height / 4);

    public bool PointInside(PointF point) =>
      // Create a hit area around the center.
      new RectangleF((Bounds.Width - CaretHandleHitAreaSize) / 2, (Bounds.Height - CaretHandleHitAreaSize) / 2, CaretHandleHitAreaSize, CaretHandleHitAreaSize)
        .Contains(point);
  }
  public class CaretView<TFont, TGlyph> where TFont : Display.IFont<TGlyph> {
    public CaretView(float fontSize) {
      scale = fontSize / CaretHandle.CaretFontSize;
      handle = new CaretHandle {
        Frame = new RectangleF(
          -(CaretHandle.CaretHandleWidth - CaretHandle.CaretWidth) * scale / 2,
          (CaretHandle.CaretHeight + CaretHandle.CaretHandleDescent) * scale,
          CaretHandle.CaretHandleWidth * scale,
          CaretHandle.CaretHandleHeight * scale
        )
      };
    }
    public readonly CaretHandle handle;
    public readonly float scale;
  }
}
