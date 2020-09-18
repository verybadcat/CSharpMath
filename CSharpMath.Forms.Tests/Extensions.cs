namespace Xunit {
  public static partial class Extensions {
    public static T NotNull<T>(this T? obj) where T : class {
      if (obj == null)
        throw new Sdk.NotNullException();
      return obj;
    }
  }
}