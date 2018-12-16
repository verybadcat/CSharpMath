using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.MathKeyboardMarkupExtensions {
  using T = Xamarin.Forms.Command;
  public abstract class MarkUpExtensionBase<T>: IMarkupExtension<T> {
    public abstract T ProvideValue();
    public T ProvideValue(IServiceProvider serviceProvider) => ProvideValue();
    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
      ProvideValue(serviceProvider);
  }

  [AcceptEmptyServiceProvider]
  public class MathInputExtension : MarkUpExtensionBase<Command> {
    public Editor.MathKeyboardInput Input { get; set; }
    public Rendering.MathKeyboard Keyboard { get; set; }

    public override Command ProvideValue() =>
      Input is 0 ? throw new ArgumentNullException(nameof(Input)) :
      Keyboard is null ? throw new ArgumentNullException(nameof(Keyboard)) :
      new Command(() => Keyboard.KeyPress(Input));
  }

  [AcceptEmptyServiceProvider]
  public class ToggleVisibilityExtension : MarkUpExtensionBase<Command> {
    public VisualElement Target { get; set; }

    public override Command ProvideValue() =>
      Target is null ? throw new ArgumentNullException(nameof(Target)) :
      new Command(() => Target.IsVisible ^= true);
  }
  
}
