using System;
using System.Drawing;
using CSharpMath.Rendering;
using SkiaSharp;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using C = CSharpMath.Structures.Color;
using static SkiaSharp.Views.Forms.Extensions;
using CSharpMath.SkiaSharp;

namespace CSharpMath.Forms {
  public class EditableMathView : BaseView<EditableMathPainter<EditableMathView.Button>, MathSource, EditableMathView.PainterSupplier>, IPainter<MathSource, Color> {
    public class Button : Editor.IButton {
      public Xamarin.Forms.Button Content { get; set; }
      public Action<>
      public string Text { get => Content.Text; set => Content.Text = value; }
      public bool Enabled { get => Content.IsEnabled; set => Content.IsEnabled = value; }
      public bool Selected { get => throw null; set => throw null; }
    }
    public struct PainterSupplier : IPainterSupplier<EditableMathPainter<Button>> {
      public EditableMathPainter<Button> Default => new EditableMathPainter<Button>(null);
    }
    #region BindableProperties
    static EditableMathView() {
      var painter = default(PainterSupplier).Default;
      var thisType = typeof(MathView);
      SkiaSharp.EditableMathPainter<Button> p(BindableObject b) => ((EditableMathView)b).Painter;
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType, painter.GlyphBoxColor,
        propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
    }
    public static readonly BindableProperty StrokeCapProperty;
    public static readonly BindableProperty GlyphBoxColorProperty;
    #endregion

    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public Interfaces.IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
    public new RectangleF? Measure => Painter.Measure;
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => Painter.Measure is RectangleF r ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height)) : base.OnMeasure(widthConstraint, heightConstraint);

    public Layout Default {
      get {
        var container = new AbsoluteLayout();
        Editor.MathKeyboardView<Button>.Default(
          (RectangleF frame, string text, float textPtSize, C title, C titleShadow, C highlightedTitle, C? disabled) => {
            var button = new Button {
              Content = new Xamarin.Forms.Button {
                Text = text,
                FontSize = textPtSize,
                TextColor = title.ToNative().ToFormsColor()
              }
            };
            container.Children.Add(button.Content, new Xamarin.Forms.Rectangle(frame.X, frame.Y, frame.Width, frame.Height));
            return button;
          }, (button, handler) => button.Content.Clicked += handler);
        return container;
      }
    }
  }
}