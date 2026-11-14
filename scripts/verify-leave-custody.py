"""Fail closed unless the task-owned fixture containers and session reaper are absent."""
import datetime as dt
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time

ID = re.compile(r"[0-9a-f]{64}\Z")
FIXTURE = re.compile(r"fixture-[0-9a-f]{32}\.jsonl\Z")
SESSION = re.compile(r"[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}\Z")
OWNER = re.compile(r"leave-[1-9][0-9]*-[1-9][0-9]*\Z")


def receipts(directory, owner, success):
    if not OWNER.fullmatch(owner) or not directory.is_dir() or directory.is_symlink():
        raise ValueError("Invalid owned ledger")
    files = list(directory.iterdir())
    if not files or len(files) > 8:
        raise ValueError("Expected bounded fixture receipts")
    observed = []
    for path in files:
        if not FIXTURE.fullmatch(path.name) or path.is_symlink() or not path.is_file() or path.stat().st_size > 262144:
            raise ValueError("Invalid receipt file")
        rows = [json.loads(line) for line in path.read_text(encoding="utf-8").splitlines()]
        if not rows or not all(isinstance(row, dict) for row in rows):
            raise ValueError("Invalid receipt rows")
        first = rows[0]
        session = first.get("session", "")
        fixture = path.stem.removeprefix("fixture-")
        if (first.get("type") != "session" or first.get("owner") != owner or first.get("fixture") != fixture
                or not SESSION.fullmatch(session) or first.get("reaperName") != "testcontainers-ryuk-" + session
                or first.get("persistentData") is not False):
            raise ValueError("Receipt identity mismatch")
        if (type(first.get("processId")) is not int or first["processId"] <= 0
                or not isinstance(first.get("executable"), str) or not first["executable"]
                or len(first["executable"]) > 4096):
            raise ValueError("Fixture process identity unavailable")
        try:
            started = dt.datetime.fromisoformat(first["processStartedUtc"].replace("Z", "+00:00"))
            expiry = dt.datetime.fromisoformat(first["expiryUtc"].replace("Z", "+00:00"))
        except (KeyError, TypeError, ValueError, AttributeError) as error:
            raise ValueError("Fixture lifetime unavailable") from error
        if started.tzinfo is None or expiry.tzinfo is None or not dt.timedelta(0) < expiry - started <= dt.timedelta(hours=1):
            raise ValueError("Fixture lifetime outside bound")
        ids = []
        for row in rows[1:]:
            kind = row.get("type")
            if kind not in {"reaper-state", "reaper-observed", "fixture-start-attempt", "fixture-created", "fixture-observed", "fixture-removed"}:
                raise ValueError("Unknown receipt entry")
            if kind == "reaper-state" and row.get("session") != session:
                raise ValueError("Reaper session mismatch")
            if kind in {"reaper-observed", "fixture-created", "fixture-observed", "fixture-removed"}:
                identity = row.get("id", "")
                if not isinstance(identity, str) or not ID.fullmatch(identity):
                    raise ValueError("Invalid container identity")
                ids.append(identity)
            if kind == "reaper-observed" and row.get("name") != "/" + first["reaperName"]:
                raise ValueError("Reaper name mismatch")
        created = [row["id"] for row in rows if row.get("type") == "fixture-created"]
        verified = [row["id"] for row in rows if row.get("type") == "fixture-observed"]
        reapers = [row["id"] for row in rows if row.get("type") == "reaper-observed"]
        attempts = sum(row.get("type") == "fixture-start-attempt" for row in rows)
        if attempts != len(created) or len(created) != len(set(created)) or set(created) != set(verified) or len(verified) != len(set(verified)):
            raise ValueError("Partial fixture creation leaves custody unverified")
        if len(set(reapers)) > 1:
            raise ValueError("Reaper identity changed")
        if success and (len(verified) != 3 or len(reapers) != 3
                        or not any(row.get("type") == "reaper-state" and row.get("state") == "MaintainingConnection" for row in rows)):
            raise ValueError("Successful suite lacks complete observed custody")
        observed.append({"fixture": fixture, "reaper": first["reaperName"], "ids": sorted(set(ids)),
                         "processId": first.get("processId"), "processStartedUtc": first.get("processStartedUtc"),
                         "executable": first.get("executable"), "expiryUtc": first.get("expiryUtc"),
                         "observations": [{key: row.get(key) for key in ("type", "id", "name", "image", "createdUtc", "mounts", "ports") if key in row}
                                          for row in rows if row.get("type") in {"reaper-observed", "fixture-observed"}]})
    return observed


def docker_ids(*filters):
    command = ["docker", "ps", "-aq", "--no-trunc"]
    for value in filters:
        command += ["--filter", value]
    result = subprocess.run(command, capture_output=True, check=True, timeout=10)
    if len(result.stdout) > 1048576:
        raise ValueError("Docker inventory quota")
    values = result.stdout.decode("ascii").splitlines()
    if any(not ID.fullmatch(value) for value in values):
        raise ValueError("Invalid Docker inventory identity")
    return set(values)


def absent(rows, owner, all_ids, query):
    if any(identity in all_ids for row in rows for identity in row["ids"]):
        return False
    if query("label=maliev.test.owner=" + owner):
        return False
    return all(not query("name=^/" + row["reaper"] + "$") for row in rows)


def main():
    owner = os.environ["MALIEV_TEST_RESOURCE_OWNER"]
    rows = receipts(Path(os.environ["MALIEV_RESOURCE_LEDGER"]), owner, os.environ["LEAVE_TEST_OUTCOME"] == "success")
    subprocess.run(["docker", "info"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=True, timeout=10)
    deadline = time.monotonic() + 45
    while True:
        if absent(rows, owner, docker_ids(), docker_ids):
            subprocess.run(["docker", "info"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=True, timeout=10)
            print(json.dumps({"owner": owner, "fixtures": rows, "exactRecordedContainersAbsent": True, "exactSessionReapersAbsent": True}))
            return
        if time.monotonic() >= deadline:
            raise RuntimeError("Owned resource remains; preserve for investigation")
        time.sleep(1)


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print("Leave custody verification failed: " + type(error).__name__, file=sys.stderr)
        sys.exit(1)
