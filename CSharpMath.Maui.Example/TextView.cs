using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;
using CSharpMath.SkiaSharp;

namespace CSharpMath.Maui.Example {
  public class TextView : SKCanvasView {

    public TextView() {
      EnableTouchEvents = false;
    }

    private TextPainter Painter { get; } = new TextPainter {

    };

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint) {
      Painter.LaTeX = LaTeX;
      Painter.FontSize = FontSize;
      var size = Painter.Measure((float)(widthConstraint - (this.Margin.Right + this.Margin.Left)));

      if (size is { } rect)
        return base.MeasureOverride(size.Width , size.Height + this.Margin.Top + this.Margin.Bottom);
      else
        return base.MeasureOverride(widthConstraint, heightConstraint);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      var canvas = e.Surface.Canvas;
      canvas.Clear();
      canvas.Scale(e.Info.Width / (float)Width);

      Painter.FontSize = FontSize;
      Painter.LaTeX = LaTeX;
      Painter.TextColor = TextColor.ToSKColor();

      Painter.Draw(canvas, CSharpMath.Rendering.FrontEnd.TextAlignment.TopLeft, new CSharpMath.Structures.Thickness(0, 0, 0, 0), 0, 0);
    }

    public string LaTeX {
      get => (string)GetValue(LaTeXProperty);
      set => SetValue(LaTeXProperty, value);
    }
    public static readonly BindableProperty LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), typeof(TextView), "",
          BindingMode.OneWay, validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);


    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color),
typeof(TextView), Microsoft.Maui.Graphics.Colors.Black, BindingMode.OneWay,
validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public Color TextColor {
      get => (Color)GetValue(TextColorProperty);
      set => SetValue(TextColorProperty, value);
    }


    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
     typeof(TextView), 16f, BindingMode.OneWay,
     validateValue: (_, value) => value != null && (float)value >= 0,
     propertyChanged: OnPropertyChangedInvalidate);

    public float FontSize {
      get => (float)GetValue(FontSizeProperty);
      set => SetValue(FontSizeProperty, value);
    }

    private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue) {
      var control = (TextView)bindable;

      if (oldvalue != newvalue) {
        control.InvalidateSurface();
      }

    }
  }
}
