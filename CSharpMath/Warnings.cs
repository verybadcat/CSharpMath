namespace CSharpMath {
  using System.Diagnostics;
  public static class Warnings {
    public static bool DisableWarnings { get; set; }
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message) =>
      Debug.Assert(!DisableWarnings && condition, message);
    [Conditional("DEBUG")]
    public static void AssertAll<T>(System.Collections.Generic.IEnumerable<T> ie,
      System.Func<T, bool> condition, string message) {
      if (!DisableWarnings) foreach (var item in ie) Debug.Assert(condition(item), message);
    }
  }
}
