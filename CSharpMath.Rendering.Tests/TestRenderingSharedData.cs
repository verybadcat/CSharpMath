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
  public abstract class TestRenderingSharedData<TThis> : IEnumerable<object[]> where TThis : TestRenderingSharedData<TThis> {
    public static IReadOnlyDictionary<string, string> AllConstants { get; } =
      typeof(TestRenderingSharedData<TThis>)
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
    public const string Color = @"\color{#008}a\color{#00F}b\color{#080}c\color{#088}d\color{#08F}e\color{#0F0}f\color{#0F8}g\color{#0FF}h\color{#800}i\color{#808}j\color{#80F}k\color{#880}l\color{#888}m\color{#88F}n\color{#8F0}o\color{#8F8}p\color{#8FF}q\color{#F00}r\color{#F08}s\color{#F0F}t\color{#F80}u\color{#F88}v\color{#F8F}w\color{#FF0}x\color{#FF8}y\color{#FFF}z";
    public const string ErrorInvalidCommand = @"\color{#008}a\color{#00F}b\color{#080}c\color{#088}d\color{#08F}e\color{#0F0}f\color{#0F8}g\color{#0FF}h\color{#800}i\color{#808}j\color{#80F}k\color{#880}l\color{#888}m\color{#88F}n\color{#8F0}o\color{#8F8}p\color{#8FF}q\color{#F00}r\color{#F08}s\color{#F0F}t\color{#F80}u\color{#F88}v\color{#F8F}w\color{#FF0}x\color{#FF8}y\color{#FFF}\notacommand";
    public const string ErrorInvalidColor = @"\color{#008}a\color{#00F}b\color{#080}c\color{#088}d\color{#08F}e\color{#0F0}f\color{0F8}g\color{#0FF}h\color{#800}i\color{#808}j\color{#80F}k\color{#880}l\color{#888}m\color{#88F}n\color{#8F0}o\color{#8F8}p\color{#8FF}q\color{#F00}r\color{#F08}s\color{#F0F}t\color{#F80}u\color{#F88}v\color{#F8F}w\color{#FF0}x\color{#FF8}y\color{#FFF}z";
    public const string ErrorMissingBrace = @"}z";
  }
}
