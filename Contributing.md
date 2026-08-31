Usability, platform support and transparency are the core values of CSharpMath.

Usability
---------
New users should be able to figure out the API without looking at the Wiki.
Following good design patterns is one way to achieve this.

Platform support
----------------
As a contributor, you should have knowledge of news around the .NET community
and suggest new platforms with future potentials for CSharpMath to support.
Never get slowed down by dying platforms (e.g. Silverlight).

Transparency
------------
New issues and pull requests should be responded in a short time.
Keep issues for reproducible bugs and concrete, actionable work. Use Discussions for
questions, ideas, and ongoing conversations that do not yet have an implementation
scope.

Use labels to classify the type, area, and impact of work, rather than to duplicate
workflow state. Use draft pull requests for work in progress, milestones for release
commitments, and native sub-issues and dependencies for larger efforts. When closing
an issue without a merged change, choose the appropriate GitHub state reason so
future readers can distinguish completed work from work that is not planned.

Review policy
-------------
Core changes are changes to CSharpMath core/editor, CSharpMath.Rendering,
CSharpMath.SkiaSharp, their corresponding tests, or cross-cutting build, release,
review, or security infrastructure. Core changes require human review.

Periphery includes symbolic algebra/Evaluation, other platform frontends and adapters,
examples, benchmarks, and isolated tests for those areas. A periphery change may
proceed without human GitHub review only when relevant CI is green, a satisfactory
independent LLM review has no unresolved findings, and it does not involve core code,
a public API, security, release, or another cross-cutting concern. Humans decide
significant feature-fit and maintainability questions and remain important for obvious
or security mistakes.
