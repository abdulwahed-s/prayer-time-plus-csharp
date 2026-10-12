# Changelog

## 0.3.0 — initial C# package

This is the first C# release, aligned with the siblings' 0.3.0 calculation
behavior. No C# 0.1.x or 0.2.x releases were published.

- Add a dependency-free `net8.0` library with immutable coordinates, civil dates,
  parameters and prayer adjustments.
- Add supported presets and compiled country Auto resolution.
- Match minute-level prayer calculations, custom Maghrib angles, interval Isha,
  Shafi/Hanafi Asr, high-latitude rules and the caller-supplied Ramadan flag.
- Add nullable offset-aware times, current/next boundaries and Sunnah portions
  using tomorrow's recomputed Fajr.
- Include XML API documentation, an example CLI, deterministic data generation,
  conformance fixtures, cross-platform CI and local package verification.
- Align the bare Maghrib flag default with the sibling constructors while
  preserving the shared Custom preset's explicit zero-minute interval.
- Define chronological current/next helpers, including equal-time tie behavior;
  helper compatibility differs from the siblings' fixed prayer-name scans when
  schedules wrap or become unordered. Application city tweaks remain excluded,
  and Sunnah preserves country/city context as in Kotlin.
- Add reproducible focused captures with source revisions, custom/Sunnah UTC
  instants and explicit helper expectations, plus .NET 8 and .NET 10 package consumers.
- Build and upload GitHub Releases from version tags after cross-platform
  verification, including NuGet packages, portable symbols and SHA256 checksums.
