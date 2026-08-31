# CSharpMath bundled-font generator

This build-only tool turns the three checked-in reference OTFs into deterministic
`CSMF1` blobs and a stable `manifest.json`. The blob records magic, schema,
endianness, source SHA-256, source name, and payload length. `ReadAndValidate`
rejects a changed source, tampered payload, unknown schema, wrong byte order, or
truncated input before any data is consumed.

The optional third argument emits a C# byte-array representation. It is a
prototype used for the #191 size/startup/allocation/AOT comparison; production
uses the binary representation. This envelope intentionally does not serialize
GSUB or GPOS: CSharpMath's typesetter does not perform OpenType shaping.

Current checked-in size evidence (2026-08-31): raw OTF total 786,744 bytes;
CSMF1/TBL1 total 415,661 bytes (47.2% smaller). The optional C# prototype is
generated in a temporary directory for measurement only (1,900,468 bytes
total), and is not checked in. Fixed-width records and no reflection or
dynamic serialization keep the binary representation suitable for
AOT/trimming. This artifact phase is **not runtime-consumed yet** and changes
no startup behavior. The #293 cold-process methodology (fresh process,
first-render timing, allocated/retained managed bytes) carries to #321 for the
runtime decoder comparison.

Example:

```text
dotnet run --project CSharpMath.FontGenerator -- generate "CSharpMath.Rendering/Reference Fonts" generated generated-csharp
```
