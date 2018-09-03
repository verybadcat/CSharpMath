using System;
using System.IO;
using System.Text;

namespace CSharpMath.DevUtils.Rendering {
  static class FontReferenceCodeBuilder {
    const int numbersPerLine = 25;
    static bool first = true;
    /// <summary>
    /// Builds all the code files inside CSharpMath\CSharpMath.SkiaSharp\Font Reference from the font files.
    /// </summary>
    public static void Build() {
      Build("latinmodern-math", "LatinModernMath");
      Build("AMS-Capital-Blackboard-Bold", "AMSCapitalBlackboardBold");
    }
    private static void Build(string otf, string cs) {
      var bytes = File.ReadAllBytes(Path.Combine(Paths.FontReferenceFolder, otf + "." + nameof(otf)));
      var b = new StringBuilder()
        .AppendLine($"namespace CSharpMath.Rendering {{")
        .AppendLine($"  //Do not modify this file directly. Instead, modify this at")
        .AppendLine($"  //CSharpMath\\CSharpMath.Utils\\Rendering\\{nameof(FontReferenceCodeBuilder)}.cs and re-generate")
        .AppendLine($"  //this file by executing the method in that file in the CSharpMath.Utils project.");
      if(first) b.AppendLine($"  [System.Diagnostics.DebuggerNonUserCode, System.Runtime.CompilerServices.CompilerGenerated]");
      first = false;
      b
        .AppendLine($"  internal static partial class Resources {{")
        .AppendLine($"    public static byte[] {cs} {{ get; }} = new byte[] {{");
      int i = 0;
      for (int l = bytes.Length - numbersPerLine; i < l; i += numbersPerLine)
        b.Append("      ").AppendJoin(", ", new ArraySegment<byte>(bytes, i, numbersPerLine)).AppendLine(", ");
      b
        .Append("      ").AppendJoin(", ", new ArraySegment<byte>(bytes, i, bytes.Length - i)).AppendLine()
        .AppendLine("    };")
        .AppendLine("  }")
        .AppendLine("}");
      File.WriteAllText(Path.Combine(Paths.FontReferenceFolder, cs + "." + nameof(cs)), b.ToString());
    }
  }
}