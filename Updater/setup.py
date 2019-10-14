import sys

from cx_Freeze import Executable, setup

if "build_exe" not in sys.argv:
    sys.argv.append("build_exe")


build_exe_options = {
    "packages": ["os", "ctypes", "sys"],
    "excludes": ["tkinter"],
    "optimize": 2,
    "include_msvcr": True,
    "include_files": [],
    "zip_include_packages": "*",
    "zip_exclude_packages": "",
    "small_app": True,
}


setup(
    name="EUD Editor Updater",
    version="1.0.0",
    description="EUD Editor updating system",
    options={"build_exe": build_exe_options},
    executables=[Executable("EUDEditorUpdate.py", base="Console")],
)
