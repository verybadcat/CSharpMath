using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.MathKeyboardMarkupExtensions {
  [AcceptEmptyServiceProvider]
  public class StringArrayExtension : IMarkupExtension<string[]> {


    public string[] ProvideValue(IServiceProvider _) => null;
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }

  [AcceptEmptyServiceProvider]
  public class CycleButton : IMarkupExtension<Command> {
    public static BindableProperty Texts = BindableProperty.CreateAttached(
      nameof(Texts), typeof(string[]), typeof(CycleButton), null,
      validateValue: (b, v) => b is Button
    );
    public static string[] GetTexts(Button button) => (string[])button.GetValue(Texts);
    public static void SetTexts(Button button, string[] value) => button.SetValue(Texts, value);

    static void CycleL(Layout<View> e) {
      if (e is Layout<View> l)
        foreach (var v in l.Children)
          if (v is Layout<View> ll)
            CycleL(ll);
          else if (v is Button b)
            CycleB(b);
    }
    static void CycleB(Button b) {
      var texts = GetTexts(b);
      if (texts is null || texts.Length is 0) return;
      var index = Array.IndexOf(texts, b.Text);
      if (index is -1 || index == texts.Length - 1) b.Text = texts[0];
      else b.Text = texts[index + 1];
    }

    public VisualElement Input { get; set; }

    public Command ProvideValue(IServiceProvider _) =>
      Input is null ? throw new ArgumentNullException(nameof(Input)) :
      Input is Layout<View> l ? new Command(() => CycleL(l)) :
      Input is Button b ? new Command(() => CycleB(b)) :
      throw new ArgumentException(
        $"{nameof(Input)} was not {nameof(Layout)}<{nameof(View)}> or {nameof(Button)}.", nameof(Input));
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }

  [AcceptEmptyServiceProvider]
  public class MathInputExtension : IMarkupExtension<Command> {
    public Editor.MathKeyboardInput Input { get; set; }
    public Rendering.MathKeyboard Keyboard { get; set; }

    public Command ProvideValue(IServiceProvider _) =>
      Input is 0 ? throw new ArgumentNullException(nameof(Input)) :
      Keyboard is null ? throw new ArgumentNullException(nameof(Keyboard)) :
      new Command(() => Keyboard.KeyPress(Input));
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }

  [AcceptEmptyServiceProvider, ContentProperty(nameof(Target))]
  public class ToggleVisibilityExtension : IMarkupExtension<Command> {
    public VisualElement Target { get; set; }

    public Command ProvideValue(IServiceProvider _) =>
      Target is null ? throw new ArgumentNullException(nameof(Target)) :
      new Command(() => Target.IsVisible ^= true);
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }

  [AcceptEmptyServiceProvider, ContentProperty(nameof(Base))]
  public class MultiplyExtension : IMarkupExtension<double> {
    public double Base { get; set; }
    public double By { get; set; }

    public double ProvideValue(IServiceProvider _) => Base * By;
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
}
