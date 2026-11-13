#!/usr/bin/env python3
"""Generate/check the backend's required online release from the game sources."""
import argparse
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def expected():
    resources = ROOT / "Assets/Frontier/Resources"
    text = (resources / "catalog.json").read_text()
    for path in sorted((resources / "Maps").glob("*.json"), key=lambda p: p.stem):
        text += "\n" + path.stem + "\n" + path.read_text()
    version = re.search(r"^  bundleVersion: (\S+)$", (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(), re.M)[1]
    protocol = re.search(r"const int Version = (\d+)", (ROOT / "Assets/Frontier/Networking/MatchProtocol.cs").read_text())[1]
    rules = re.search(r"ReplayRulesVersion\s*=\s*(\d+)", (ROOT / "Assets/Frontier/Core/MatchRecording.cs").read_text())[1]
    return {"version": version, "protocol": int(protocol), "rules": int(rules),
            "contentHash": hashlib.sha256(text.encode()).hexdigest().upper()}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    path = ROOT / "backend/nakama/release.json"
    release = expected()
    if args.check:
        if not path.exists() or json.loads(path.read_text()) != release:
            raise SystemExit("Online release is stale. Run python3 scripts/online-release.py and rebuild Nakama/client/server together.")
    else:
        path.write_text(json.dumps(release, indent=2) + "\n")
    print("Online release " + release["version"] + " verified: " + str(release["protocol"]) + ":" + str(release["rules"]) + ":" + release["contentHash"])
