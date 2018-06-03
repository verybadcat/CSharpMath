using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Args = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using Action = System.Collections.Specialized.NotifyCollectionChangedAction;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
    public SettingsPage() {
      InitializeComponent();

      var values = typeof(SkiaSharp.SkiaTextAlignment).GetEnumValues();
      Array.Reverse(values);
      Alignment.ItemsSource = values;
      Alignment.SelectedItem = SkiaSharp.SkiaTextAlignment.Centre;

      App.AllViews.CollectionChanged += CollectionChanged;
      CollectionChanged(this, new Args(Action.Add, App.AllViews));
    }

    private void CollectionChanged(object sender, Args e) {
      if (e.NewItems != null) foreach (var v in e.NewItems.Cast<FormsLatexView>()) {
          v.DrawGlyphBoxes = DrawStringBoxes.On;
          v.TextAlignment = (SkiaSharp.SkiaTextAlignment)Alignment.SelectedItem;
          v.TextColor = TextColor.LabelColor;
          v.BackgroundColor = BackColor.LabelColor;
        }
    }

    private void SwitchCell_OnChanged(object sender, ToggledEventArgs e) {
      foreach (var v in App.AllViews) {
        v.DrawGlyphBoxes = e.Value;
        v.InvalidateSurface();
      }
    }

    private void Alignment_SelectedIndexChanged(object sender, EventArgs e) {
      foreach (var v in App.AllViews) {
        v.TextAlignment = (SkiaSharp.SkiaTextAlignment)Alignment.SelectedItem;
        v.InvalidateSurface();
      }
    }

    private void TextColor_Completed(object sender, EventArgs e) {
      TextColor.LabelColor =
        global::SkiaSharp.Views.Forms.Extensions.ToFormsColor
        (global::SkiaSharp.SKColor.TryParse(TextColor.Text, out var c) ? c : global::SkiaSharp.SKColors.Black);
      foreach (var v in App.AllViews) {
        v.TextColor = TextColor.LabelColor;
        v.InvalidateSurface();
      }
    }

    private void BackColor_Completed(object sender, EventArgs e) {
      BackColor.LabelColor =
        global::SkiaSharp.Views.Forms.Extensions.ToFormsColor
        (global::SkiaSharp.SKColor.TryParse(BackColor.Text, out var c) ? c : global::SkiaSharp.SKColors.Black);
      foreach (var v in App.AllViews) {
        v.BackgroundColor = BackColor.LabelColor;
        v.InvalidateSurface();
      }
    }
  }
}