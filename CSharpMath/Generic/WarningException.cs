namespace CSharpMath {
  public class WarningException : System.Exception {
    WarningException(string message) : base(message) =>
      System.Diagnostics.Debugger.Break();

    public static bool DisableWarnings { get; set; }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void WarnIf(bool condition, string message) {
      if (condition && !DisableWarnings) throw new WarningException(message);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void WarnIfAny<T>(System.Collections.Generic.IEnumerable<T> ie,
      System.Func<T, bool> condition, string message) {
      if (!DisableWarnings)
        foreach (var item in ie)
          if (condition(item)) throw new WarningException(message);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void WarnIfAll<T>(System.Collections.Generic.IEnumerable<T> ie,
      System.Func<T, bool> condition, string message) {
      if (DisableWarnings) return;
      foreach (var item in ie)
        if (!condition(item)) return;
      throw new WarningException(message);
    }
  }
}
