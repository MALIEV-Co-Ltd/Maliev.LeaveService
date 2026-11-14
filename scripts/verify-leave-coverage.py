"""Require raw, nonempty 80% line coverage for every owned assembly."""
import hashlib
import json
from pathlib import Path
import sys
import xml.etree.ElementTree as ET

EXPECTED = {"Maliev.LeaveService." + name for name in ("Api", "Application", "Domain", "Infrastructure")}


def evaluate(root):
    reports = sorted(root.rglob("coverage.cobertura.xml"))
    if not reports or len(reports) > 8 or any(not p.is_file() or p.is_symlink() or p.stat().st_size > 50_000_000 for p in reports):
        raise ValueError("Raw coverage report unavailable or outside bounds")
    digests = {hashlib.sha256(p.read_bytes()).hexdigest() for p in reports}
    if len(digests) != 1:
        raise ValueError("Raw coverage reports differ")
    lines = {name: {} for name in EXPECTED}
    for package in ET.parse(reports[0]).findall("./packages/package"):
        name = package.get("name", "")
        if name not in EXPECTED:
            continue
        for cls in package.findall("./classes/class"):
            filename = cls.get("filename")
            if not filename:
                raise ValueError("Missing production source filename")
            for line in cls.findall("./lines/line"):
                number, hits = int(line.attrib["number"]), int(line.attrib["hits"])
                if number <= 0 or hits < 0:
                    raise ValueError("Invalid raw line")
                key = (filename, number)
                lines[name][key] = lines[name].get(key, False) or hits > 0
    summary = []
    for name, inventory in sorted(lines.items()):
        valid, covered = len(inventory), sum(inventory.values())
        summary.append({"assembly": name, "covered": covered, "valid": valid,
                        "percent": covered * 100 / valid if valid else None,
                        "threshold": 80, "passed": valid > 0 and covered * 100 >= valid * 80})
    return {"raw_sha256": next(iter(digests)), "identical_raw_copies": len(reports),
            "exclusions": [], "assemblies": summary}


def main():
    if len(sys.argv) != 2:
        raise ValueError("One owned results directory required")
    root = Path(sys.argv[1])
    result = evaluate(root)
    (root / "coverage-gate.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, indent=2))
    if not all(row["passed"] for row in result["assemblies"]):
        raise ValueError("Owned assembly coverage below unchanged floor")


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, ET.ParseError, KeyError) as error:
        print("Leave coverage verification failed: " + type(error).__name__, file=sys.stderr)
        sys.exit(1)
