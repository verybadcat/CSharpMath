using CSharpMath.SkiaSharp;
using SkiaSharp;

// set up canvas
using var surface = SKSurface.Create(new SKImageInfo(258, 258));
var c = surface.Canvas;

// declare constants
const int count = 10; //number of digits in outer circle
const float r = 100f; //outer circle radius
const float f = 40f; //font size in points
const float thicknessAdjust = 2 * f / 3; //thickness adjust of the two circles
const float θ = 360f / count; //angle to rotate when drawing each digit
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

// save
using var snapshot = surface.Snapshot();
var tempFileName = Path.Combine(Path.GetTempPath(), "CSharpMathIcon.png");
using (var tempFile = new FileStream(tempFileName, FileMode.OpenOrCreate)) {
  snapshot.Encode(SKEncodedImageFormat.Png, 100).SaveTo(tempFile);
}

// open the image file with default image viewer
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
  FileName = tempFileName,
  UseShellExecute = true
});