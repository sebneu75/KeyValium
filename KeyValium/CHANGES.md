# KeyValium Change Log

## v0.6.0 (2024-11-17)

- the sharing modes SharedNetwork and SharedLocal seem to work now (only available on Windows)
- fixed a bug in SharedPageProvider that could cause database corruption (cached pages where considered valid for too long)
- reworked the locking mechanisms for shared access (SharedLocal and SharedNetwork)
- PageAllocator releases the pages in Dispose
- PageAllocator.Dispose does additional checks (RefCount must be zero, checks that all allocated pages are deallocated)
- fixed locking mistakes in Transaction (order of nested locks)
- new method Transaction.CompactFreespace() to merge adjacent freespace entries.
- added DotNet 9 to target frameworks
- minor improvements


## v0.5.6 (2024-09-27)

- new option FillCache that fills the cache after opening a database
- Performance improvements in Inspector HexView

 
## v0.5.4 (2024-03-09)

- SharingModes.SharedNetwork has been removed because of potential database corruption. 
  (There are cases where two computers see different file contents. Even manually opening the files in explorer will show differences.)
- Small improvements


## v0.5.3 (2024-01-04)

- Added page validations.


## v0.5.2 (2023-12-30)

- updated documentation


## v0.5.1 (2023-12-28)

- minor bug fixes
- updated access modifiers for classes and methods
- updated documentation


## v0.5.0 (2023-12-24)

- Initial release
