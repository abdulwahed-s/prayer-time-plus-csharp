# Contributing to PrayerTimePlus

Thanks for your interest in improving **PrayerTimePlus**, the C# / .NET port of
an offline Islamic prayer-times engine. This guide covers development setup,
the standards for changes, and the pull request workflow.

The same engine also ships for
[Dart / Flutter](https://github.com/abdulwahed-s/prayer_time_plus),
[Swift](https://github.com/abdulwahed-s/prayer-time-plus-swift), and
[Kotlin / JVM](https://github.com/abdulwahed-s/prayer-time-plus-kotlin).
Discuss changes to computed times across the ports so their supported
calculations stay aligned. The [README](../README.md) describes intentional
helper and city-context differences.

## Ways to contribute

- **Report a bug** or an incorrect prayer time.
- **Request a feature** or a new calculation method.
- **Improve the docs**, XML API comments, or the example.
- **Send a pull request** using the workflow below.

Open issues from the [templates](https://github.com/abdulwahed-s/prayer-time-plus-csharp/issues/new/choose).
For security reports, follow [SECURITY.md](SECURITY.md) and report privately.

## Development setup

Install the stable .NET 10 SDK pinned in [global.json](../global.json).
The library targets `net8.0`; tests, the data generator, and the example target
`net10.0`. C# 12 is the language baseline. Install the .NET 8 SDK alongside the
pinned SDK for the package-consumer checks, which need its runtime and reference
packs. The package verifier runs in PowerShell.

Run these commands from the repository root:

```sh
dotnet restore PrayerTimePlus.slnx
dotnet build PrayerTimePlus.slnx -c Release --no-restore
dotnet format PrayerTimePlus.slnx --verify-no-changes --no-restore
dotnet test tests/PrayerTimePlus.Tests/PrayerTimePlus.Tests.csproj -c Release --no-build --no-restore
dotnet run --project examples/PrayerTimePlus.Example -c Release --no-build
dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release --no-build -- --check
dotnet pack src/PrayerTimePlus/PrayerTimePlus.csproj -c Release --no-build --no-restore -o artifacts/packages
```

Then verify the packed library from isolated consumers on actual .NET 8 and
.NET 10 runtimes, using only the local NuGet package:

```powershell
./tools/Verify-Package.ps1
```

Builds and normal tests use committed data and fixtures. They require no
sibling checkout, Python, Dart, Java, or Kotlin toolchain.

## Standards every change is held to

1. **Zero third-party runtime dependencies.** Use the .NET base class library.
   Test and development tools may use pinned dependencies with
   `PrivateAssets="all"` where appropriate.
2. **Preserve numeric parity.** Keep formulas, literal constants, evaluation
   order, and nearest-minute rounding. For an intentional output change, explain
   the correction and include independently justified regression expectations.
   Do not derive expected fixtures from the C# implementation being tested.
3. **Keep verification green.** Release builds, formatting, tests, generated
   data checks, the example, and package-consumer checks must pass. Compiler and
   analyzer warnings are errors. Cover new calculation behavior with tests.
4. **Document the public API.** Every public declaration needs self-contained
   `///` XML documentation, including units, defaults, null behavior, and
   validation exceptions. Include runnable examples for main types and keep
   implementation helpers `internal`.

Keep calculation code pure and safe for concurrent callers. Location lookup,
time-zone discovery, scheduling, and UI belong in consumer applications.

## Commit & PR conventions

- Use [Conventional Commits](https://www.conventionalcommits.org/):
  `type(scope): subject` in the imperative mood, such as
  `fix(engine): preserve interval Isha adjustments` or
  `docs: clarify utcOffset handling`. Types include `feat`, `fix`, `test`,
  `docs`, `refactor`, `perf`, `chore`, `build`, and `ci`.
- Keep commits small and atomic, with one logical change each.
- Bundled method parameters and Auto mappings are generated. Edit JSON under
  `tools/data/`, run the command below, and commit the generated source and
  method documentation. Do not hand-edit generated files.
- Keep pull requests focused, fill in the template, and link related issues.

```sh
dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release
```

If changing sibling conformance fixtures, follow the
[independent capture recipe](../tools/conformance/README.md) and review its
recorded source revisions and expected differences.

## Code of conduct

Be respectful and constructive. Harassment or abuse is not welcome in issues,
pull requests, or discussions.
