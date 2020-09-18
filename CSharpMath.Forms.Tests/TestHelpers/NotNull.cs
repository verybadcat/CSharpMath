namespace CSharpMath.Forms.Tests {
  public static class Extensions {
    public static T NotNull<T>(this T? obj) where T : class {
      if (obj == null)
        throw new Xunit.Sdk.NotNullException();
      return obj;
    }
  }
}