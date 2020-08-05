using System;
using System.Linq;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using Action = System.Collections.Specialized.NotifyCollectionChangedAction;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CSharpMath.Rendering.FrontEnd;
using System.Globalization;
using System.Net;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class SettingsPage : ContentPage {
    public SettingsPage() {
      InitializeComponent();

      var values = typeof(Rendering.FrontEnd.TextAlignment).GetEnumValues();
      Array.Reverse(values);
      Alignment.ItemsSource = values;
      Alignment.SelectedItem = Rendering.FrontEnd.TextAlignment.Top;

      values = typeof(PaintStyle).GetEnumValues();
      PaintStyle.ItemsSource = values;
      PaintStyle.SelectedItem = Rendering.FrontEnd.PaintStyle.Fill;

      values = typeof(Atom.LineStyle).GetEnumValues();
      LineStyle.ItemsSource = values;
      LineStyle.SelectedItem = Atom.LineStyle.Display;

      DrawGlyphBoxes_OnChanged(this, new ToggledEventArgs(DrawGlyphBoxes.On));
      TextColor_Completed(this, EventArgs.Empty);
      HighlightColor_Completed(this, EventArgs.Empty);
      BackColor_Completed(this, EventArgs.Empty);

      App.AllMathViews.CollectionChanged += CollectionChanged<SkiaSharp.MathPainter, Atom.MathList>;
      CollectionChanged<SkiaSharp.MathPainter, Atom.MathList>(this, new Args(Action.Add, App.AllMathViews));
      App.AllTextViews.CollectionChanged += CollectionChanged<SkiaSharp.TextPainter, Rendering.Text.TextAtom>;
      CollectionChanged<SkiaSharp.TextPainter, Rendering.Text.TextAtom>(this, new Args(Action.Add, App.AllTextViews));
    }

    Color Parse(string color) => global::SkiaSharp.Views.Forms.Extensions.ToFormsColor
        (global::SkiaSharp.SKColor.TryParse(color, out var c) ? c : global::SkiaSharp.SKColors.Black);

    private void CollectionChanged<TPainter, TContent>(object sender, Args e)
      where TPainter : Painter<global::SkiaSharp.SKCanvas, TContent, global::SkiaSharp.SKColor>, new() where TContent : class {
      if (e.NewItems != null) foreach (var v in e.NewItems.Cast<BaseView<TPainter, TContent>>()) {
          v.GlyphBoxColor = DrawGlyphBoxes.On ? (Parse(GlyphBoxColor.Text), Parse(GlyphRunColor.Text)) : default((Color glyph, Color textRun)?);
          v.TextAlignment = (Rendering.FrontEnd.TextAlignment)Alignment.SelectedItem;
          v.TextColor = TextColor.LabelColor;
          v.HighlightColor = HighlightColor.LabelColor;
          v.BackgroundColor = BackColor.LabelColor;
          v.PaintStyle = (PaintStyle)PaintStyle.SelectedItem;
          v.LineStyle = (Atom.LineStyle)LineStyle.SelectedItem;
        }
    }

    private void Alignment_SelectedIndexChanged(object sender, EventArgs e) {
      foreach (var v in App.AllMathViews) {
        v.TextAlignment = (Rendering.FrontEnd.TextAlignment)Alignment.SelectedItem;
      }
      foreach (var v in App.AllTextViews) {
        v.TextAlignment = (Rendering.FrontEnd.TextAlignment)Alignment.SelectedItem;
      }
    }

    private void TextColor_Completed(object sender, EventArgs e) {
      TextColor.LabelColor = Parse(TextColor.Text);
      foreach (var v in App.AllMathViews) {
        v.TextColor = TextColor.LabelColor;
      }
      foreach (var v in App.AllTextViews) {
        v.TextColor = TextColor.LabelColor;
      }
    }

    private void HighlightColor_Completed(object sender, EventArgs e) {
      HighlightColor.LabelColor = Parse(HighlightColor.Text);
      foreach (var v in App.AllMathViews) {
        v.HighlightColor = HighlightColor.LabelColor;
      }
      foreach (var v in App.AllTextViews) {
        v.HighlightColor = HighlightColor.LabelColor;
      }
    }

    private void BackColor_Completed(object sender, EventArgs e) {
      BackColor.LabelColor = Parse(BackColor.Text);
      foreach (var v in App.AllMathViews) {
        v.BackgroundColor = BackColor.LabelColor;
      }
      foreach (var v in App.AllTextViews) {
        v.BackgroundColor = BackColor.LabelColor;
      }
    }

    private void GlyphBoxColor_Completed(object sender, EventArgs e) {
      GlyphBoxColor.LabelColor = Parse(GlyphBoxColor.Text);
      foreach (var v in App.AllMathViews) if (v.GlyphBoxColor is { } value) {
        value.glyph = Parse(GlyphBoxColor.Text);
        v.GlyphBoxColor = value;
      }
      foreach (var v in App.AllTextViews) if (v.GlyphBoxColor is { } value) {
        value.glyph = Parse(GlyphBoxColor.Text);
        v.GlyphBoxColor = value;
      }
    }

    private void GlyphRunColor_Completed(object sender, EventArgs e) {
      GlyphRunColor.LabelColor = Parse(GlyphRunColor.Text);
      foreach (var v in App.AllMathViews) if (v.GlyphBoxColor is { } value) {
        value.textRun = Parse(GlyphRunColor.Text);
        v.GlyphBoxColor = value;
      }
      foreach (var v in App.AllTextViews) if (v.GlyphBoxColor is { } value) {
        value.textRun = Parse(GlyphRunColor.Text);
        v.GlyphBoxColor = value;
      }
    }

    private void DrawGlyphBoxes_OnChanged(object sender, ToggledEventArgs e) {
      GlyphBoxColor.IsEnabled = e.Value;
      GlyphRunColor.IsEnabled = e.Value;
      GlyphBoxColor.LabelColor = Parse(GlyphBoxColor.Text);
      GlyphRunColor.LabelColor = Parse(GlyphRunColor.Text);
      foreach (var v in App.AllMathViews) {
        v.GlyphBoxColor = e.Value ? (Parse(GlyphBoxColor.Text), Parse(GlyphRunColor.Text)) : default((Color glyph, Color textRun)?);
      }
      foreach (var v in App.AllTextViews) {
        v.GlyphBoxColor = e.Value ? (Parse(GlyphBoxColor.Text), Parse(GlyphRunColor.Text)) : default((Color glyph, Color textRun)?);
      }
    }

    private void PaintStyle_SelectedIndexChanged(object sender, EventArgs e) {
      foreach (var v in App.AllMathViews) {
        v.PaintStyle = (PaintStyle)PaintStyle.SelectedItem;
      }
      foreach (var v in App.AllTextViews) {
        v.PaintStyle = (PaintStyle)PaintStyle.SelectedItem;
      }
    }

    private void LineStyle_SelectedIndexChanged(object sender, EventArgs e) {
      foreach (var v in App.AllMathViews) {
        v.LineStyle = (Atom.LineStyle)LineStyle.SelectedItem;
      }
      foreach (var v in App.AllTextViews) {
        v.LineStyle = (Atom.LineStyle)LineStyle.SelectedItem;
      }
    }
  }
}