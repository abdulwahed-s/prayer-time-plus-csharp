# Focused sibling conformance capture

Normal .NET builds and CI read committed fixtures and need no sibling checkout,
Dart, Java, Kotlin, Python or network capture. This optional development recipe
rebuilds `focused-sibling-goldens.json` from independent engines. It preserves the
original 1,232 prayer-time fixtures.

Prerequisites: Python 3.10+, Dart with `pub get` completed in its checkout, a Java
JDK, and Kotlin with `:lib:jar` built at the desired source revision. Supply the
resulting library JAR and its matching Kotlin standard library JAR explicitly.
Sibling tracked files must be clean. A JAR SHA256 and both Git revisions are
recorded; build the JAR from that clean Kotlin revision before capture.

From the C# root, for example:

```powershell
python tools/conformance/capture.py `
  --dart-root ../prayer_time_plus_dart `
  --kotlin-root ../prayer-time-plus-kotlin `
  --kotlin-jar ../prayer-time-plus-kotlin/lib/build/libs/lib-0.3.0.jar `
  --kotlin-stdlib /path/to/kotlin-stdlib-2.3.21.jar
```

Append `--check` to recapture and compare without replacing the fixture.
Captures have deterministic ordering and no wall-clock metadata. Temporary
adapter outputs are under ignored `artifacts/` and removed when capture finishes.
Review any changed independent-engine results before committing replacements.

`cases.json` contains focused civil dates, coordinates, whole-minute offsets,
parameter overrides, method/user tuning and country/city context. Missing flags
in direct construction exercise the siblings' actual defaults. The Java adapter
calls Kotlin's generated default constructor using reflection, leaving only its
required Fajr/Isha angles explicit; it does not substitute the C# flag default.
The Dart adapter uses the direct constructor normally.

Both adapters record seven local date/time values, absolute UTC instants, two
Sunnah values and helper answers at each whole-hour instant. Dart's UTC-flagged
wall-clock values are normalized by subtracting its supplied offset; Kotlin's
OffsetDateTime values use their actual offset.

The selected C# helper contract is chronological instants, with the later prayer
name winning current ties and the earlier name winning next ties. The capture
tool derives those expected answers from independent captured timestamps.
Raw sibling helper answers are retained as compatibility evidence, rather than
used as C# expectations for unordered schedules.

Kotlin captures supply the expected prayer/Sunnah values. The tool rejects
unexpected Dart differences. Two intentional exceptions are explicit per case:
Dart's application city tweaks, and its omission of country/city context when
recomputing tomorrow's Sunnah boundary. C# preserves the Kotlin behavior. This
capture does not modify sibling code or require complete helper API equivalence.
