using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Avalonia.Example.Pages {
  public class CalculatorPage : UserControl {
    public CalculatorPage() => AvaloniaXamlLoader.Load(this);
  }
  class CalculatorPageConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      value is string latex
      ? Atom.LaTeXParser.MathListFromLaTeX(latex)
        .Bind(list => Evaluation.Interpret(list))
        .Match(success => success, error => latex)
      : value;
    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      throw new NotImplementedException();
    public static CalculatorPageConverter Singleton { get; } = new CalculatorPageConverter();
  }
}