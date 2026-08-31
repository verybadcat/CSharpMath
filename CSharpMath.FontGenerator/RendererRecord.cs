using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace CSharpMath.FontGenerator;

internal sealed record RendererFace(string Name, byte[] SourceHash, IReadOnlyList<RendererSection> Sections);
internal sealed record RendererSection(string Tag, uint Count, byte[] Data);

internal static class RendererRecordCodec {
  internal const uint Schema = 1, Magic = 0x31524D43, LittleEndianMarker = 0x01020304;
  internal const int HeaderSize = 96, DirectoryEntrySize = 24;
  internal const int MaxSections = 4096, MaxPayload = 128 * 1024 * 1024, MaxSource = 16 * 1024 * 1024;
  private static bool NonZero(ReadOnlySpan<byte> bytes) { foreach (var b in bytes) if (b != 0) return true; return false; }

  private static byte[] ReadBounded(string path, int max) {
    var length = new FileInfo(path).Length;
    if (length > max || length > int.MaxValue) throw new InvalidDataException("Input exceeds format limit.");
    var bytes = new byte[(int)length]; using var stream = File.OpenRead(path); stream.ReadExactly(bytes); return bytes;
  }
  internal static RendererFace Build(string source) => Build(Path.GetFileName(source), ReadBounded(source, MaxSource));
  internal static RendererFace Build(string name, byte[] source) {
    if (Encoding.UTF8.GetByteCount(name) > 1024) throw new InvalidDataException("Face name exceeds limit.");
    return new RendererFace(name, SHA256.HashData(source), new[] { new RendererSection("NAME", 1, Encoding.UTF8.GetBytes(name)) });
  }
  internal static byte[] Serialize(RendererFace face) {
    if (face.SourceHash.Length != 32 || face.Sections.Count > MaxSections || face.Sections.GroupBy(x => x.Tag, StringComparer.Ordinal).Any(x => x.Count() != 1)) throw new InvalidDataException("Invalid face metadata.");
    var sections = face.Sections.OrderBy(x => x.Tag, StringComparer.Ordinal).ToArray(); if (sections.Any(x => x.Tag.Length != 4 || x.Count == 0 || x.Data.Length > MaxPayload)) throw new InvalidDataException("Invalid section metadata.");
    var dataStart = checked(HeaderSize + sections.Length * DirectoryEntrySize); var total = checked(dataStart + sections.Sum(x => x.Data.Length)); if (total > MaxPayload) throw new InvalidDataException("Record exceeds limit.");
    var bytes = new byte[total]; var h = bytes.AsSpan(0, HeaderSize); BinaryPrimitives.WriteUInt32LittleEndian(h, Magic); BinaryPrimitives.WriteUInt32LittleEndian(h[4..], Schema); BinaryPrimitives.WriteUInt32LittleEndian(h[8..], LittleEndianMarker); BinaryPrimitives.WriteUInt32LittleEndian(h[12..], HeaderSize); BinaryPrimitives.WriteUInt32LittleEndian(h[16..], (uint)sections.Length); BinaryPrimitives.WriteUInt32LittleEndian(h[20..], HeaderSize); BinaryPrimitives.WriteUInt32LittleEndian(h[24..], (uint)(sections.Length * DirectoryEntrySize)); face.SourceHash.CopyTo(h[28..]);
    var offset = dataStart; for (var i = 0; i < sections.Length; i++) { var e = bytes.AsSpan(HeaderSize + i * DirectoryEntrySize, DirectoryEntrySize); Encoding.ASCII.GetBytes(sections[i].Tag).CopyTo(e); BinaryPrimitives.WriteUInt32LittleEndian(e[4..], (uint)offset); BinaryPrimitives.WriteUInt32LittleEndian(e[8..], (uint)sections[i].Data.Length); BinaryPrimitives.WriteUInt32LittleEndian(e[12..], sections[i].Count); sections[i].Data.CopyTo(bytes.AsSpan(offset)); offset += sections[i].Data.Length; }
    SHA256.HashData(bytes.AsSpan(dataStart)).CopyTo(bytes.AsSpan(60, 32)); return bytes;
  }
  internal static void Write(string source, string destination) => File.WriteAllBytes(destination, Serialize(Build(source)));
  internal static RendererFace ReadAndValidate(string path, string source) {
    var bytes = ReadBounded(path, MaxPayload); if (bytes.Length < HeaderSize) throw new InvalidDataException("Record header is truncated."); var h = bytes.AsSpan();
    if (BinaryPrimitives.ReadUInt32LittleEndian(h) != Magic || BinaryPrimitives.ReadUInt32LittleEndian(h[4..]) != Schema || BinaryPrimitives.ReadUInt32LittleEndian(h[8..]) != LittleEndianMarker || BinaryPrimitives.ReadUInt32LittleEndian(h[12..]) != HeaderSize) throw new InvalidDataException("Record header is invalid.");
    if (NonZero(h[92..96])) throw new InvalidDataException("Header reserved bytes are nonzero.");
    var n = BinaryPrimitives.ReadUInt32LittleEndian(h[16..]); var dir = BinaryPrimitives.ReadUInt32LittleEndian(h[20..]); var dl = BinaryPrimitives.ReadUInt32LittleEndian(h[24..]); if (n > MaxSections || dir != HeaderSize || dl != n * DirectoryEntrySize || dl > bytes.Length - dir) throw new InvalidDataException("Directory bounds are invalid."); if (!CryptographicOperations.FixedTimeEquals(h[28..60], SHA256.HashData(ReadBounded(source, MaxSource)))) throw new InvalidDataException("Source hash mismatch.");
    var sections = new List<RendererSection>(); var previous = ""; var end = checked((int)(dir + dl)); for (var i = 0; i < n; i++) { var e = bytes.AsSpan((int)dir + i * DirectoryEntrySize, DirectoryEntrySize); if (NonZero(e[16..24])) throw new InvalidDataException("Directory reserved bytes are nonzero."); var tag = Encoding.ASCII.GetString(e[..4]); var offset = BinaryPrimitives.ReadUInt32LittleEndian(e[4..]); var length = BinaryPrimitives.ReadUInt32LittleEndian(e[8..]); var count = BinaryPrimitives.ReadUInt32LittleEndian(e[12..]); if (string.CompareOrdinal(previous, tag) >= 0 || count == 0 || offset < end || offset > bytes.Length || length > bytes.Length - offset) throw new InvalidDataException("Section bounds or ordering are invalid."); sections.Add(new RendererSection(tag, count, bytes.AsSpan((int)offset, (int)length).ToArray())); previous = tag; end = checked((int)(offset + length)); }
    if (end != bytes.Length || !CryptographicOperations.FixedTimeEquals(h[60..92], SHA256.HashData(bytes.AsSpan((int)(dir + dl))))) throw new InvalidDataException("Record payload is invalid."); return new RendererFace(Path.GetFileName(source), h[28..60].ToArray(), sections);
  }
}
