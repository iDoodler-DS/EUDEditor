"""A euddraft plugin that writes down the constants eudplib names.

The editor's drop-downs hold a place in a list; epScript would rather be
written the way a person writes it, with `Player1` and `AtLeast` instead of a
1 and a 0. eudplib names those constants itself, so the names come from asking
the euddraft that is installed, the same way the signatures do.

For each name that stands for a number, this writes the number and the type
eudplib gives it, so the editor can line a list up against it.

eudplib_constants.py puts this in a folder of its own and runs euddraft on it.
"""

from eudplib import *

import eudplib
import inspect
import io
import json
import os

OUT = os.path.join(os.getcwd(), "eudplib_constants.json")

found = {}
for name in dir(eudplib):
    if name.startswith("_"):
        continue
    obj = getattr(eudplib, name, None)
    if inspect.isclass(obj) or inspect.isroutine(obj):
        continue

    # A constant is a ConstType. Nothing else in eudplib stands for one of
    # these numbers, so the family it belongs to is its own type.
    family = [c.__name__ for c in type(obj).__mro__]
    if "ConstType" not in family:
        continue

    try:
        value = obj.getValue()
    except Exception:
        continue
    if not isinstance(value, int) or isinstance(value, bool):
        continue

    # eudplib gives several names to the same constant (P1 is Player1). What it
    # prints as is the one it calls its own, and the one to write.
    found[name] = {
        "type": type(obj).__name__,
        "family": [c for c in family if c not in ("Generic", "object")],
        "prints": repr(obj),
        "value": value,
    }

io.open(OUT, "w", encoding="utf-8").write(json.dumps(found, indent=1, sort_keys=True))
ep_warn("wrote %d constants to %s" % (len(found), OUT))
