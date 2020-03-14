using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSharpMath.SkiaSharp {
  public sealed class SharedData : SharedData<SharedData> { }
  [Android.Runtime.Preserve(AllMembers = true),
   Foundation.Preserve(AllMembers = true)]
  public class SharedData<TThis> : IEnumerable<object[]> where TThis : SharedData<TThis> {
    public static IReadOnlyDictionary<string, string> AllConstants { get; } =
      typeof(TThis)
      .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
      .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
      .ToDictionary(fi => fi.Name, fi => (string)fi.GetRawConstantValue());
    public IEnumerator<object[]> GetEnumerator() =>
      AllConstants.Select(tuple => new[] { tuple.Key, tuple.Value }).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public const string Numbers = @"1234567890";
    public const string Alphabets = @"abcdefghijklmnopqrstuvwxyz";
    public const string Capitals = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Greeks = @"\alpha \beta \gamma \delta \varepsilon \zeta \eta \theta \iota \kappa \lambda " +
                                    @"\mu \nu \xi \omicron \pi \rho \sigma \varsigma \tau \upsilon \varphi \chi \omega ";
    public const string CapitalGreeks = @"ΑΒ\Gamma \Delta ΕΖΗ\Theta ΙΚ\Lambda ΜΝ\Xi Ο\Pi Ρ\Sigma Τ\Upsilon \Phi Χ\Omega ";
  }
}
