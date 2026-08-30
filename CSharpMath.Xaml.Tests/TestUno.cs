using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;
using Xunit;

namespace CSharpMath.Xaml.Tests {
  public class TestUno {
    [Fact]
    public void BindingExpressionMarkerPreventsCounterpartAssignment() {
      var binding = RuntimeHelpers.GetUninitializedObject(typeof(BindingExpression));
      Assert.False(Uno.UnoPropertySynchronization.ShouldAssignCounterpart(binding));
      Assert.True(Uno.UnoPropertySynchronization.ShouldAssignCounterpart(null));
      Assert.True(Uno.UnoPropertySynchronization.ShouldAssignCounterpart(new object()));
    }
  }
}
