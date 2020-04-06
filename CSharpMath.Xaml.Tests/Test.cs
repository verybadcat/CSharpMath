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
    where TMathView : TBaseView, ICSharpMathAPI<MathList, TColor>, new()
    where TTextView : TBaseView, ICSharpMathAPI<TextAtom, TColor>, new() {
    class ViewModel<TContent> : INotifyPropertyChanged where TContent : class {
      void SetAndRaise<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null) {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      }
      string? latex = "";
      public string? LaTeX { get => latex; set => SetAndRaise(ref latex, value); }
      TContent? content;
      public TContent? Content { get => content; set => SetAndRaise(ref content, value); }
      public event PropertyChangedEventHandler? PropertyChanged;
    }
    protected abstract string FrontEndNamespace { get; }
    protected abstract TBindingMode Default { get; }
    protected abstract TBindingMode OneWayToSource { get; }
    protected abstract TBindingMode TwoWay { get; }
    protected abstract TView ParseFromXaml<TView>(string xaml) where TView : TBaseView, new();
    protected abstract void SetBindingContext(TBaseView view, object viewModel);
    protected abstract IDisposable SetBinding(TBaseView view, TProperty property, string viewModelProperty, TBindingMode bindingMode);
    IDisposable SetBinding<TView>(TView view, string propertyName, TBindingMode? bindingMode = null) where TView : TBaseView =>
      SetBinding(view,
        (TProperty)typeof(TView)
        .GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        ?.GetValue(view) ?? throw new NotImplementedException($"Property not found in {typeof(TView)}: {propertyName}"),
        propertyName,
        bindingMode ?? Default);
    [Fact]
    public void LaTeXUpdatesContent() {
      static void Test<TView, TContent>(TView view, TContent content)
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor> where TContent : class {
        view.LaTeX = "1";
        Assert.Equal(content, view.Content);
        Assert.Equal("1", view.LaTeX);
        Assert.Null(view.ErrorMessage);
      }
      Test(new TMathView(), new MathList(new Atom.Atoms.Number("1")));
      Test(new TTextView(), (TextAtom)new TextAtom.Text("1"));
    }
    [Fact]
    public void ContentUpdatesLaTeX() {
      static void Test<TView, TContent>(TView view, TContent content)
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor> where TContent : class {
        view.Content = content;
        Assert.Equal(view.Content, content);
        Assert.Equal("1", view.LaTeX);
        Assert.Null(view.ErrorMessage);
      }
      Test(new TMathView(), new MathList(new Atom.Atoms.Number("1")));
      Test(new TTextView(), (TextAtom)new TextAtom.Text("1"));
    }
    [Fact]
    public void ContentIsBindable() {
      void Test<TView, TContent>(TView view, TContent one, TContent two, TContent three)
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor> where TContent : class {
        var viewModel = new ViewModel<TContent>();
        SetBindingContext(view, viewModel);
        using (var binding = SetBinding(view, nameof(viewModel.Content))) {
          viewModel.Content = one;
          Assert.Equal(one, view.Content);
          Assert.Equal("1", view.LaTeX);
          Assert.Null(view.ErrorMessage);

          view.Content = two;
          Assert.Equal(one, viewModel.Content); // Because one-way binding
          Assert.Equal("2", view.LaTeX);
          Assert.Null(view.ErrorMessage);
        }
        using (var binding = SetBinding(view, nameof(viewModel.Content), TwoWay)) {
          view.Content = three;
          Assert.Equal(three, viewModel.Content);
          Assert.Equal("3", view.LaTeX);
          Assert.Null(view.ErrorMessage);
        }
      }
      Test(new TMathView(),
        new MathList(new Atom.Atoms.Number("1")),
        new MathList(new Atom.Atoms.Number("2")),
        new MathList(new Atom.Atoms.Number("3")));
      Test(new TTextView(),
        (TextAtom)new TextAtom.Text("1"),
        new TextAtom.Text("2"),
        new TextAtom.Text("3"));
    }
    [Fact]
    public void LaTeXIsBindable() {
      void Test<TView, TContent>(TView view, TContent one, TContent two, TContent three)
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor> where TContent : class {
        var viewModel = new ViewModel<TContent>();
        SetBindingContext(view, viewModel);
        using (var binding = SetBinding(view, nameof(viewModel.LaTeX))) {

          viewModel.LaTeX = "1";
          Assert.Equal("1", view.LaTeX);
          Assert.Equal(one, view.Content);
          Assert.Null(view.ErrorMessage);

          view.LaTeX = "2";
          Assert.Equal("1", viewModel.LaTeX); // Because one-way binding
          Assert.Equal(two, view.Content);
          Assert.Null(view.ErrorMessage);
        }

        using (var binding = SetBinding(view, nameof(viewModel.LaTeX), TwoWay)) {
          view.LaTeX = "3";
          Assert.Equal("3", viewModel.LaTeX);
          Assert.Equal(three, view.Content);
          Assert.Null(view.ErrorMessage);
        }
      }
      Test(new TMathView(),
        new MathList(new Atom.Atoms.Number("1")),
        new MathList(new Atom.Atoms.Number("2")),
        new MathList(new Atom.Atoms.Number("3")));
      Test(new TTextView(),
        (TextAtom)new TextAtom.Text("1"),
        new TextAtom.Text("2"),
        new TextAtom.Text("3"));
    }
    [Fact]
    public void ErrorMessageUpdates() {
      void Test<TView, TContent>(TView view, TContent oneTwoThree)
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor> where TContent : class {
        var viewModel = new ViewModel<TContent>();
        SetBindingContext(view, viewModel);
        using (var binding = SetBinding(view, nameof(viewModel.LaTeX))) {

          viewModel.LaTeX = @"\alpha\beta\gamme";
          Assert.Equal(@"\alpha\beta\gamme", view.LaTeX);
          Assert.Null(view.Content);
          Assert.Equal("Error: Invalid command \\gamme\n\\alpha\\beta\\gamme\n                ↑ (pos 17)", view.ErrorMessage);
        }
        using (var binding = SetBinding(view, nameof(viewModel.LaTeX), OneWayToSource)) {
          view.LaTeX = @"123";
          Assert.Equal(@"123", view.LaTeX);
          Assert.Equal(@"123", viewModel.LaTeX);
          Assert.Equal(oneTwoThree, view.Content);
          Assert.Null(view.ErrorMessage);

          using var innerBinding = SetBinding(view, nameof(viewModel.LaTeX));
          viewModel.LaTeX = @"\\\";
          if (typeof(TBindingMode) == typeof(global::Avalonia.Data.BindingMode)) {
            // Avalonia processes bindings from newest to oldest
            Assert.Equal(@"123", view.LaTeX);
            Assert.Equal(@"123", viewModel.LaTeX);
            Assert.Equal(oneTwoThree, view.Content);
            Assert.Null(view.ErrorMessage);
          } else if (typeof(TBindingMode) == typeof(Xamarin.Forms.BindingMode)) {
            // Xamarin Forms processes bindings from oldest to newest
            Assert.Equal(@"\\\", view.LaTeX);
            Assert.Equal(@"\\\", viewModel.LaTeX);
            Assert.Null(view.Content);
            Assert.Equal("Error: Invalid command \\\n\\\\\\\n  ↑ (pos 3)", view.ErrorMessage);
          } else throw new NotImplementedException();
        }
        using (var binding = SetBinding(view, nameof(viewModel.LaTeX))) {
          viewModel.LaTeX = @"}";
          Assert.Equal(@"}", view.LaTeX);
          Assert.Null(view.Content);
          Assert.Equal("Error: Missing opening brace\n}\n↑ (pos 1)", view.ErrorMessage);
        }
      }
      Test(new TMathView(), new MathList(new Atom.Atoms.Number("1"), new Atom.Atoms.Number("2"), new Atom.Atoms.Number("3")));
      Test(new TTextView(), (TextAtom)new TextAtom.Text("123"));
    }
    [Fact]
    public void LaTeXAutoFormats() {
      void Test<TView, TContent>(TView view, TContent alphaBetaGamma)
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor> where TContent : class {
        var viewModel = new ViewModel<TContent>();
        SetBindingContext(view, viewModel);
        using var binding = SetBinding(view, nameof(viewModel.LaTeX));

        viewModel.LaTeX = @"\alpha\beta\gamma";
        Assert.Equal(@"\alpha \beta \gamma ", view.LaTeX);
        Assert.Equal(alphaBetaGamma, view.Content);
        Assert.Null(view.ErrorMessage);
      }
      Test(new TMathView(), new MathList(new Atom.Atoms.Variable("α"), new Atom.Atoms.Variable("β"), new Atom.Atoms.Variable("γ")));
      Test(new TTextView(), (TextAtom)new TextAtom.List(new[] { new TextAtom.Text("α"), new TextAtom.Text("β"), new TextAtom.Text("γ") }));
    }
    [Fact]
    public void TypefacesAreBindable() {
      var typeface = new Typography.OpenFont.OpenFontReader().Read(
        Assembly.GetExecutingAssembly().GetManifestResourceStream("CSharpMath.Xaml.Tests.ComicNeue_Bold.otf")
        ?? throw new Structures.InvalidCodePathException("Font not found"))
        ?? throw new Structures.InvalidCodePathException("Font is invalid");
      void Test<TView, TContent>()
        where TView : TBaseView, ICSharpMathAPI<TContent, TColor>, new() where TContent : class {
        var view = new TView();
        var viewModel = new { LocalTypefaces = new[] { typeface } };
        SetBindingContext(view, viewModel);
        Assert.Empty(view.LocalTypefaces);

        using var binding = SetBinding(view, nameof(viewModel.LocalTypefaces));
        viewModel = new { LocalTypefaces = Array.Empty<Typography.OpenFont.Typeface>() };
        Assert.Same(typeface, Assert.Single(view.LocalTypefaces));

        SetBindingContext(view, viewModel);
        Assert.Empty(view.LocalTypefaces);
      }
      Test<TMathView, MathList>();
      Test<TTextView, TextAtom>();
    }
    [Fact]
    public void ParseXaml() {
      var xaml = $@"<MathView xmlns=""clr-namespace:CSharpMath.{FrontEndNamespace};assembly=CSharpMath.{FrontEndNamespace}"" LaTeX=""1\leq 2"" />";
      var math = ParseFromXaml<TMathView>(xaml);
      Assert.Equal(@"1\leq 2", math.LaTeX);
      Assert.Equal(new MathList(new Atom.Atoms.Number("1"), new Atom.Atoms.Relation("≤"), new Atom.Atoms.Number("2")), math.Content);

      xaml = $@"<TextView xmlns=""clr-namespace:CSharpMath.{FrontEndNamespace};assembly=CSharpMath.{FrontEndNamespace}"" LaTeX=""a b"" />";
      var text = ParseFromXaml<TTextView>(xaml);
      Assert.Equal(@"a\ b", text.LaTeX);
      Assert.Equal(new TextAtom.List(new TextAtom[] { new TextAtom.Text("a"), new TextAtom.ControlSpace(), new TextAtom.Text("b") }), text.Content);
    }
  }
}
