using System;
using System.Drawing;
using CSharpMath.Rendering;
using SkiaSharp;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using static SkiaSharp.Views.Forms.Extensions;

namespace CSharpMath.Forms {
  public class Button : Editor.IButton {
    public Xamarin.Forms.Button Content { get; set; }
    public string Text { get => Content.Text; set => Content.Text = value; }
    public bool Enabled { get => Content.IsEnabled; set => Content.IsEnabled = value; }
    public bool Selected { get => throw null; set => throw null; }
  }
  public class Label : Editor.ITextView{
    public Xamarin.Forms.Label Content { get; set; }

    public void Insert(int position, char value) {
      var s = Content.Text;

      var newString = new char[s.Length + 1];
      s.CopyTo(0, newString, 0, s.Length);
      for (int i = newString.Length - 1; i >= 0 && i > position; i--)
        newString[i] = newString[i - 1];
      newString[position] = value;

      Content.Text = new string(newString);
    }

    public void Insert(int position, string value) => Content.Text = Content.Text.Insert(position, value);

    public void Remove(int position) => Content.Text = Content.Text.Remove(position, 1);
  }
  public class EditableMathView : BaseView<SkiaSharp.EditableMathPainter<Button, Label>, MathSource>, IPainter<MathSource, Color> {
    #region BindableProperties
  static EditableMathView() {
      var painter = new SkiaSharp.EditableMathPainter<Button, Label>();
      var thisType = typeof(MathView);
      SkiaSharp.EditableMathPainter<Button, Label> p(BindableObject b) => ((EditableMathView)b).Painter;
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType,
        defaultValue: painter.GlyphBoxColor,
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

  }
}
