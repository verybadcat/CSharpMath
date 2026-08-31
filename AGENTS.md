# Coding-agent guidance

Follow [Contributing.md](Contributing.md) and [SECURITY.md](SECURITY.md), including
their review policy. Keep pull-request titles concise and descriptions simple. Run
focused validation relevant to the change, inspect the diff and repository for
accidentally exposed secrets without reproducing suspected secrets, and disclose the
actual automated or AI review used and its findings and fixes. Do not assume GitHub
Copilot is available.

Changes to CSharpMath core/editor, CSharpMath.Rendering, CSharpMath.SkiaSharp, their
corresponding tests, or cross-cutting build, release, review, or security
infrastructure require human review. Humans also decide significant feature-fit and
maintainability questions and should check obvious and security mistakes.

Periphery changes (Evaluation/symbolic algebra, other platform frontends or adapters,
examples, benchmarks, and isolated tests) are ready without human GitHub review only
when relevant CI is green, a satisfactory independent LLM review has no unresolved
findings, and no core, public-API, security, release, or cross-cutting concern is
involved. Never claim that any model can never leak secrets; use safe handling and
escalation instead.
