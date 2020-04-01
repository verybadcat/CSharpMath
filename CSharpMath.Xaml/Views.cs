using System;
using System.Collections.Generic;
using System.Text;

#if Avalonia
using Property = Avalonia.AvaloniaProperty;
namespace CSharpMath.Avalonia {
#elif Forms
using Property = Xamarin.Forms.BindableProperty;
namespace CSharpMath.Forms {
#endif
  class Views {

  }
}
