"""A euddraft plugin that writes down what eudplib offers.

euddraft runs this inside its own Python, with its own eudplib, so what comes
out describes the version the user actually builds with. eudplib itself ships
compiled for a Python the machine may not have, so asking euddraft is the only
way to read it that stays true when euddraft updates itself.

eudplib_symbols.py puts this in a folder of its own and runs euddraft on it.
"""

from eudplib import *

import eudplib
import inspect
import io
import json
import os

# euddraft runs a plugin without giving it a __file__, so the place to write to
# comes from where euddraft was started.
OUT = os.path.join(os.getcwd(), "eudplib_signatures.json")


def describe(obj):
    entry = {"kind": type(obj).__name__}
    try:
        sig = inspect.signature(obj)
    except (ValueError, TypeError) as why:
        entry["params"] = None
        entry["why"] = str(why)
    else:
        entry["params"] = [
            {
                "name": p.name,
                "kind": str(p.kind),
                "default": None if p.default is inspect.Parameter.empty else repr(p.default),
                "annotation": None if p.annotation is inspect.Parameter.empty else str(p.annotation),
            }
            for p in sig.parameters.values()
        ]
    doc = inspect.getdoc(obj)
    if doc:
        entry["doc"] = doc.splitlines()[0][:200]
    return entry


found = {}
for name in dir(eudplib):
    if name.startswith("_"):
        continue
    obj = getattr(eudplib, name)
    if callable(obj):
        found[name] = describe(obj)

io.open(OUT, "w", encoding="utf-8").write(json.dumps(found, indent=1, sort_keys=True))
ep_warn("wrote %d names to %s" % (len(found), OUT))
