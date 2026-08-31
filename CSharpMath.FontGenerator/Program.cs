using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CSharpMath.FontGenerator;

/// <summary>Build-time format for the generated bundled-face inputs.</summary>
internal static class FontBlob {
  public const uint Schema = 1;
  private const uint Magic = 0x31464D43; // CMF1, little endian
  private const uint LittleEndianMarker = 0x01020304;

  public static void Write(string source, string destination) {
    var bytes = File.ReadAllBytes(source);
    var hash = SHA256.HashData(bytes);
    var name = Encoding.UTF8.GetBytes(Path.GetFileName(source));
    bytes = BuildRequiredPayload(bytes);
    using var compressed = new MemoryStream();
    using (var brotli = new BrotliStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
      brotli.Write(bytes);
    var payload = compressed.ToArray();
    using var output = File.Create(destination);
    Span<byte> header = stackalloc byte[88];
    BinaryPrimitives.WriteUInt32LittleEndian(header, Magic);
    BinaryPrimitives.WriteUInt32LittleEndian(header[4..], Schema);
    BinaryPrimitives.WriteUInt32LittleEndian(header[8..], LittleEndianMarker);
    hash.CopyTo(header[12..]);
    SHA256.HashData(bytes).CopyTo(header[44..]);
    BinaryPrimitives.WriteUInt32LittleEndian(header[76..], (uint)name.Length);
    BinaryPrimitives.WriteUInt64LittleEndian(header[80..], (ulong)payload.Length);
    output.Write(header);
    output.Write(name);
    output.Write(payload);
  }

  public static byte[] ReadAndValidate(string path, string source) {
    using var input = File.OpenRead(path);
    Span<byte> header = stackalloc byte[88];
    if (input.Read(header) != header.Length) throw new InvalidDataException("Font blob header is truncated.");
    if (BinaryPrimitives.ReadUInt32LittleEndian(header) != Magic) throw new InvalidDataException("Font blob magic mismatch.");
    if (BinaryPrimitives.ReadUInt32LittleEndian(header[4..]) != Schema) throw new InvalidDataException("Font blob schema mismatch.");
    if (BinaryPrimitives.ReadUInt32LittleEndian(header[8..]) != LittleEndianMarker) throw new InvalidDataException("Font blob endianness mismatch.");
    var expectedHash = header[12..44].ToArray();
    var payloadHash = header[44..76].ToArray();
    var nameLength = BinaryPrimitives.ReadUInt32LittleEndian(header[76..]);
    var length = BinaryPrimitives.ReadUInt64LittleEndian(header[80..]);
    if (nameLength > 1024 || length > int.MaxValue) throw new InvalidDataException("Font blob lengths are invalid.");
    var nameBytes = new byte[(int)nameLength];
    try { input.ReadExactly(nameBytes); } catch (EndOfStreamException ex) { throw new InvalidDataException("Font blob name is truncated.", ex); }
    var encodedName = Encoding.UTF8.GetString(nameBytes);
    if (!string.Equals(encodedName, Path.GetFileName(source), StringComparison.Ordinal)) throw new InvalidDataException("Font blob source name mismatch.");
    if (length > (ulong)(input.Length - input.Position)) throw new InvalidDataException("Font blob payload is truncated.");
    var compressed = new byte[(int)length];
    if (input.Read(compressed) != compressed.Length) throw new InvalidDataException("Font blob payload is truncated.");
    byte[] payload;
    try {
      using var compressedStream = new MemoryStream(compressed, writable: false);
      using var brotli = new BrotliStream(compressedStream, CompressionMode.Decompress);
      using var uncompressed = new MemoryStream();
      brotli.CopyTo(uncompressed);
      payload = uncompressed.ToArray();
      if (compressedStream.Position != compressedStream.Length) throw new InvalidDataException("Font blob has trailing compressed data.");
    } catch (Exception ex) when (ex is InvalidDataException or IOException or InvalidOperationException) { throw new InvalidDataException("Font blob compression is invalid.", ex); }
    var sourceHash = SHA256.HashData(File.ReadAllBytes(source));
    if (!CryptographicOperations.FixedTimeEquals(expectedHash, sourceHash)) throw new InvalidDataException("Font source SHA-256 mismatch.");
    if (!CryptographicOperations.FixedTimeEquals(payloadHash, SHA256.HashData(payload))) throw new InvalidDataException("Font blob payload SHA-256 mismatch.");
    if (input.Position != input.Length) throw new InvalidDataException("Font blob has trailing data.");
    return payload;
  }

  // Keep only deterministic SFNT table records used by CSharpMath. GSUB/GPOS are
  // deliberately omitted because the renderer does not perform OpenType shaping.
  internal static byte[] BuildRequiredPayload(byte[] source) {
    if (source.Length < 12) throw new InvalidDataException("Font SFNT header is truncated.");
    var count = (source[4] << 8) | source[5];
    using var output = new MemoryStream();
    output.Write(new byte[] { (byte)'T', (byte)'B', (byte)'L', 1 });
    var kept = new List<(string Tag, int Offset, int Length)>();
    for (var i = 0; i < count; i++) {
      var p = 12 + i * 16;
      if (p + 16 > source.Length) throw new InvalidDataException("Font table directory is truncated.");
      var tag = Encoding.ASCII.GetString(source, p, 4);
      var offset = (int)BinaryPrimitives.ReadUInt32BigEndian(source.AsSpan(p + 8));
      var length = (int)BinaryPrimitives.ReadUInt32BigEndian(source.AsSpan(p + 12));
      if (tag is "GSUB" or "GPOS") continue;
      if (offset < 0 || length < 0 || offset > source.Length - length) throw new InvalidDataException("Font table range is invalid.");
      kept.Add((tag, offset, length));
    }
    Span<byte> number = stackalloc byte[4];
    BinaryPrimitives.WriteUInt32LittleEndian(number, (uint)kept.Count); output.Write(number);
    foreach (var table in kept.OrderBy(t => t.Tag, StringComparer.Ordinal)) {
      output.Write(Encoding.ASCII.GetBytes(table.Tag));
      BinaryPrimitives.WriteUInt32LittleEndian(number, (uint)table.Length); output.Write(number);
      output.Write(source, table.Offset, table.Length);
    }
    return output.ToArray();
  }
}

internal static class Program {
  private static readonly string[] Bundled = { "latinmodern-math.otf", "AMS-Capital-Blackboard-Bold.otf", "cyrillic-modern-nmr10.otf" };

  public static int Main(string[] args) {
    if ((args.Length is < 3 or > 4) || (!string.Equals(args[0], "generate", StringComparison.OrdinalIgnoreCase) && !string.Equals(args[0], "verify", StringComparison.OrdinalIgnoreCase))) {
      Console.Error.WriteLine("Usage: CSharpMath.FontGenerator generate <reference-font-directory> <output-directory> [prototype-directory]");
      return 2;
    }
    var sourceDirectory = Path.GetFullPath(args[1]);
    var outputDirectory = Path.GetFullPath(args[2]);
    if (string.Equals(args[0], "verify", StringComparison.OrdinalIgnoreCase)) return Verify(sourceDirectory, outputDirectory);
    Generate(sourceDirectory, outputDirectory, args.Length == 4 ? Path.GetFullPath(args[3]) : null);
    return 0;
  }

  private static int Verify(string sourceDirectory, string outputDirectory) {
    var temporary = Directory.CreateTempSubdirectory("csmf-verify-");
    try {
      Generate(sourceDirectory, temporary.FullName, null);
      var expected = Directory.GetFiles(temporary.FullName, "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(temporary.FullName, p)).OrderBy(p => p, StringComparer.Ordinal).ToArray();
      var actual = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(outputDirectory, p)).OrderBy(p => p, StringComparer.Ordinal).ToArray();
      if (!expected.SequenceEqual(actual, StringComparer.Ordinal)) throw new InvalidDataException("Generated font artifact set differs from deterministic output.");
      foreach (var relative in expected) {
        var a = File.ReadAllBytes(Path.Combine(outputDirectory, relative));
        var b = File.ReadAllBytes(Path.Combine(temporary.FullName, relative));
        if (!a.AsSpan().SequenceEqual(b)) throw new InvalidDataException("Generated font artifact differs: " + relative);
      }
      Console.WriteLine("CSMF1 artifacts verified against source SHA-256.");
      return 0;
    } finally { temporary.Delete(true); }
  }

  private static void Generate(string sourceDirectory, string outputDirectory, string? prototypeDirectory) {
    Directory.CreateDirectory(outputDirectory);
    var entries = new List<ManifestEntry>();
    foreach (var fileName in Bundled.OrderBy(x => x, StringComparer.Ordinal)) {
      var source = Path.Combine(sourceDirectory, fileName);
      if (!File.Exists(source)) throw new FileNotFoundException("Bundled source font is missing", source);
      var outputName = Path.ChangeExtension(fileName, ".csmfont");
      FontBlob.Write(source, Path.Combine(outputDirectory, outputName));
      var bytes = File.ReadAllBytes(source);
      entries.Add(new ManifestEntry(fileName, outputName, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), bytes.Length, new FileInfo(Path.Combine(outputDirectory, outputName)).Length));
    }
    var manifest = new { schema = FontBlob.Schema, byteOrder = "little", format = "CSMF1", shaping = "GSUB/GPOS intentionally excluded; CSharpMath does not shape text with them.", fonts = entries };
    File.WriteAllText(Path.Combine(outputDirectory, "manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));
    if (prototypeDirectory != null) WritePrototype(outputDirectory, prototypeDirectory, entries);
  }

  private static void WritePrototype(string outputDirectory, string prototypeDirectory, IReadOnlyList<ManifestEntry> entries) {
    Directory.CreateDirectory(prototypeDirectory);
    var sb = new StringBuilder("// Generated by CSharpMath.FontGenerator. Do not edit.\nnamespace CSharpMath.Generated;\n\ninternal static class BundledFontPrototype\n{\n");
    foreach (var entry in entries) {
      var bytes = File.ReadAllBytes(Path.Combine(outputDirectory, entry.Output));
      sb.Append("    internal static readonly byte[] ").Append(Path.GetFileNameWithoutExtension(entry.Output).Replace('-', '_')).Append(" = new byte[] { ");
      sb.Append(string.Join(", ", bytes.Select(b => b.ToString()))).AppendLine(" };\n");
    }
    sb.AppendLine("}");
    File.WriteAllText(Path.Combine(prototypeDirectory, "BundledFontPrototype.g.cs"), sb.ToString(), new UTF8Encoding(false));
  }

  private sealed record ManifestEntry(string Source, string Output, string SourceSha256, int SourceBytes, long GeneratedBytes);
}
