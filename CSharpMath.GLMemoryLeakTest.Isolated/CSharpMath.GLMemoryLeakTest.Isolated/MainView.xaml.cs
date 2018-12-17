using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.GLMemoryLeakTest.Isolated
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainView : ContentView
	{
		public MainView ()
		{
			InitializeComponent ();
		}
	}
}