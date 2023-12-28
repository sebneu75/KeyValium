# KeyValium - A key-value-store for DotNet

KeyValium is a very fast key-value store for DotNet (currently DotNet 7 and 8 is supported). All data is stored in a recursive B+-tree as byte arrays.
A frontend is included that implements the IDictionary interface and allows multiple dictionaries in a single database file.

There are no dependencies.

## Features

* **Recursive B+-Tree:** Data is stored in a single file. Every key can be the root of another B+-tree.
* **Supported Pagesizes:** The following pagesizes are supported: 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536. 
Maximum key size depends on the page size (usually 1/4 of it).
* **Transactions:** One writer, multiple readers, nested transactions.
* **Stream Support:** Values greater then 2 GB are supported via stream interface.
* **Multiple sharing modes:** 
    + **Exclusive**: Only one instance can be opened.
    + **SharedLocal**: Multiple instances can be opened on the same machine.
    + **SharedNetwork**: Multiple instances can be opened on different machines.
* **Count support:** Every tree keeps a local and a total count of keys.
* **Frontends:** A MultiDictionary which manages multiple persistent dictionaries in one database file. More frontends are possible.
* **Encryption:** The database can be encrypted with AES via password and/or a keyfile.


# Documentation

See the Wiki for documentation.

# Usage

TODO

# Changelog

See the release notes.

# License

[Apache 2.0](https://opensource.org/license/apache-2-0)






