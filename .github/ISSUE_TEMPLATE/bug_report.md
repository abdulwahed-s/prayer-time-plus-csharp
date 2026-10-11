---
name: Bug report
about: Report a defect or an incorrect prayer time
title: "[Bug]: "
labels: bug
assignees: ''
---

**Describe the bug**

A clear and concise description of what is wrong.

**Inputs**

- Coordinates (latitude/longitude in degrees, altitude in metres):
- Civil date (year-month-day):
- Calculation method or stable method key:
- Madhab:
- High-latitude rule:
- UTC offset (`TimeSpan`, including DST for that date):
- Country code (for Auto resolution or country-specific rules):
- City name, if supplied:
- `IsRamadan`:
- Custom angles, interval flags/values, and method/user minute adjustments:
- Query instant, including offset (for current/next-prayer reports):

**Expected times**

What you expected, and the source or authority you are comparing against.

**Actual times**

What the library returned. Include local dates, times, offsets, and any `null`
values. Include UTC instants when reporting offset or helper behavior.

**Minimal reproduction**

<!-- A small C# snippet that reproduces the issue is ideal. Include all custom parameters. -->

```csharp
```

**Environment**

- NuGet package/version: <!-- e.g. PrayerTimePlus 0.3.0, or source commit for a local build -->
- .NET SDK (`dotnet --version`):
- Installed runtimes (`dotnet --list-runtimes`):
- Consumer target framework: <!-- e.g. net8.0 / net10.0 -->
- OS: <!-- e.g. Windows / Linux / macOS -->

**Additional context**

Anything else that might help.
