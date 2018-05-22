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
		public SettingsPage ()
		{
			InitializeComponent ();
      App.AllViews.CollectionChanged += CollectionChanged;
      CollectionChanged(this, new Args(Action.Add, App.AllViews));
		}

    private void CollectionChanged(object sender, Args e) {
      if(e.NewItems != null) foreach (var v in e.NewItems.Cast<FormsLatexView>()) {
        v.DrawStringBoxes = DrawStringBoxes.On;
      } 
    }

    private void SwitchCell_OnChanged(object sender, ToggledEventArgs e) {
      foreach (var v in App.AllViews) {
        v.DrawStringBoxes = e.Value;
        v.InvalidateSurface();
      }
    }
  }
}