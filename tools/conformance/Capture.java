import io.github.abdulwaheds.prayertimeplus.*;
import java.nio.file.*;
import java.time.*;
import java.time.format.*;
import java.util.*;

class Capture {
    static int i(String[] fields, int index) { return Integer.parseInt(fields[index]); }
    static double d(String[] fields, int index) { return Double.parseDouble(fields[index]); }
    static int methodOffset(String[] fields, int index, int existing) {
        return fields[index].equals("_") ? existing : i(fields, index);
    }
    static String stamp(OffsetDateTime value) {
        if (value == null) return "_";
        var format = DateTimeFormatter.ofPattern("uuuu-MM-dd'T'HH:mm");
        return value.format(format) + "|" + value.withOffsetSameInstant(ZoneOffset.UTC).format(format) + "Z";
    }
    static CalculationParameters bareDefaults() throws ReflectiveOperationException {
        // Invoke Kotlin's generated default constructor, preserving every optional default.
        var constructor = CalculationParameters.class.getDeclaredConstructor(
            String.class, double.class, boolean.class, double.class, boolean.class, double.class,
            PrayerAdjustments.class, PrayerAdjustments.class, Madhab.class, HighLatitudeRule.class,
            boolean.class, int.class, Class.forName("kotlin.jvm.internal.DefaultConstructorMarker"));
        return constructor.newInstance(null, 18.0, false, 0.0, false, 17.0,
            null, null, null, null, false, 2013, null);
    }
    public static void main(String[] args) throws Exception {
        for (String line : Files.readAllLines(Path.of(args[0]))) {
            String[] f = line.split("\t", -1);
            CalculationParameters p = f[25].equals("direct")
                ? bareDefaults() : CalculationMethod.Companion.fromKey(f[1]).parameters();
            var a = p.getMethodAdjustments();
            p = p.copy(p.getMethod(), f[14].equals("_") ? p.getFajrAngle() : d(f, 14),
                f[15].equals("_") ? p.getMaghribIsInterval() : f[15].equals("1"),
                f[16].equals("_") ? p.getMaghribValue() : d(f, 16),
                f[17].equals("_") ? p.getIshaIsInterval() : f[17].equals("1"),
                f[18].equals("_") ? p.getIshaValue() : d(f, 18),
                new PrayerAdjustments(methodOffset(f, 26, a.getFajr()), methodOffset(f, 27, a.getSunrise()),
                    methodOffset(f, 28, a.getDhuhr()), methodOffset(f, 29, a.getAsr()),
                    methodOffset(f, 30, a.getMaghrib()), methodOffset(f, 31, a.getIsha())),
                new PrayerAdjustments(i(f, 19), i(f, 20), i(f, 21), i(f, 22), i(f, 23), i(f, 24)),
                Madhab.values()[i(f, 11)], HighLatitudeRule.values()[i(f, 12)], f[13].equals("1"));
            ZoneOffset offset = ZoneOffset.ofTotalSeconds(i(f, 8) * 60);
            var times = new PrayerTimes(new Coordinates(d(f, 2), d(f, 3), d(f, 4)),
                new DateComponents(i(f, 5), i(f, 6), i(f, 7)), p, offset, f[9], f[10]);
            var sunnah = new SunnahTimes(times);
            List<String> output = new ArrayList<>();
            output.add(f[0]);
            for (var value : new OffsetDateTime[] { times.getFajr(), times.getSunrise(), times.getDhuhr(),
                times.getAsr(), times.getSunset(), times.getMaghrib(), times.getIsha(),
                sunnah.getMiddleOfTheNight(), sunnah.getLastThirdOfTheNight() }) {
                output.add(stamp(value));
            }
            List<String> helpers = new ArrayList<>();
            for (int hour = 0; hour < 24; hour++) {
                var at = OffsetDateTime.of(i(f, 5), i(f, 6), i(f, 7), hour, 0, 0, 0, offset);
                helpers.add(times.currentPrayer(at).name().toLowerCase(Locale.ROOT) + "/"
                    + times.nextPrayer(at).name().toLowerCase(Locale.ROOT));
            }
            output.add(String.join(",", helpers));
            System.out.println(String.join("\t", output));
        }
    }
}
