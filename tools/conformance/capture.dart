import 'dart:io';
import 'package:prayer_time_plus/prayer_time_plus.dart';

void main(List<String> args) {
  for (final line in File(args.single).readAsLinesSync()) {
    final f = line.split('\t');
    int i(int n) => int.parse(f[n]);
    double d(int n) => double.parse(f[n]);
    final p = f[25] == 'direct'
        ? CalculationParameters(fajrAngle: 18, ishaValue: 17)
        : CalculationMethod.fromKey(f[1])!.getParameters();
    p.madhab = Madhab.values[i(11)];
    p.highLatitudeRule = HighLatitudeRule.values[i(12)];
    p.isRamadan = f[13] == '1';
    if (f[14] != '_') p.fajrAngle = d(14);
    if (f[15] != '_') p.maghribIsInterval = f[15] == '1';
    if (f[16] != '_') p.maghribValue = d(16);
    if (f[17] != '_') p.ishaIsInterval = f[17] == '1';
    if (f[18] != '_') p.ishaValue = d(18);
    p.adjustments = PrayerAdjustments(
      fajr: i(19), sunrise: i(20), dhuhr: i(21), asr: i(22),
      maghrib: i(23), isha: i(24),
    );
    final a = p.methodAdjustments;
    int methodOffset(int index, int existing) =>
        f[index] == '_' ? existing : i(index);
    p.methodAdjustments = PrayerAdjustments(
      fajr: methodOffset(26, a.fajr), sunrise: methodOffset(27, a.sunrise),
      dhuhr: methodOffset(28, a.dhuhr), asr: methodOffset(29, a.asr),
      maghrib: methodOffset(30, a.maghrib), isha: methodOffset(31, a.isha),
    );
    final offset = Duration(minutes: i(8));
    final times = PrayerTimes(
      Coordinates(d(2), d(3), altitude: d(4)),
      DateComponents(i(5), i(6), i(7)), p,
      utcOffset: offset, countryCode: f[9], cityName: f[10],
    );
    final sunnah = SunnahTimes(times);
    String stamp(DateTime? value) => value == null ? '_' :
        '${value.toIso8601String().substring(0, 16)}|'
        '${value.subtract(offset).toIso8601String().substring(0, 16)}Z';
    final helpers = List.generate(24, (hour) {
      final instant = DateTime.utc(i(5), i(6), i(7), hour).subtract(offset);
      return '${times.currentPrayer(instant).name}/${times.nextPrayer(instant).name}';
    });
    stdout.writeln([
      f[0], ...[times.fajr, times.sunrise, times.dhuhr, times.asr,
        times.sunset, times.maghrib, times.isha].map(stamp),
      stamp(sunnah.middleOfTheNight), stamp(sunnah.lastThirdOfTheNight),
      helpers.join(','),
    ].join('\t'));
  }
}
