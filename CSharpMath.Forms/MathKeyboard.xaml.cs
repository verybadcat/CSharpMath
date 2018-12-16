using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {[XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MathKeyboard : ContentView {

    public MathKeyboard() {
      InitializeComponent();
    }

    public Rendering.MathKeyboard Keyboard { get; } = new Rendering.MathKeyboard();
  }
}