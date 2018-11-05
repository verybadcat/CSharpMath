using System;
using System.Drawing;
using CSharpMath.Rendering;
using SkiaSharp;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using C = CSharpMath.Structures.Color;
using static SkiaSharp.Views.Forms.Extensions;
using CSharpMath.SkiaSharp;
using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms {
  public class EditableMathView : BaseView<EditableMathPainter<EditableMathView.Button, EditableMathView.ButtonLayout>, EditableMathSource, EditableMathView.PainterSupplier>, IPainter<EditableMathSource, Color> {
    public class Button : Editor.IButton {
      public Xamarin.Forms.Button Content { get; set; }
      public string Text { get => Content.Text; set => Content.Text = value; }
      public bool Enabled { get => Content.IsEnabled; set => Content.IsEnabled = value; }
      public bool Selected { get => false; set => new int(); }
      public bool Visible { get => Content.IsVisible; set => Content.IsVisible = value; }
      public RectangleF Bounds { get; set; }
    }
    public class ButtonLayout : Editor.IButtonLayout<Button, ButtonLayout> {
      public AbsoluteLayout Content { get; set; } = new AbsoluteLayout();
      public RectangleF Bounds { get; set; }
      public bool Visible { get => Content.IsVisible; set => Content.IsVisible = value; }
      public void Add(Button button) =>
        Content.Children.Add(button.Content, new Xamarin.Forms.Rectangle(button.Bounds.X, button.Bounds.Y, button.Bounds.Width, button.Bounds.Height));
      public void Add(ButtonLayout button) =>
        Content.Children.Add(button.Content, new Xamarin.Forms.Rectangle(button.Bounds.X, button.Bounds.Y, button.Bounds.Width, button.Bounds.Height));
    }
    public struct PainterSupplier : IPainterSupplier<EditableMathPainter<Button, ButtonLayout>> {
      public EditableMathPainter<Button, ButtonLayout> Default => new EditableMathPainter<Button, ButtonLayout>(null);
    }
    public EditableMathView(EditableMathPainter<Button, ButtonLayout> painter) : base(painter) {
      painter.TextModified += delegate { base.InvalidateSurface(); };
    }
    #region BindableProperties
    static EditableMathView() {
      var painter = default(PainterSupplier).Default;
      var thisType = typeof(EditableMathView);
      SkiaSharp.EditableMathPainter<Button, ButtonLayout> p(BindableObject b) => ((EditableMathView)b).Painter;
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType, painter.GlyphBoxColor,
        propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
    }
    public static readonly BindableProperty StrokeCapProperty;
    public static readonly BindableProperty GlyphBoxColorProperty;
    #endregion
    
    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public Atoms.MathList MathList => Painter.MathList;

    protected override void OnTouch(SKTouchEventArgs e) {
      base.OnTouch(e);
      if (e.ActionType != SKTouchAction.Pressed) return;
      Painter.Tap(new PointF(e.Location.X, e.Location.Y));
      e.Handled = true;
    }

    public static (EditableMathView view, Layout keyboard) Default {
      get {
        var view = Editor.MathKeyboardView<Button, ButtonLayout>.Default(
          (RectangleF frame, string text, float textPtSize, C title, C titleShadow, C highlightedTitle, C? disabled) => {
            var pressed = highlightedTitle.ToNative().ToFormsColor();
            var released = title.ToNative().ToFormsColor();
            var button = new Button {
              Content = new Xamarin.Forms.Button {
                Text = text,
                FontSize = textPtSize,
                TextColor = released
              },
              Bounds = frame
            };
            button.Content.Pressed += (sender, e) =>
              ((Xamarin.Forms.Button)sender).TextColor = pressed;
            button.Content.Released += (sender, e) =>
              ((Xamarin.Forms.Button)sender).TextColor = released;
            return button;
          }, () => new ButtonLayout(), (button, handler) => button.Content.Clicked += handler);
        var keyboard = view.layout;
        keyboard.Content.WidthRequest = keyboard.Bounds.Width;
        keyboard.Content.HeightRequest = keyboard.Bounds.Height;
        return (new EditableMathView (new EditableMathPainter<Button, ButtonLayout>(view)){ WidthRequest = view.layout.Bounds.Width, HeightRequest = view.layout.Bounds.Height }, keyboard.Content);
      }
    }
  }
}