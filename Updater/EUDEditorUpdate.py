# We omitted cryptographic checks intentionally.
# That would be handled properly by https protocol used by GitHub

import atexit
import io
import os
import sys
import zipfile
from threading import Thread
from urllib.error import URLError
from urllib.request import urlopen

import msgbox

VERSION_URL = (
    "https://raw.githubusercontent.com/iDoodler-DS/EUDEditor/master/version/version"
)


def download(url):
    try:
        with urlopen(url) as urlf:
            return urlf.read()
    except URLError:
        return None


def getLatestVersion():
    v = download(VERSION_URL)
    if v is None:
        return (None, None)
    else:
        return (v.decode("utf-8")).split("\n")[:2]


def getRelease(url):
    return download(url)


def checkUpdate():
    # auto update only supports Win32 by now.
    # Also, the application should be frozen
    if not msgbox.isWindows or not getattr(sys, "frozen", False):
        return False

    latestVersion, release_url = getLatestVersion()
    if latestVersion is None:
        return msgbox.MessageBox(
            "Update failed", "Fail to get latest version info", textio=sys.stderr
        )

    # Download the needed data
    print("Downloading EUD Editor2 %s" % latestVersion)
    release = getRelease(release_url)
    if not release:
        return msgbox.MessageBox("Update failed", "No release", textio=sys.stderr)

    dataDir = os.path.dirname(sys.executable)
    updateDir = os.path.join(dataDir, "_update")

    with zipfile.ZipFile(io.BytesIO(release), "r") as zipf:
        zipf.extractall(updateDir)

    # Write an auto-update script. This script will run after euddraft exits
    with open(os.path.join(dataDir, "_update.bat"), "w") as batf:
        batf.write(
            """\
@echo off
xcopy _update . /e /y /q
rd _update /s /q
del _update.bat /q
"""
        )

    def onExit():
        from subprocess import Popen

        Popen("_update.bat")

    atexit.register(onExit)


def issueAutoUpdate():
    checkUpdateThread = Thread(target=checkUpdate)
    checkUpdateThread.start()
    # We don't join this thread. This thread will automatically join when
    # the whole euddraft process is completed.
    #
    # Also, we don't want this thread to finish before it completes its update
    # process, even if the main thread has already finished. So we don't make
    # this thread a daemon thread.


if __name__ == "__main__" or __name__ == "euddraft__main__":
    issueAutoUpdate()
