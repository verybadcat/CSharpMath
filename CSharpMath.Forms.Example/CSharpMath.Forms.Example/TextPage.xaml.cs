using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TextPage : ContentPage
	{
    public TextPage() {
      InitializeComponent();
      //Text.Text = @"Here are some text. This text is made to be long enough to have the TextPainter of CSharpMath (hopefully) add a line break to this text automatically. To demonstrate the capabilities of the TextPainter, here are some math content: First, a fraction in inline mode: $\frac34$ Next, a summation in inline mode: $\sum_{i=0}^3i^i$ Then, a summation in display mode: $$\sum_{i=0}^3i^i$$ (ah, bugs.) After that, an integral in display mode: $$\int^6_{-56}x\ dx$$ Finally, an escaped dollar sign \$ that represents the start/end of math mode when it is unescaped. Even though colours are currently unsupported, it can be done via math mode with the \\color command with the help of the \\text command. It looks like this: $\color{#F00}{\text{some red text}}$, which is nearly indistinguishable from non-math mode aside from not being able to automatically break up when spaces are inside the coloured text. The SkiaSharp version of this is located at CSharpMath.SkiaSharp.TextPainter; and the Xamarin.Forms version of this is located at CSharpMath.Forms.TextView. Was added in 0.1.0-pre4; working in 0.1.0-pre5.";
      //However, it is also not yet complete as in not being able to SeparateThisReallyLongWordWhichIsSoLongThatItSpansAcrossTheEntirePageAndWontStopEvenWhenPartOfItIsOutOfBounds.
    }

    private double width = 0;
    private double height = 0;
    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);
      if (width != this.width || height != this.height) {
        this.width = width;
        this.height = height;
        if (width > height) {
          outerStack.Orientation = StackOrientation.Horizontal;
          innerStack.Orientation = StackOrientation.Vertical;
        } else {
          outerStack.Orientation = StackOrientation.Vertical;
          innerStack.Orientation = StackOrientation.Horizontal;
        }
      }
    }

    private void Size_SelectedIndexChanged(object sender, EventArgs e) {
      View.FontSize = (float)Size.SelectedItem;
      View.InvalidateSurface();
    }

    private void Text_TextChanged(object sender, TextChangedEventArgs e) {
      View.LaTeX = e.NewTextValue;
      View.InvalidateSurface();
    }
  }
}
