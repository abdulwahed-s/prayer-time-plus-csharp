# Sibling golden vectors

`sibling-goldens.json` contains 1,232 minute-exact vectors captured by running
the Dart `prayer_time_plus` 0.3.0 engine with explicit, fixed inputs. Each row
records its stable method key, coordinates in degrees/metres, Gregorian civil
date, UTC offset in minutes, country code, Madhab and high-latitude choice.
Expected clock values are Fajr, Sunrise, Dhuhr, Asr, Sunset, Maghrib, Isha;
undefined values are JSON null.

The rule values map to Automatic=0, None=1, MiddleOfTheNight=2,
SeventhOfTheNight=3, TwilightAngle=4. Madhab values map to Shafi=0, Hanafi=1.
The tests compare both local minutes and absolute UTC instants. This includes
56 shared preset labels, eleven scenarios and both Madhabs. The Kotlin `none`
placeholder is verified separately against the shared MWL defaults.

These fixtures are standalone test data. Test runs require no Dart installation,
network request or sibling checkout. Further cases should be captured from an
independent sibling implementation with explicit inputs, rather than deriving
expected values from the C# implementation.

`focused-sibling-goldens.json` adds twenty focused cases independently captured
from both Kotlin and Dart, with source revisions and the Kotlin JAR SHA256.
It includes omitted Maghrib flags, custom tuning, Ramadan, Sunnah rollovers and
country elevation, plus helper schedules with wrapped, equal and undefined
boundaries. Local date/time fields and normalized UTC instants are recorded.
Kotlin supplies expected prayer/Sunnah values; the raw Dart results preserve
the documented city/context exceptions. Chronological helper expectations are
derived from independently captured instants under the declared C# policy.
See [the capture recipe](../../../tools/conformance/README.md) for regeneration.
