using static System.Math;
using Stream = System.IO.Stream;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using Xamarin.Forms;

namespace CSharpMath.Forms.Example {
  public class SvgButton : Button {
    public static readonly BindableProperty SvgProperty =
      BindableProperty.Create(nameof(Svg), typeof(Stream), typeof(SvgButton));
    public Stream Svg {
      get => (Stream)GetValue(SvgProperty);
      set => SetValue(SvgProperty, value);
    }
    // Don't let it be GC'd
    protected SKPicture _scaledPicture;
    protected override void OnSizeAllocated(double width, double height) {
      if (!Svg.CanRead || !Svg.CanSeek)
        throw new System.NotSupportedException(
          "The stream provided in the Svg property cannot be read and seeked." +
          "Copy it to a MemoryStream instead."
        );
      Svg.Position = 0;
      var svg = new SKSvg();
      svg.Load(Svg);
      // Forcefully set aspect to AspectFit
      var scale =
        Min((float)width / svg.CanvasSize.Width, (float)height / svg.CanvasSize.Height);
      using (var recorder = new SKPictureRecorder())
      using (var canvas = recorder.BeginRecording(svg.ViewBox)) {
        canvas.Scale(scale);
        canvas.DrawPicture(svg.Picture);
        ImageSource = new SKPictureImageSource {
          Picture = _scaledPicture = recorder.EndRecording(),
          Dimensions =
            new SKSize(svg.CanvasSize.Width * scale, svg.CanvasSize.Height * scale).ToSizeI()
        };
      }
      base.OnSizeAllocated(width, height);
    }
  }
}