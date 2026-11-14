import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location("custody", Path(__file__).with_name("verify-leave-custody.py"))
custody = importlib.util.module_from_spec(spec)
spec.loader.exec_module(custody)


class ReceiptTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        self.owner = "leave-123-1"
        self.fixture = "a" * 32
        self.session = "00000000-0000-4000-8000-000000000001"
        self.path = self.root / ("fixture-" + self.fixture + ".jsonl")
        self.header = {"type": "session", "owner": self.owner, "fixture": self.fixture,
                       "session": self.session, "reaperName": "testcontainers-ryuk-" + self.session,
                       "persistentData": False, "processId": 123, "processStartedUtc": "2026-11-14T04:00:00Z",
                       "expiryUtc": "2026-11-14T04:35:00Z", "executable": "/sdk/dotnet"}
        self.rows = [self.header, {"type": "reaper-state", "session": self.session, "state": "MaintainingConnection"}]
        for c in "cde":
            self.rows += [{"type": "fixture-start-attempt"},
                          {"type": "fixture-created", "id": c * 64},
                          {"type": "fixture-observed", "id": c * 64},
                          {"type": "reaper-observed", "id": "b" * 64, "name": "/" + self.header["reaperName"]}]
        self.write()

    def write(self):
        self.path.write_text("".join(json.dumps(row) + "\n" for row in self.rows), encoding="utf-8")

    def test_complete_receipt_and_absence_are_required(self):
        rows = custody.receipts(self.root, self.owner, True)
        self.assertTrue(custody.absent(rows, self.owner, set(), lambda _: set()))
        self.assertFalse(custody.absent(rows, self.owner, {"b" * 64}, lambda _: set()))
        self.assertFalse(custody.absent(rows, self.owner, set(), lambda _: {"f" * 64}))

    def test_missing_or_wrong_identity_cannot_prove_cleanup(self):
        self.path.unlink()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, True)
        self.write()
        self.rows[0]["owner"] = "foreign"
        self.write()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, True)
        self.rows[0]["owner"] = self.owner
        self.rows[5]["name"] = "/other-reaper"
        self.write()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, True)

    def test_success_requires_all_three_fixtures_and_observed_reaper(self):
        self.rows = self.rows[:-4]
        self.write()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, True)
        self.assertEqual(1, len(custody.receipts(self.root, self.owner, False)))
        self.rows.append({"type": "fixture-start-attempt"})
        self.write()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, False)

    def test_unknown_or_malformed_identity_fails_closed(self):
        self.rows.append({"type": "fixture-created", "id": "short"})
        self.write()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, False)
        self.rows[-1] = {"type": "unknown"}
        self.write()
        with self.assertRaises(ValueError): custody.receipts(self.root, self.owner, False)


if __name__ == "__main__":
    unittest.main()
