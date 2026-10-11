# Calculation methods

Generated from the committed inputs in `tools/data/`. Regenerate with:

```sh
dotnet run --project tools/PrayerTimePlus.DataGenerator -c Release
```

Keys are case-sensitive; `CalculationMethods.FromKey` returns null for unknown keys. Country Auto matching ignores case but preserves whitespace. Unknown countries use `mwl`. Each preset returns fresh parameters with Shafi Asr, Automatic high-latitude handling, zero user adjustments and Ramadan disabled.

Maghrib and Isha values are degrees when the interval flag is 0, and minutes when it is 1. A non-positive Maghrib angle uses Sunset. A positive angle must be finite, later than Sunset and earlier than angle-based Isha; otherwise it falls back to Sunset plus its offsets. Interval Isha starts from the final Maghrib. Offsets are signed minutes in Fajr, Sunrise, Dhuhr, Asr, Maghrib, Isha order.

| Key | C# preset | Fajr degrees | Maghrib interval | Maghrib value | Isha interval | Isha value | Fajr offset | Sunrise offset | Dhuhr offset | Asr offset | Maghrib offset | Isha offset |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `aachen` | `Aachen` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `algeria` | `Algeria` | 18.0 | 1 | 3.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `austria` | `Austria` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `azrou` | `Azrou` | 19.1 | 0 | 0.0 | 0 | 17.0 | 0 | -1 | 5 | 0 | 1 | 0 |
| `belgium` | `Belgium` | 18.0 | 0 | 0.0 | 0 | 18.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `birmingham` | `Birmingham` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `blackburn` | `Blackburn` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `calgary` | `Calgary` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `custom` | `Custom` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `czech` | `Czech` | 12.04 | 0 | 0.0 | 0 | 12.04 | 0 | 0 | 5 | 0 | -2 | 0 |
| `dordrecht` | `Dordrecht` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `egypt` | `Egyptian` | 19.5 | 1 | 0.0 | 0 | 17.5 | 0 | 0 | 0 | 0 | 0 | 0 |
| `eindhoven` | `Eindhoven` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `emirates` | `Emirates` | 18.5 | 1 | 2.0 | 0 | 18.5 | 1 | -4 | 2 | 0 | 0 | -3 |
| `fribourg` | `Fribourg` | 18.01 | 0 | 0.0 | 1 | 100.0 | 1 | 5 | 0 | 0 | -5 | 0 |
| `indonesia` | `Indonesia` | 20.0 | 1 | 0.0 | 0 | 18.0 | 2 | -2 | 2 | 2 | 2 | 2 |
| `iraq` | `Iraq` | 18.0 | 0 | 0.0 | 0 | 17.0 | 0 | 0 | 5 | 3 | 2 | 0 |
| `isna` | `NorthAmerica` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `jordan` | `Jordan` | 18.12 | 0 | 0.0 | 0 | 17.975 | 0 | 0 | 0 | 0 | 1 | 0 |
| `karachi` | `Karachi` | 18.0 | 1 | 0.0 | 0 | 18.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `kazakhstan` | `Kazakhstan` | 14.97 | 0 | 0.0 | 0 | 14.96 | 0 | 0 | 5 | 5 | 0 | 0 |
| `kuwait` | `Kuwait` | 18.0 | 1 | 0.0 | 0 | 17.5 | 0 | 0 | 0 | 0 | 0 | 0 |
| `libya` | `Libya` | 18.3 | 0 | 0.0 | 0 | 18.35 | 0 | 0 | 4 | 0 | 4 | 0 |
| `london` | `London` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `luxembourg` | `Luxembourg` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `lyon` | `Lyon` | 12.0 | 1 | 0.0 | 0 | 12.0 | -5 | 0 | 5 | 0 | 4 | 5 |
| `makkah` | `Makkah` | 18.5 | 1 | 0.0 | 1 | 90.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `malaysia` | `Malaysia` | 20.0 | 1 | 0.0 | 0 | 18.0 | 0 | 0 | 1 | 0 | 0 | 0 |
| `malaysia2` | `Malaysia2` | 20.0 | 1 | 0.0 | 0 | 18.46 | 0 | 0 | 3 | 2 | 1 | 0 |
| `maldives` | `Maldives` | 19.0 | 1 | 0.0 | 0 | 19.0 | 0 | -1 | 4 | 1 | 1 | 1 |
| `mississauga` | `Mississauga` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `montreal` | `Montreal` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `morocco` | `Morocco` | 19.09 | 0 | 0.0 | 0 | 17.0 | 0 | -2 | 5 | 0 | 3 | 0 |
| `moscow` | `Moscow` | 16.0 | 0 | 0.0 | 0 | 15.1 | 0 | 0 | 1 | 1 | 1 | 2 |
| `munchen` | `Munchen` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `mwl` | `MuslimWorldLeague` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `none` | `None` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `nurnberg` | `Nurnberg` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `oman` | `Oman` | 18.0 | 1 | 5.0 | 0 | 18.0 | 0 | 0 | 5 | 5 | 0 | 1 |
| `omanMuscat` | `OmanMuscat` | 17.74 | 0 | 0.0 | 0 | 18.229 | 0 | 1 | 6 | 6 | 6 | 0 |
| `orleans` | `Orleans` | 15.0 | 0 | 0.0 | 0 | 12.34 | 0 | 0 | 5 | 0 | 0 | 0 |
| `palestine` | `Palestine` | 20.11 | 0 | 0.0 | 0 | 17.9 | 0 | -5 | 0 | 0 | 4 | 0 |
| `paris` | `Paris` | 12.0 | 1 | 0.0 | 0 | 12.0 | -5 | 0 | 5 | 0 | 4 | 5 |
| `potsdam` | `Potsdam` | 18.0 | 1 | 0.0 | 0 | 17.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `qatar` | `Qatar` | 18.0 | 1 | 0.0 | 1 | 90.0 | 0 | 0 | 0 | 0 | 2 | 0 |
| `rotterdam` | `Rotterdam` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `southkorea` | `SouthKorea` | 18.0 | 1 | 0.0 | 0 | 18.0 | 1 | -1 | 0 | 0 | 0 | -6 |
| `sudan` | `Sudan` | 18.12 | 0 | 0.0 | 0 | 17.88 | 0 | 3 | 3 | 0 | -4 | 0 |
| `switzerland` | `Switzerland` | 17.99 | 0 | 0.0 | 1 | 100.0 | 1 | 4 | 0 | 0 | -4 | 0 |
| `syria` | `Syria` | 19.5 | 1 | 0.0 | 0 | 17.5 | 0 | 0 | 0 | 0 | 0 | 0 |
| `tajikistan` | `Tajikistan` | 18.0 | 1 | 0.0 | 0 | 18.0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `toulouse` | `Toulouse` | 12.0 | 1 | 0.0 | 0 | 12.0 | -5 | 0 | 5 | 0 | 4 | 5 |
| `tunisia` | `Tunisia` | 18.0 | 1 | 1.0 | 0 | 18.0 | -1 | 0 | 7 | 0 | 0 | 1 |
| `turkey` | `Turkey` | 18.0 | 0 | 0.0 | 0 | 16.93 | 0 | -6 | 6 | 4 | 5 | 0 |
| `uoif` | `Uoif` | 12.0 | 1 | 0.0 | 0 | 12.0 | -5 | 0 | 5 | 0 | 4 | 5 |
| `windsor` | `Windsor` | 15.0 | 1 | 0.0 | 0 | 15.0 | 0 | 0 | 0 | 0 | 0 | 0 |

`Dubai` has stable key `dubai` and uses the same numeric fallback as `mwl`. `None` is a placeholder with MWL values; `Custom` is the shared custom starting point. Shia presets are unsupported. City-labelled presets use the listed parameters without city tables or local tweaks.

The horizon depression is 0.833 degrees. Only methods `iraq`, `morocco`, `tunisia`, `jordan`, `orleans`, `sudan`, `belgium`, `kazakhstan`, or countries `PS`, `IL`, `CZ`, `CH`, add `0.0347 * sqrt(altitudeMetres)` to the depression. Country matching ignores case without trimming.

The `makkah` preset with country `SA` and `IsRamadan = true` adds 30 minutes to Isha before twilight correction.
