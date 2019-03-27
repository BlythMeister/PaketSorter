# Paket Sorter

[![Build status](https://ci.appveyor.com/api/projects/status/t3y8umshr09nwj82/branch/master?svg=true)](https://ci.appveyor.com/project/BlythMeister/paketsorter/branch/master)
[![Release](https://img.shields.io/github/release/BlythMeister/PaketSorter.svg?style=flat)](https://github.com/BlythMeister/PaketSorter/releases/latest)
[![Issues](https://img.shields.io/github/issues/BlythMeister/PaketSorter.svg?style=flat)](https://github.com/BlythMeister/PaketSorter/issues)

A command line application to update, sort, simplify & install paket dependencies

# Installation

* Unzip the release
* Add folder you unzipped into into your PATH

# Usages

```
Usage: PaketSorter [options]

Options:
  -d|--dir <PATH>             The path to a root of a repository (Note: <PATH> should be in quotes)
  -ua|--update-args <ARGS>    Args to pass to paket update (Note: <ARGS> should be in quotes)
  -ia|--install-args <ARGS>   Args to pass to paket install (Note: <ARGS> should be in quotes)
  -sa|--simplify-args <ARGS>  Args to pass to paket simplify (Note: <ARGS> should be in quotes)
  -s|--simplify               Include a paket simplify
  -u|--update                 Include a paket update
  -np|--no-prompt             Never prompt user input
  -?|-h|--help                Show help information
```

# 3rd Party Libraries

* [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)
* [Paket](https://github.com/fsprojects/Paket)
