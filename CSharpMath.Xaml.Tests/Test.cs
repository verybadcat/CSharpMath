using System;
using System.ComponentModel;
using System.Reflection;
using Xunit;

namespace CSharpMath.Xaml.Tests {
  using Atom;
  using Rendering.FrontEnd;
  using Rendering.Text;
  [CollectionDefinition(nameof(TestFixture))]
  public class TestFixture : ICollectionFixture<TestFixture> {
    public TestFixture() {
      Xamarin.Forms.Device.PlatformServices = new Xamarin.Forms.Core.UnitTests.MockPlatformServices();
    }
  }
  [Collection(nameof(TestFixture))]
  public abstract class Test<TColor, TBindingMode, TProperty, TBaseView, TMathView, TTextView>
    where TBindingMode : struct, Enum
    where TMathView : TBaseView, IPainter<MathList, TColor>, new()
    where TTextView : TBaseView, IPainter<TextAtom, TColor>, new() {
    class ViewModel : INotifyPropertyChanged {
      void SetAndRaise<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null) {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      }
      string latex = "";
      public string LaTeX { get => latex; set => SetAndRaise(ref latex, value); }
      MathList content = new MathList();
      public MathList Content { get => content; set => SetAndRaise(ref content, value); }
      public event PropertyChangedEventHandler? PropertyChanged;
    }
    protected abstract TBindingMode Default { get; }
    protected abstract TBindingMode TwoWay { get; }
    protected abstract void SetBindingContext(TBaseView view, object viewModel);
    protected abstract void SetBinding(TBaseView view, TProperty property, string viewModelProperty, TBindingMode bindingMode);
    void SetBinding<TView>(TView view, string propertyName, TBindingMode? bindingMode = null) where TView : TBaseView =>
      SetBinding(view,
        (TProperty)typeof(TView)
        .GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        ?.GetValue(view) ?? throw new NotImplementedException($"Property not found in {typeof(TView)}: {propertyName}"),
        propertyName,
        bindingMode ?? Default);
    [Fact]
    public void LaTeXUpdatesContent() {
      var mathView = new TMathView { LaTeX = "1" };
      Assert.Equal(new MathList(new Atom.Atoms.Number("1")), mathView.Content);
      Assert.Null(mathView.ErrorMessage);
    }
    [Fact]
    public void ContentUpdatesLaTeX() {
      var mathView = new TMathView { Content = new MathList(new Atom.Atoms.Number("1")) };
      Assert.Equal("1", mathView.LaTeX);
      Assert.Null(mathView.ErrorMessage);
    }
    [Fact]
    public void ContentIsBindable() {
      var viewModel = new ViewModel();
      var mathView = new TMathView();
      SetBindingContext(mathView, viewModel);
      SetBinding(mathView, nameof(ViewModel.Content));

      viewModel.Content = new MathList(new Atom.Atoms.Number("1"));
      Assert.Equal(new MathList(new Atom.Atoms.Number("1")), mathView.Content);
      Assert.Equal("1", mathView.LaTeX);
      Assert.Null(mathView.ErrorMessage);

      mathView.Content = new MathList(new Atom.Atoms.Number("2"));
      Assert.Equal(new MathList(new Atom.Atoms.Number("1")), viewModel.Content); // Because one-way binding
      Assert.Equal("2", mathView.LaTeX);
      Assert.Null(mathView.ErrorMessage);

      SetBinding(mathView, nameof(ViewModel.Content), TwoWay);
      mathView.Content = new MathList(new Atom.Atoms.Number("3"));
      Assert.Equal(new MathList(new Atom.Atoms.Number("3")), viewModel.Content);
      Assert.Equal("3", mathView.LaTeX);
      Assert.Null(mathView.ErrorMessage);
    }
    [Fact]
    public void LaTeXIsBindable() {
      var viewModel = new ViewModel();
      var mathView = new TMathView();
      SetBindingContext(mathView, viewModel);
      SetBinding(mathView, nameof(ViewModel.LaTeX));

      viewModel.LaTeX = "1";
      Assert.Equal("1", mathView.LaTeX);
      Assert.Equal(new MathList(new Atom.Atoms.Number("1")), mathView.Content);
      Assert.Null(mathView.ErrorMessage);

      mathView.LaTeX = "2";
      Assert.Equal("1", viewModel.LaTeX); // Because one-way binding
      Assert.Equal(new MathList(new Atom.Atoms.Number("2")), mathView.Content);
      Assert.Null(mathView.ErrorMessage);

      SetBinding(mathView, nameof(ViewModel.LaTeX), TwoWay);
      mathView.LaTeX = "3";
      Assert.Equal("3", viewModel.LaTeX);
      Assert.Equal(new MathList(new Atom.Atoms.Number("3")), mathView.Content);
      Assert.Null(mathView.ErrorMessage);
    }
    [Fact]
    public void LaTeXIsBindable_Error() {
      var viewModel = new ViewModel();
      var mathView = new TMathView();
      SetBindingContext(mathView, viewModel);
      SetBinding(mathView, nameof(ViewModel.LaTeX));

      viewModel.LaTeX = @"\alpha\beta\gamme";
      Assert.Equal(@"\alpha\beta\gamme", mathView.LaTeX);
      Assert.Null(mathView.Content);
      Assert.Equal(@"Invalid command \gamme", mathView.ErrorMessage);
    }
    [Fact]
    public void LaTeXIsBindable_Formatting() {
      var viewModel = new ViewModel();
      var mathView = new TMathView();
      SetBindingContext(mathView, viewModel);
      SetBinding(mathView, nameof(ViewModel.LaTeX));

      viewModel.LaTeX = @"\alpha\beta\gamma";
      Assert.Equal(@"\alpha \beta \gamma ", mathView.LaTeX);
      Assert.Equal(new MathList(new Atom.Atoms.Variable("α"), new Atom.Atoms.Variable("β"), new Atom.Atoms.Variable("γ")), mathView.Content);
      Assert.Null(mathView.ErrorMessage);
    }
    [Fact]
    public void LaTeXIsBindable_Formatting2() {
      var viewModel = new ViewModel();
      var mathView = new TMathView();
      SetBindingContext(mathView, viewModel);
      SetBinding(mathView, nameof(ViewModel.LaTeX));

      viewModel.LaTeX = @"\begin{cases} y=x^2-x+3 \\ y=x^2+\sqrt x-\frac2x \end{cases}";
      Assert.Equal(@"\left\{ \, \begin{array}{l}\textstyle y=x^2-x+3\\ \textstyle y=x^2+\sqrt{x}-\frac{2}{x}\end{array}\right. ", mathView.LaTeX);
      Assert.NotNull(mathView.Content);
      Assert.Null(mathView.ErrorMessage);
    }
  }
}
