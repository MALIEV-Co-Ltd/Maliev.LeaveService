import importlib.util
from pathlib import Path
import tempfile
import unittest
import xml.etree.ElementTree as ET

spec = importlib.util.spec_from_file_location("coverage", Path(__file__).with_name("verify-leave-coverage.py"))
coverage = importlib.util.module_from_spec(spec)
spec.loader.exec_module(coverage)


class CoverageControls(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        self.xml = ET.Element("coverage")
        packages = ET.SubElement(self.xml, "packages")
        for name in sorted(coverage.EXPECTED):
            package = ET.SubElement(packages, "package", name=name)
            classes = ET.SubElement(package, "classes")
            cls = ET.SubElement(classes, "class", filename=name + ".cs")
            lines = ET.SubElement(cls, "lines")
            for i in range(5):
                ET.SubElement(lines, "line", number=str(i + 1), hits="1" if i < 4 else "0")
        self.report = self.root / "coverage.cobertura.xml"
        self.save()

    def save(self):
        ET.ElementTree(self.xml).write(self.report, encoding="utf-8")

    def test_exact_floor_and_no_exclusions(self):
        result = coverage.evaluate(self.root)
        self.assertEqual([], result["exclusions"])
        self.assertTrue(all(row["covered"] == 4 and row["valid"] == 5 and row["passed"] for row in result["assemblies"]))

    def test_missing_assembly_or_below_floor_fails(self):
        package = self.xml.find("./packages/package")
        package.find("./classes/class/lines/line").set("hits", "0")
        self.save()
        self.assertFalse(all(row["passed"] for row in coverage.evaluate(self.root)["assemblies"]))
        self.xml.find("./packages").remove(package)
        self.save()
        self.assertTrue(any(row["valid"] == 0 and not row["passed"] for row in coverage.evaluate(self.root)["assemblies"]))

    def test_absent_or_conflicting_reports_fail_closed(self):
        other = self.root / "other"
        other.mkdir()
        (other / "coverage.cobertura.xml").write_text("<coverage/>")
        with self.assertRaises(ValueError): coverage.evaluate(self.root)
        (other / "coverage.cobertura.xml").unlink()
        self.report.unlink()
        with self.assertRaises(ValueError): coverage.evaluate(self.root)

    def test_duplicate_raw_line_is_not_double_counted(self):
        lines = self.xml.find("./packages/package/classes/class/lines")
        ET.SubElement(lines, "line", number="1", hits="0")
        self.save()
        self.assertTrue(all(row["valid"] == 5 for row in coverage.evaluate(self.root)["assemblies"]))


if __name__ == "__main__":
    unittest.main()
