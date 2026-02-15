using CSharpMath.SkiaSharp;
using SkiaSharp;

static void Draw(SKCanvas c) {
  // declare constants
  const int count = 10; // number of digits in outer circle
  const float r = 100f; // outer circle radius
  const float f = 40f; // font size in points
  const float thicknessAdjust = 2 * f / 3; // thickness adjust of the two circles
  const float θ = 360f / count; // angle to rotate when drawing each digit
  var painter = new MathPainter { FontSize = f }; // try: GlyphBoxColor = (SKColors.Red, SKColors.Red)
  var cx = c.DeviceClipBounds.Width / 2;
  var cy = c.DeviceClipBounds.Height / 2;

  // draw outer circle
  using (var black = new SKPaint { Color = SKColors.Black, IsAntialias = true })
    c.DrawCircle(cx, cy, r + thicknessAdjust, black);
  painter.TextColor = SKColors.White;
  for (int i = 0; i < count; i++) {
    painter.LaTeX = i.ToString();
    var m = painter.Measure();
    painter.Draw(c, cx - m.Width / 2, cy + m.Height / 2 - r);
    c.RotateDegrees(θ, cx, cy);
  }

  // draw inner circle
  using (var white = new SKPaint { Color = SKColors.White, IsAntialias = true })
    c.DrawCircle(cx, cy, r - thicknessAdjust, white);
  painter.TextColor = SKColors.Black;
  painter.LaTeX = @"\ \raisebox{25mu}{\begin{gather}C\#\\Math\end{gather}}\ ";
  painter.Draw(c);
}

// PNG
var pngPath = Path.Combine("..", "..", "..", "..", "Icon.png");
using (var surface = SKSurface.Create(new SKImageInfo(258, 258))) {
  Draw(surface.Canvas);
  using var snapshot = surface.Snapshot();
  using var tempFile = new FileStream(pngPath, FileMode.OpenOrCreate);
  snapshot.Encode(SKEncodedImageFormat.Png, 100).SaveTo(tempFile);
}
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
  FileName = pngPath,
  UseShellExecute = true
});

// SVG
var svgPath = Path.Combine("..", "..", "..", "..", "CSharpMath.Uno.Example", "Assets", "Icons", "icon.svg");
using (var stream = File.OpenWrite(svgPath))
using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, 258, 258), stream))
  Draw(canvas);
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
  FileName = svgPath,
  UseShellExecute = true
});
File.Copy(svgPath, Path.Combine("..", "..", "..", "..", "CSharpMath.Uno.Example", "Assets", "Splash", "splash_screen.svg"), true);