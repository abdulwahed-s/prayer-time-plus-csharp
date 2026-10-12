# Security Policy

## Supported versions

PrayerTimePlus is pre-1.0. Security maintenance focuses on the latest `0.x`
release. Version **0.3.0** is the initial C# release, distributed through
[NuGet.org](https://www.nuget.org/packages/PrayerTimePlus) and
[GitHub Releases](https://github.com/abdulwahed-s/prayer-time-plus-csharp/releases).

| Version | Status |
| ------- | ------ |
| 0.3.x | Supported |
| Earlier C# versions | No published releases |

## Reporting a vulnerability

**Please do not report security vulnerabilities through public issues, pull
requests, or discussions.**

Report privately using GitHub's
[**Report a vulnerability**](https://github.com/abdulwahed-s/prayer-time-plus-csharp/security/advisories/new)
link on the repository's **Security** tab.

Please include:

- The affected package version, .NET runtime, and operating system.
- A description of the issue and its impact.
- Steps or a minimal C# example that reproduces it.
- Any suggested fix, if you have one.

The maintainer will review the report privately and coordinate any confirmed
fix and public advisory. Reporters are credited unless they prefer to remain
anonymous.

## Scope

This is an offline calculation library with no third-party runtime dependencies.
It takes coordinates, a civil date, calculation parameters, and a supplied UTC
offset, and returns times. Its calculation path performs no network, file, or
process access.

Incorrect prayer times usually belong in the
[bug report template](ISSUE_TEMPLATE/bug_report.md). Exploitable behavior,
denial of service, and package or build supply-chain concerns belong in a
private security report.
