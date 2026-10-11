## Summary

<!-- What does this PR change, and why? -->

## Related issues

<!-- e.g. Closes #123 -->

## Type of change

- [ ] Bug fix (corrects an incorrect result or defect)
- [ ] New feature (e.g. a new calculation method or API)
- [ ] Refactor / internal change (no behavior change)
- [ ] Documentation
- [ ] Build / CI / tooling

## Does this change any computed prayer time?

- [ ] No; results are identical and golden fixtures are unchanged.
- [ ] Yes; I explained the correction above and included independently justified regression expectations.

## Checklist

- [ ] Commits follow Conventional Commits and are small and focused.
- [ ] `dotnet build PrayerTimePlus.slnx -c Release --no-restore` passes with zero warnings.
- [ ] `dotnet format PrayerTimePlus.slnx --verify-no-changes --no-restore` passes.
- [ ] `dotnet test tests/PrayerTimePlus.Tests/PrayerTimePlus.Tests.csproj -c Release --no-build --no-restore` passes.
- [ ] Public API changes have complete XML documentation and working examples.
- [ ] Generated data passes `dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release --no-build -- --check`; source JSON changes were regenerated.
- [ ] Package changes pass `dotnet pack` and `./tools/Verify-Package.ps1` on actual .NET 8 and .NET 10 runtimes (if applicable).
- [ ] No new third-party runtime dependency was added to the library.

<!-- See .github/CONTRIBUTING.md for setup and the complete verification commands. -->
