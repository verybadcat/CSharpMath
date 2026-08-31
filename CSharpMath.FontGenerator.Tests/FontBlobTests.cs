using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CSharpMath.FontGenerator;
using Xunit;

namespace CSharpMath.FontGenerator.Tests;

public sealed class FontBlobTests {
  private static readonly string Root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
  private static readonly string Sources = Path.Combine(Root, "CSharpMath.Rendering", "Reference Fonts");
  private static readonly string Generated = Path.Combine(Root, "CSharpMath.Rendering", "Generated Fonts");
  private static readonly string[] Faces = { "latinmodern-math", "AMS-Capital-Blackboard-Bold", "cyrillic-modern-nmr10" };

  [Fact]
  public void AllFacesVerifyAndRepeatedGenerationIsStable() {
    foreach (var face in Faces) {
      var source = Path.Combine(Sources, face + ".otf");
      var blob = Path.Combine(Generated, face + ".csmfont");
      Assert.NotEmpty(FontBlob.ReadAndValidate(blob, source));
      var first = File.ReadAllBytes(blob);
      using var temp = new TempDirectory();
      FontBlob.Write(source, Path.Combine(temp.Path, "font.csmfont"));
      Assert.Equal(first, File.ReadAllBytes(Path.Combine(temp.Path, "font.csmfont")));
    }
  }

  [Fact]
  public void HeaderFailuresAreRejected() {
    var source = Path.Combine(Sources, "latinmodern-math.otf");
    var original = File.ReadAllBytes(Path.Combine(Generated, "latinmodern-math.csmfont"));
    foreach (var (offset, value) in new[] { (0, (byte)0), (4, (byte)0), (8, (byte)0), (120, (byte)0) }) {
      var tampered = (byte[])original.Clone(); tampered[offset] ^= 0xFF;
      using var temp = new TempDirectory(); var path = Path.Combine(temp.Path, "bad.csmfont"); File.WriteAllBytes(path, tampered);
      Assert.Throws<InvalidDataException>(() => FontBlob.ReadAndValidate(path, source));
    }
    var wrongSource = Path.Combine(Sources, "cyrillic-modern-nmr10.otf");
    using var mismatch = new TempDirectory(); var mismatchPath = Path.Combine(mismatch.Path, "font.csmfont"); File.WriteAllBytes(mismatchPath, original);
    Assert.Throws<InvalidDataException>(() => FontBlob.ReadAndValidate(mismatchPath, wrongSource));
  }

  [Fact]
  public void TableContainerExcludesShapingAndPreservesEveryOtherTable() {
    foreach (var face in Faces) {
      var sourceBytes = File.ReadAllBytes(Path.Combine(Sources, face + ".otf"));
      var payload = FontBlob.BuildRequiredPayload(sourceBytes);
      Assert.Equal(new byte[] { (byte)'T', (byte)'B', (byte)'L', 1 }, payload[..4]);
      var tables = ReadTables(payload);
      var tags = tables.Keys.ToArray();
      Assert.Equal(tags.OrderBy(x => x, StringComparer.Ordinal), tags);
      var count = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(4, 4)); var p = 8;
      for (var i = 0; i < count; i++) { p += 4; var len = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(p, 4)); p += 4 + checked((int)len); }
      Assert.DoesNotContain("GSUB", tags); Assert.DoesNotContain("GPOS", tags);
      foreach (var (tag, bytes) in SourceTables(sourceBytes)) if (tag is not ("GSUB" or "GPOS")) Assert.Equal(bytes, tables[tag]);
    }
  }

  [Fact]
  public void ManifestFaceOrderIsStableAndVerifyRejectsTampering() {
    using var generated = new TempDirectory();
    Assert.Equal(0, Program.Main(new[] { "generate", Sources, generated.Path }));
    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(generated.Path, "manifest.json")));
    var names = manifest.RootElement.GetProperty("fonts").EnumerateArray().Select(x => x.GetProperty("Source").GetString()).ToArray();
    Assert.Equal(new[] { "AMS-Capital-Blackboard-Bold.otf", "cyrillic-modern-nmr10.otf", "latinmodern-math.otf" }, names);
    foreach (var file in Directory.GetFiles(Generated)) File.Copy(file, Path.Combine(generated.Path, Path.GetFileName(file)), true);
    var tampered = Path.Combine(generated.Path, "latinmodern-math.csmfont");
    var bytes = File.ReadAllBytes(tampered); bytes[^1] ^= 0x80; File.WriteAllBytes(tampered, bytes);
    Assert.Throws<InvalidDataException>(() => Program.Main(new[] { "verify", Sources, generated.Path }));
  }

  private static Dictionary<string, byte[]> ReadTables(byte[] payload) { var result = new Dictionary<string, byte[]>(StringComparer.Ordinal); var n = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(4, 4)); var p = 8; for (var i = 0; i < n; i++) { var tag = System.Text.Encoding.ASCII.GetString(payload, p, 4); p += 4; var len = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(p, 4))); p += 4; result.Add(tag, payload.AsSpan(p, len).ToArray()); p += len; } return result; }
  private static IEnumerable<(string Tag, byte[] Bytes)> SourceTables(byte[] bytes) { var n = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(4)); for (var i = 0; i < n; i++) { var p = 12 + i * 16; var tag = System.Text.Encoding.ASCII.GetString(bytes, p, 4); var offset = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(p + 8))); var len = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(p + 12))); yield return (tag, bytes.AsSpan(offset, len).ToArray()); } }
  private sealed class TempDirectory : IDisposable { public string Path { get; } = System.IO.Directory.CreateTempSubdirectory("csmf-").FullName; public void Dispose() => Directory.Delete(Path, true); }
}
