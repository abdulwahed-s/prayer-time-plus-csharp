"""Capture focused conformance data from independent Dart and Kotlin APIs."""

import argparse
import datetime as dt
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
PRAYERS = ["fajr", "sunrise", "dhuhr", "asr", "maghrib", "isha"]
SLOTS = ["fajr", "sunrise", "dhuhr", "asr", "sunset", "maghrib", "isha"]


def run(arguments):
    return subprocess.run(arguments, check=True, capture_output=True, text=True).stdout


def revision(path):
    if run(["git", "-C", str(path), "status", "--porcelain", "--untracked-files=no"]).strip():
        raise ValueError(f"Sibling has tracked edits; commit them before capture: {path}")
    return run(["git", "-C", str(path), "rev-parse", "HEAD"]).strip()


def read_results(output):
    results = {}
    for line in output.splitlines():
        fields = line.split("\t")
        if len(fields) != 11 or fields[0] in results:
            raise ValueError(f"Invalid or duplicate capture row: {line}")
        def stamp(value):
            if value == "_":
                return None
            local, utc = value.split("|")
            return {"local": local, "utc": utc}
        results[fields[0]] = {
            "times": [stamp(value) for value in fields[1:8]],
            "sunnah": [stamp(value) for value in fields[8:10]],
            "helpers": fields[10].split(","),
        }
    return results


def helpers_from_instants(case, times):
    # Derive the declared C# policy from independent captured instants, never C# output.
    boundaries = [(name, times[SLOTS.index(name)]) for name in PRAYERS
                  if times[SLOTS.index(name)] is not None]
    year, month, day = case["date"]
    offset = dt.timezone(dt.timedelta(minutes=case["offsetMinutes"]))
    queries = []
    for hour in range(24):
        at = dt.datetime(year, month, day, hour, tzinfo=offset).astimezone(dt.timezone.utc)
        before = [(dt.datetime.fromisoformat(value["utc"]), PRAYERS.index(name), name)
                  for name, value in boundaries if dt.datetime.fromisoformat(value["utc"]) <= at]
        after = [(dt.datetime.fromisoformat(value["utc"]), PRAYERS.index(name), name)
                 for name, value in boundaries if dt.datetime.fromisoformat(value["utc"]) > at]
        queries.append({"localHour": hour, "current": max(before)[2] if before else "none",
                        "next": min(after)[2] if after else "none"})
    return queries


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dart-root", type=Path, required=True)
    parser.add_argument("--kotlin-root", type=Path, required=True)
    parser.add_argument("--kotlin-jar", type=Path, required=True)
    parser.add_argument("--kotlin-stdlib", type=Path, required=True)
    parser.add_argument("--check", action="store_true", help="Compare captures without overwriting the fixture")
    arguments = parser.parse_args()
    dart_root, kotlin_root = arguments.dart_root.resolve(), arguments.kotlin_root.resolve()
    sources = {
        "dart": {"repository": "https://github.com/abdulwahed-s/prayer_time_plus",
                 "revision": revision(dart_root)},
        "kotlin": {"repository": "https://github.com/abdulwahed-s/prayer-time-plus-kotlin",
                   "revision": revision(kotlin_root),
                   "jarSha256": hashlib.sha256(arguments.kotlin_jar.read_bytes()).hexdigest()},
    }
    specification = json.loads((HERE / "cases.json").read_text(encoding="utf-8"))
    cases = [specification["defaults"] | case for case in specification["cases"]]
    rows = []
    for case in cases:
        parameters = case["parameters"]
        adjustments = case["adjustments"]
        method_adjustments = case.get("methodAdjustments", {})
        row = [case["id"], case["method"], *case["coordinates"], *case["date"],
               case["offsetMinutes"], case["country"], case["city"], case["madhab"], case["rule"],
               int(case["ramadan"])]
        for key in ["fajrAngle", "maghribIsInterval", "maghribValue", "ishaIsInterval", "ishaValue"]:
            value = parameters.get(key, "_")
            row.append(int(value) if isinstance(value, bool) else value)
        row.extend(adjustments.get(key, 0) for key in PRAYERS)
        row.append(case["construction"])
        row.extend(method_adjustments.get(key, "_") for key in PRAYERS)
        rows.append("\t".join(map(str, row)))
    artifacts = (ROOT / "artifacts").resolve()
    if not artifacts.is_relative_to(ROOT.resolve()):
        raise ValueError("Capture artifacts must remain inside the repository")
    artifacts.mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="conformance-", dir=artifacts) as temporary:
        work = Path(temporary)
        if not work.resolve().is_relative_to(artifacts):
            raise ValueError("Capture temporary directory is outside its verified parent")
        inputs = work / "inputs.tsv"
        inputs.write_text("\n".join(rows) + "\n", encoding="utf-8")
        dart = read_results(run([shutil.which("dart"),
            f"--packages={dart_root / '.dart_tool/package_config.json'}", str(HERE / "capture.dart"), str(inputs)]))
        classpath = os.pathsep.join(map(str, [arguments.kotlin_jar.resolve(), arguments.kotlin_stdlib.resolve()]))
        run(["javac", "-cp", classpath, "-d", str(work), str(HERE / "Capture.java")])
        kotlin = read_results(run(["java", "-cp", str(work) + os.pathsep + classpath, "Capture", str(inputs)]))
    vectors = []
    expected_ids = {case["id"] for case in cases}
    if set(dart) != expected_ids or set(kotlin) != expected_ids:
        raise ValueError("Capture output does not match the requested case IDs")
    for case in cases:
        identifier = case["id"]
        for field, allowed in [("times", "allowDartPrayerDifference"), ("sunnah", "allowDartSunnahDifference")]:
            if dart[identifier][field] != kotlin[identifier][field] and not case.get(allowed, False):
                raise ValueError(f"Unexpected independent-engine difference: {identifier} / {field}")
        vectors.append({"input": case, "expected": kotlin[identifier], "dart": dart[identifier],
                        "chronologicalHelpers": helpers_from_instants(case, kotlin[identifier]["times"])})
    fixture = {"schemaVersion": 1, "sources": sources,
        "helperPolicy": "Chronological instants; current ties use the later prayer name, next ties the earlier.",
        "vectors": vectors}
    output = json.dumps(fixture, indent=2, ensure_ascii=False) + "\n"
    destination = ROOT / "tests/PrayerTimePlus.Tests/Fixtures/focused-sibling-goldens.json"
    if arguments.check:
        if destination.read_text(encoding="utf-8") != output:
            raise ValueError("Focused fixture differs; review captures before replacing it")
    else:
        destination.write_text(output, encoding="utf-8", newline="\n")
    print(f"{'Verified' if arguments.check else 'Captured'} {len(vectors)} independent focused vectors.")


if __name__ == "__main__":
    main()
