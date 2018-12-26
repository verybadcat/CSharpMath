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
      SwitchTab = new Command(tab => {
        if (tab != Numbers) Numbers.IsVisible = false;
        if (tab != Operations) Operations.IsVisible = false;
        if (tab != Functions) Functions.IsVisible = false;
        if (tab != Letters) Letters.IsVisible = false;
        if (tab != Letters2) Letters2.IsVisible = false;
      });
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

    public Rendering.MathKeyboard Keyboard = new Rendering.MathKeyboard();

    private Command SwitchTab;

    private void Shift_Clicked(object sender, EventArgs e) {
      Letters.IsVisible ^= true;
      Letters2.IsVisible ^= true;
      
    }
  }
}