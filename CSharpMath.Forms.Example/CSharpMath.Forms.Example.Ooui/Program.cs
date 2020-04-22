using Ooui;
using Xamarin.Forms;

namespace CSharpMath.Forms.Example.Ooui {
  using CSharpMath.Rendering.FrontEnd;
  using CSharpMath.Structures;
  class Program {
    static void Main(string[] args) {
      Xamarin.Forms.Forms.Init();
      // Throws unfortuately, needs SkiaSharp renderer
      var canvas = new Canvas { Style = { BackgroundColor = new global::Ooui.Color(128, 128, 128) } };
      new MathPainter { LaTeX = "1+2" }.Draw(canvas, TextAlignment.TopLeft);
      UI.Publish("/", new Div(new global::Ooui.Button("Hi!"), canvas));
      System.Console.WriteLine("Server ready! Press Enter to terminate...");
      System.Console.ReadLine();
    }
    public sealed class OouiPath : Path {
      public OouiPath(CanvasRenderingContext2D context) {
        this.context = context;
        context.BeginPath();
      }
      public override Color? Foreground { get; set; }
      private readonly CanvasRenderingContext2D context;
      public override void MoveTo(float x0, float y0) {
        context.ClosePath();
        context.BeginPath();
        context.MoveTo(x0, y0);
      }
      public override void LineTo(float x1, float y1) => context.LineTo(x1, y1);
      public override void Curve3(float x1, float y1, float x2, float y2) =>
        context.QuadraticCurveTo(x1, y1, x2, y2);
      public override void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) =>
        context.BezierCurveTo(x1, y1, x2, y2, x3, y3);
      public override void CloseContour() => context.ClosePath();
      public override void Dispose() {
        context.Fill();
      }
    }
    public sealed class OouiCanvas : ICanvas {
      readonly Canvas canvas;
      readonly CanvasRenderingContext2D context;
      public OouiCanvas(Canvas canvas) {
        this.canvas = canvas;
        context = canvas.GetContext2D();
      }
      public float Width => canvas.Width;
      public float Height => canvas.Height;
      public Color DefaultColor { get; set; }
      public Color? CurrentColor { get; set; }
      public PaintStyle CurrentStyle { get;set; }

      public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
        context.BeginPath();
        context.MoveTo(x1, y1);
        context.LineTo(x2, y2);
        context.ClosePath();
      }
      public void FillRect(float left, float top, float width, float height) =>
        context.FillRect(left, top, width, height);
      public void Restore() => context.Restore();
      public void Save() => context.Save();
      public void Scale(float sx, float sy) => context.Scale(sx, sy);
      public Path StartNewPath() => new OouiPath(context);
      public void StrokeRect(float left, float top, float width, float height) =>
        context.StrokeRect(left, top, width, height);
      public void Translate(float dx, float dy) => context.Translate(dx, dy);
    }
    class MathPainter : MathPainter<Canvas, global::Ooui.Color> {
      public override global::Ooui.Color UnwrapColor(Color color) =>
        new global::Ooui.Color(color.R, color.G, color.B, color.A);
      public override ICanvas WrapCanvas(Canvas canvas) => new OouiCanvas(canvas);
      public override Color WrapColor(global::Ooui.Color color) =>
        new Color(color.R, color.G, color.B, color.A);
    }
  }
}