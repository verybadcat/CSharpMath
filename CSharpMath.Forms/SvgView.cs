using ICommand = System.Windows.Input.ICommand;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
namespace CSharpMath.Forms {
  public class SvgView : SKCanvasView {
    public SvgView() {
      EnableTouchEvents = true;
      Pressed = delegate {
        var param = CommandParameter;
        if (Command is Command command && command.CanExecute(param))
          command.Execute(param);
      };
    }

    public static readonly BindableProperty SourceProperty =
      BindableProperty.Create(nameof(Source), typeof(System.IO.Stream), typeof(SvgView));
    public System.IO.Stream Source {
      get => (System.IO.Stream)GetValue(SourceProperty);
      set => SetValue(SourceProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
      BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SvgView));
    public ICommand Command {
      get => (ICommand)GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
      BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SvgView));
    public object CommandParameter {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      if (Source is null) return;
      var svg = new SKSvg();
      svg.Load(Source);
      e.Surface.Canvas.DrawPicture(svg.Picture);
      base.OnPaintSurface(e);
    }
    protected override void OnTouch(SKTouchEventArgs e) {
      base.OnTouch(e);
      if (e.ActionType == SKTouchAction.Pressed)
        Pressed(this, System.EventArgs.Empty);
      e.Handled = true;
    }
    public event System.EventHandler Pressed;
  }
}
