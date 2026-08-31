using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CSharpMath.FontGenerator;
using Xunit;

namespace CSharpMath.FontGenerator.Tests;

public sealed class RendererRecordTests {
  private const int DirectoryOffset = RendererRecordCodec.HeaderSize;
  [Fact]
  public void SyntheticRecordRoundTripsByteForByteAndIsOrdered() {
    using var source = new TempFile(new byte[] { 1, 2, 3 });
    var hash = SHA256.HashData(File.ReadAllBytes(source.Path));
    var face = new RendererFace("synthetic.otf", hash, new[] {
      new RendererSection("glyf", 1, new byte[] { 3, 2, 1 }),
      new RendererSection("head", 1, new byte[] { 9 })
    });
    var bytes = RendererRecordCodec.Serialize(face);
    using var blob = new TempFile(bytes);
    var read = RendererRecordCodec.ReadAndValidate(blob.Path, source.Path);
    Assert.Equal(bytes, RendererRecordCodec.Serialize(read));
    Assert.Equal(new[] { "glyf", "head" }, read.Sections.Select(x => x.Tag));
  }

  [Fact]
  public void BundledFacesHaveStableLogicalRoundTrips() {
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
    var dir = Path.Combine(root, "CSharpMath.Rendering", "Reference Fonts");
    foreach (var name in new[] { "latinmodern-math", "AMS-Capital-Blackboard-Bold", "cyrillic-modern-nmr10" }) {
      var source = Path.Combine(dir, name + ".otf");
      var first = RendererRecordCodec.Serialize(RendererRecordCodec.Build(source));
      using var blob = new TempFile(first);
      Assert.Equal(first, RendererRecordCodec.Serialize(RendererRecordCodec.ReadAndValidate(blob.Path, source)));
    }
  }

  [Fact]
  public void RejectsHeaderBoundsOverlapTruncationSourceDriftAndTrailingBytes() {
    using var source = new TempFile(new byte[] { 4, 5, 6 });
    var face = new RendererFace("synthetic.otf", SHA256.HashData(File.ReadAllBytes(source.Path)), new[] { new RendererSection("head", 1, new byte[] { 1, 2 }) });
    var original = RendererRecordCodec.Serialize(face);
    foreach (var mutate in new Action<byte[]>[] {
      x => x[0] ^= 1, x => x[4] ^= 1, x => x[8] ^= 1, x => x[20] = 0, x => x[36] ^= 1,
      x => x[DirectoryOffset + 4] = 0, x => x[DirectoryOffset + 8] = 0, x => x[DirectoryOffset + 16] = 1, x => x[^1] ^= 1
    }) {
      var bytes = (byte[])original.Clone(); mutate(bytes); using var blob = new TempFile(bytes);
      Assert.Throws<InvalidDataException>(() => RendererRecordCodec.ReadAndValidate(blob.Path, source.Path));
    }
    using var trailing = new TempFile(original.Concat(new byte[] { 0 }).ToArray());
    Assert.Throws<InvalidDataException>(() => RendererRecordCodec.ReadAndValidate(trailing.Path, source.Path));
    File.WriteAllBytes(source.Path, new byte[] { 7 });
    using var drift = new TempFile(original);
    Assert.Throws<InvalidDataException>(() => RendererRecordCodec.ReadAndValidate(drift.Path, source.Path));
  }

  [Fact]
  public void BundledFacesHaveDistinctSourceIdentity() {
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../")); var dir = Path.Combine(root, "CSharpMath.Rendering", "Reference Fonts");
    var hashes = new[] { "latinmodern-math", "AMS-Capital-Blackboard-Bold", "cyrillic-modern-nmr10" }.Select(x => RendererRecordCodec.Build(Path.Combine(dir, x + ".otf")).SourceHash).Select(Convert.ToHexString).ToArray();
    Assert.Equal(3, hashes.Distinct(StringComparer.Ordinal).Count());
  }

  [Fact]
  public void BundledRecordsContainIdentityOnly() {
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../")); var dir = Path.Combine(root, "CSharpMath.Rendering", "Reference Fonts");
    foreach (var name in new[] { "latinmodern-math", "AMS-Capital-Blackboard-Bold", "cyrillic-modern-nmr10" }) {
      var face = RendererRecordCodec.Build(Path.Combine(dir, name + ".otf"));
      Assert.Single(face.Sections); Assert.Equal("NAME", face.Sections[0].Tag); Assert.Equal(name + ".otf", System.Text.Encoding.UTF8.GetString(face.Sections[0].Data));
    }
  }

  private sealed class TempFile : IDisposable {
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "csmr-" + Guid.NewGuid().ToString("N"));
    public TempFile(byte[] bytes) => File.WriteAllBytes(Path, bytes);
    public void Dispose() { if (File.Exists(Path)) File.Delete(Path); }
  }
}
