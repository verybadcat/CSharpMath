using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MathKeyboard : ContentView {
    public MathKeyboard() {
      InitializeComponent();
    }

    public event EventHandler RedrawRequested {
      add => Keyboard.RedrawRequested += value;
      remove => Keyboard.RedrawRequested -= value;
    }
    public event EventHandler ReturnPressed {
      add => Keyboard.ReturnPressed += value;
      remove => Keyboard.ReturnPressed -= value;
    }
    public event EventHandler DismissPressed {
      add => Keyboard.DismissPressed += value;
      remove => Keyboard.DismissPressed -= value;
    }

    public IDisplay<Rendering.Fonts, Rendering.Glyph> Display => Keyboard.Display;

    private void SwitchTab(Grid tab) {
      tab.IsVisible = true;
      if (tab != Numbers) Numbers.IsVisible = false;
      if (tab != Operations) Operations.IsVisible = false;
      if (tab != Functions) Functions.IsVisible = false;
      if (tab != Letters) Letters.IsVisible = false;
      if (tab != Letters2) Letters2.IsVisible = false;
    }

    private void Shift_Clicked(object sender, EventArgs e) {
      Letters.IsVisible ^= true;
      Letters2.IsVisible ^= true;
    }

    private void NumbersButton_Clicked(object sender, EventArgs e) => SwitchTab(Numbers);
    private void OperationsButton_Clicked(object sender, EventArgs e) => SwitchTab(Operations);
    private void FunctionsButton_Clicked(object sender, EventArgs e) => SwitchTab(Functions);
    private void LettersButton_Clicked(object sender, EventArgs e) => SwitchTab(Letters);
  }
}