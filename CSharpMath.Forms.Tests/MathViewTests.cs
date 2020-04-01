using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xunit;

namespace CSharpMath.Forms.Tests {
  public class MathViewTests {
    public MathViewTests() => Device.PlatformServices = new Xamarin.Forms.Core.UnitTests.MockPlatformServices();
    class ViewModel : INotifyPropertyChanged {
      string latex = "";
      public string LaTeX {
        get => latex;
        set { latex = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LaTeX))); }
      }
      Atom.MathList content = new Atom.MathList();
      public Atom.MathList Content {
        get => content;
        set { content = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content))); }
      }
      public event PropertyChangedEventHandler? PropertyChanged;
    }
    [Fact]
    public void LaTeXUpdatesContent() {
      var mathView = new MathView { LaTeX = "1" };
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("1")), mathView.Content);
    }
    [Fact]
    public void ContentUpdatesLaTeX() {
      var mathView = new MathView { Content = new Atom.MathList(new Atom.Atoms.Number("1")) };
      Assert.Equal("1", mathView.LaTeX);
    }
    [Fact]
    public void ContentIsBindable() {
      var viewModel = new ViewModel();
      var mathView = new MathView { BindingContext = viewModel };
      mathView.SetBinding(MathView.ContentProperty, nameof(ViewModel.Content));

      viewModel.Content = new Atom.MathList(new Atom.Atoms.Number("1"));
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("1")), mathView.Content);
      Assert.Equal("1", mathView.LaTeX);

      mathView.Content = new Atom.MathList(new Atom.Atoms.Number("2"));
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("1")), viewModel.Content); // Because one-way binding
      Assert.Equal("2", mathView.LaTeX);

      mathView.SetBinding(MathView.ContentProperty, nameof(ViewModel.Content), BindingMode.TwoWay);
      mathView.Content = new Atom.MathList(new Atom.Atoms.Number("3"));
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("3")), viewModel.Content);
      Assert.Equal("3", mathView.LaTeX);
    }
    [Fact]
    public void LaTeXIsBindable() {
      var viewModel = new ViewModel();
      var mathView = new MathView { BindingContext = viewModel };
      mathView.SetBinding(MathView.LaTeXProperty, nameof(ViewModel.LaTeX));

      viewModel.LaTeX = "1";
      Assert.Equal("1", mathView.LaTeX);
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("1")), mathView.Content);

      mathView.LaTeX = "2";
      Assert.Equal("1", viewModel.LaTeX); // Because one-way binding
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("2")), mathView.Content);

      mathView.SetBinding(MathView.LaTeXProperty, nameof(ViewModel.LaTeX), BindingMode.TwoWay);
      mathView.LaTeX = "3";
      Assert.Equal("3", viewModel.LaTeX);
      Assert.Equal(new Atom.MathList(new Atom.Atoms.Number("3")), mathView.Content);
    }
  }
}
