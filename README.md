# Paket Sorter

[![Build status](https://ci.appveyor.com/api/projects/status/t3y8umshr09nwj82/branch/master?svg=true)](https://ci.appveyor.com/project/BlythMeister/paketsorter/branch/master)
[![Release](https://img.shields.io/github/release/BlythMeister/PaketSorter.svg?style=flat)](https://github.com/BlythMeister/PaketSorter/releases/latest)
[![Issues](https://img.shields.io/github/issues/BlythMeister/PaketSorter.svg?style=flat)](https://github.com/BlythMeister/PaketSorter/issues)

A command line application to update, sort, simplify & install paket dependencies

# Installation

* Unzip the release
* Add folder you unzipped into into your PATH

# Usages

`PaketSorter` - Will run the sorter for the directory your currently in.

`PaketSorter --auto-close` or `PaketSorter -ac` - Will run the sorter for the directory your currently in and then close the app after running.

`PaketSorter --dir <Path>` or `PaketSorter -d <Path>` - Will run the sorter for the directory specified, this can also be coupled with the `--auto-close` or `-ac` argument.

`PaketSorter -?` or `PaketSorter -h` or `PakertSorter -help` - Will show the help

# 3rd Party Libraries

* [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)
* [Paket](https://github.com/fsprojects/Paket)