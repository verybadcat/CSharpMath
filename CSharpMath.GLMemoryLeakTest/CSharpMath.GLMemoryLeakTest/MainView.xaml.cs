using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaLeakMinProject
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ContentPage
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Loop(Func<View> buildView, string msg)
        { 
            for (var c = 1; c < 30; c++)
            {
                container.Children.Clear();

                for (var i = 1; i < 150; i++)
                {
                    container.Children.Add(buildView());
                }
            }
            infoLabel.Text = "Done: " + msg + ". 30 iteratoins, 150 items: "  ;
        }

        private void BlankCanvas_Button_Click(object sender, EventArgs e)
        {
            Loop(() => new CanvasView(), "blank canvas");
        }

        private void BlankGLView_Button_Click(object sender, EventArgs e)
        {
            Loop(() => new GLView(), "blank gl view");
        }

        private void GLMathView_Button_Click(object sender, EventArgs e)
        {
            Loop(() => new GLMathView(), "math GL view");
        }

        private void MathViewCanvasButton_Click(object sender, EventArgs e)
        {
            Loop(() => new CanvasMathView(), "math canvas view");
        }

        private void LabelTest_Button_Click(object sender, EventArgs e)
        {
            Loop(() => new Label() { Text = "Label" }, "Label");
        }

        private void Clear_Button_Clicked(object sender, EventArgs e)
        {
            container.Children.Clear(); 
            GC.Collect();
            GC.Collect();
        }
    }
}