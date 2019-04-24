using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration;
using System.ComponentModel;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace CSharpMath.Forms.Example
{
	public partial class App : Application
	{
    public static ObservableCollection<MathView> AllViews = new ObservableCollection<MathView>();
    public App() {
      InitializeComponent();
      /*
      var vm = new ViewModel();
      contentPage.BindingContext = vm;
      System.Threading.Tasks.Task.Run(async () => {
        vm.LaTeX = "1234";
        await System.Threading.Tasks.Task.Delay(5000);
        vm.LaTeX = "";
        vm.PropertyChanged(vm, new PropertyChangedEventArgs("LaTeX"));
        await System.Threading.Tasks.Task.Delay(5000);
        vm.LaTeX = @"\int^6_4 x dx";
        vm.PropertyChanged(vm, new PropertyChangedEventArgs("LaTeX"));
        await System.Threading.Tasks.Task.Delay(5000);
        vm.LaTeX = @"\int^6_4 x^2 dx";
        vm.PropertyChanged(vm, new PropertyChangedEventArgs("LaTeX"));
      });
      */
    }
    class ViewModel : System.ComponentModel.INotifyPropertyChanged {
      public string LaTeX;
      public PropertyChangedEventHandler PropertyChanged;
      event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
      }
    }
    int index = -1;
    void Handle_ChildAdded(object sender, ElementEventArgs e) {
      index++;
      if (Device.RuntimePlatform == Device.iOS && e.Element is Page p && !(p is ExamplesPage)) {
        p.Padding = new Thickness(0, index > 3 ? 90 : 30, 0, 0); //Pages after 4th page have an extra thicc tab bar on iOS
      }
    }

    protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
