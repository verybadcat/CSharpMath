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

The `CMR1` renderer-record schema is a separate versioned envelope. Its fixed
little-endian header is followed by a sorted directory of fixed-width records;
each record contains an explicit offset, byte length, and logical count. Readers
reject unknown schema/byte order, out-of-range or overlapping sections,
truncation, source-hash drift, and trailing bytes. Schema versions are not
silently upgraded: an incompatible layout requires a new schema number.
Binary and generated-C# prototypes are serialized from the same immutable
logical model, with ordinal face and section ordering. Counts and lengths are
capped before allocation. The codec uses only BCL APIs and fixed records, with
no reflection or dynamic code, so it is AOT/trimming-safe; applications using
linker trimming should preserve the codec entry points.

Generation keeps legacy `.csmfont` artifacts and can emit `.csmrecord` CMR1
artifacts plus `BundledFontPrototype.g.cs`; these are build/measurement inputs,
never runtime inputs. `verify` accepts an optional prototype directory and
compares its complete immutable file set with fresh temporary regeneration.
Source inputs are capped at 16 MiB, records at 128 MiB, and directories at
4096 sections before allocation; offset arithmetic is checked.
The current CMR1 logical model intentionally contains only the face identity
(`NAME`) and source hash; font tables and rendering payloads are deferred to
later schema work.

Example:

```text
dotnet run --project CSharpMath.FontGenerator -- generate "CSharpMath.Rendering/Reference Fonts" generated generated-csharp
```
