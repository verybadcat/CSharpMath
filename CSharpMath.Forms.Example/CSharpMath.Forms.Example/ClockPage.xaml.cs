using System;
using static System.Math;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using CSharpMath.SkiaSharp;

namespace CSharpMath.Forms.Example {
  /// <summary>
  /// Credits to https://github.com/sadqiang
  /// at https://github.com/verybadcat/CSharpMath/issues/27
  /// </summary>
  public partial class ClockPage : ContentPage {
    readonly SKPaint blackFillPaint = new SKPaint {
      Style = SKPaintStyle.Fill,
      Color = SKColors.Black
    };
    readonly SKPaint whiteFillPaint = new SKPaint {
      Style = SKPaintStyle.Fill,
      Color = SKColors.White
    };
    readonly SKPaint whiteStrokePaint = new SKPaint {
      Style = SKPaintStyle.Stroke,
      Color = SKColors.White,
      StrokeCap = SKStrokeCap.Round,
      IsAntialias = true
    };
    readonly SKPaint redStrokePaint = new SKPaint {
      Style = SKPaintStyle.Stroke,
      Color = SKColors.Red,
      StrokeCap = SKStrokeCap.Round,
      IsAntialias = true
    };
    readonly string[] labels = {
      // Four 4s make 1 to 12 using different operations
      @"$\frac{44+4}{4}$",
      @"$\frac{44}{44}$",
      @"$\frac{4}{4}+\frac{4}{4}$",
      @"$\frac{4+4+4}{4}$",
      @"$4+\frac{4-4}{4}$",
      @"$4+4^{4-4}$",
      @"$4+\frac{4+4}{4}$",
      @"$\frac{44}{4}-4$",
      @"$\sqrt{4}^{4-\frac{4}{4}}$",
      @"$\:\:(4-\frac{4}{4})^{\sqrt{4}}$",
      @"$\frac{44-4}{4}$",
      @"$\frac{4!}{\sqrt{4}}-\frac{4}{4}$"
    };
    public ClockPage() {
      InitializeComponent();
      Device.StartTimer(TimeSpan.FromMilliseconds(20), () => {
        canvasView.InvalidateSurface();
        return true;
      });
    }
    private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      var canvas = e.Surface.Canvas;
      canvas.Clear(SKColors.CornflowerBlue);
      canvas.Translate(e.Info.Width / 2, e.Info.Height / 2);
      canvas.Scale(e.Info.Width / 210f);
      canvas.DrawCircle(0, 0, 100, blackFillPaint);
      var painter = new TextPainter { FontSize = 8, TextColor = SKColors.White };
      for (int i = 0; i < 60; i++) {
        // Dots
        canvas.Save();
        canvas.RotateDegrees(6 * i);
        canvas.DrawCircle(0, -90, i % 5 == 0 ? 4 : 2, whiteFillPaint);
        canvas.Restore();
        // Maths
        if (i % 5 == 0) {
          painter.LaTeX = labels[i / 5];
          var measure = painter.Measure(e.Info.Width).Value;
          var θ = (90 - 6 * i) / 180f * PI;
          var sinθ = (float)Sin(θ);
          var cosθ = (float)Cos(θ);
          painter.Draw(canvas,
            new System.Drawing.PointF(75 * cosθ - measure.Width / 2,
              -75 * sinθ - measure.Height / 2),
            float.PositiveInfinity);
        }
      }
      var dateTime = DateTime.Now;
      // H
      canvas.Save();
      canvas.RotateDegrees(30 * dateTime.Hour + dateTime.Minute / 2f);
      whiteStrokePaint.StrokeWidth = 12;
      canvas.DrawLine(0, 0, 0, -50, whiteStrokePaint);
      canvas.Restore();
      // M
      canvas.Save();
      canvas.RotateDegrees(6 * dateTime.Minute + dateTime.Second / 10f);
      whiteStrokePaint.StrokeWidth = 6;
      canvas.DrawLine(0, 0, 0, -65, whiteStrokePaint);
      canvas.Restore();
      // S
      canvas.Save();
      canvas.RotateDegrees(6f * (dateTime.Second + dateTime.Millisecond / 1000f));
      redStrokePaint.StrokeWidth = 2;
      canvas.DrawLine(0, 0, 0, -75, redStrokePaint);
      canvas.Restore();
    }
  }
}