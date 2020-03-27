using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//For the Android linker
namespace Android.Runtime {
  public sealed class PreserveAttribute : System.Attribute {
    public bool AllMembers; public bool Conditional;
  }
}
#if !__IOS__
//For the iOS linker
namespace Foundation {
  public sealed class PreserveAttribute : System.Attribute {
    public bool AllMembers; public bool Conditional;
  }
}
#endif
namespace CSharpMath.Rendering.Tests {
  [Android.Runtime.Preserve(AllMembers = true), Foundation.Preserve(AllMembers = true)]
  public abstract class SharedData<TThis> : IEnumerable<object[]> where TThis : SharedData<TThis> {
    public static IReadOnlyDictionary<string, string> AllConstants { get; } =
      typeof(SharedData<TThis>)
      .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
      .Concat(typeof(TThis)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
      .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
      .ToDictionary(fi => fi.Name, fi => fi.GetRawConstantValue() as string
        ?? throw new Structures.InvalidCodePathException("All constants must be strings!"));
    public IEnumerator<object[]> GetEnumerator() =>
      AllConstants.Select(tuple => new[] { tuple.Key, tuple.Value }).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public const string Numbers = @"1234567890";
    public const string Alphabets = @"abcdefghijklmnopqrstuvwxyz";
    public const string Capitals = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Greeks = @"\alpha \beta \gamma \delta \epsilon \varepsilon \zeta \eta \theta \iota \kappa \lambda " +
                                 @"\mu \nu \xi \omicron \pi \rho \sigma \varsigma \tau \upsilon \varphi \chi \psi \omega";
    public const string CapitalGreeks = @"ΑΒ\Gamma \Delta ΕΖΗ\Theta ΙΚ\Lambda ΜΝ\Xi Ο\Pi Ρ\Sigma Τ\Upsilon \Phi Χ\Psi \Omega ";
    public const string Cyrillic = @"А а\ Б б\ В в\ Г г\ Д д\ Е е\ Ё ё\ Ж ж\\ З з\ И и\ Й й\ К к\ Л л\ М м\ Н н\ О о\ П п" +
                                   @"\\ Р р\ С с\ Т т\ У у\ Ф ф\ Х х\ Ц ц\ Ч ч\\ Ш ш\ Щ щ\ Ъ ъ\ Ы ы\ Ь ь\ Э э\ Ю ю\ Я я";
  }
}
