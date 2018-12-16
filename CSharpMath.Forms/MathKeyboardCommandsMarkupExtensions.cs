using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.MathKeyboardCommandsMarkupExtensions {
  public abstract class CommandExtension : IMarkupExtension<Command> {
    public abstract Command ProvideCommand();
    public Command ProvideValue(IServiceProvider serviceProvider) => ProvideCommand();
    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
      ProvideValue(serviceProvider);
  }

  public class MathInputExtension : CommandExtension {
    public Editor.MathKeyboardInput Input { get; set; }
    public Rendering.MathKeyboard Keyboard { get; set; }

    public override Command ProvideCommand() =>
      Input is 0 ? throw new ArgumentNullException(nameof(Input)) :
      Keyboard is null ? throw new ArgumentNullException(nameof(Keyboard)) :
      new Command(() => Keyboard.KeyPress(Input));
  }

  public class 
}
