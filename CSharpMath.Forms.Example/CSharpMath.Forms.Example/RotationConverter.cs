using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace CSharpMath.Forms.Example
{
  public class RotationConverter : BindableObject, IValueConverter {
    public int Count { get; set; }
    public Point Centre { get; set; }
    public double Radius { get; set; }

    public int Index { get => (int)GetValue(IndexProperty);
      set => SetValue(IndexProperty, value); }
    public static BindableProperty IndexProperty = BindableProperty.Create(nameof(Index), typeof(int), typeof(RotationConverter), 0);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      //throw new ApplicationException();
      System.Diagnostics.Debugger.Break();
      System.Diagnostics.Debug.WriteLine(targetType);
      throw null;
      if (targetType == typeof(Rectangle)) //Absolute position is requested
      {
        var theta = Math.PI * 2 / Count * Index;
        var x = -Radius * Math.Sin(theta);
        var y = Radius * Math.Cos(theta);
        return ((Rectangle)value).Offset(x, y);
      } else if (targetType == typeof(float)) //Rotation in degrees is requested
        return 360 / Count * Index;
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
