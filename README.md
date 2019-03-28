# Paket Sorter

[![Build status](https://ci.appveyor.com/api/projects/status/t3y8umshr09nwj82/branch/master?svg=true)](https://ci.appveyor.com/project/BlythMeister/paketsorter/branch/master)
[![Release](https://img.shields.io/github/release/BlythMeister/PaketSorter.svg?style=flat)](https://github.com/BlythMeister/PaketSorter/releases/latest)
[![Issues](https://img.shields.io/github/issues/BlythMeister/PaketSorter.svg?style=flat)](https://github.com/BlythMeister/PaketSorter/issues)

A command line application call paket in standard folder structures and chain multiple commands together.

With added ability to sort dependencies and references files alphabetically prior to install.

# Installation

* Download latest release Zip from https://github.com/BlythMeister/PaketSorter/releases/latest
* Unzip the application into a folder on your machine
* Add the folder you unzipped paket sorter into to your PATH variable
* In any directory call `PaketSorter` as below

# Usage

```
Usage: PaketSorter [options]

Options:
  -d|--dir <PATH>             The path to a root of a repository, defaults to current directory if not provided (Note: <PATH> should be in quotes)
  -ua|--update-args <ARGS>    Args to pass to paket update (Note: <ARGS> should be in quotes)
  -ia|--install-args <ARGS>   Args to pass to paket install (Note: <ARGS> should be in quotes)
  -sa|--simplify-args <ARGS>  Args to pass to paket simplify (Note: <ARGS> should be in quotes)
  -cc|--clear-cache           Clear caches before running
  -s|--simplify               Include a paket simplify
  -u|--update                 Include a paket update
  -np|--no-prompt             Never prompt user input
  -?|-h|--help                Show help information
```

# 3rd Party Libraries

* [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)
* [Paket](https://github.com/fsprojects/Paket)
