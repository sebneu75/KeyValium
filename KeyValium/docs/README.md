# KeyValium

KeyValium is a very fast key-value store for DotNet (currently DotNet 7 and 8 is supported). All data is stored in a recursive B+-tree as byte arrays.
A frontend is included that implements the IDictionary interface and allows multiple dictionaries in a single database file.

There are no dependencies.

## Features

### Recursive B+-Tree 
Every key can be the root of another B+-tree.

### Multiple sharing modes
* **Exclusive**: The database is opened exclusively. Subsequent attempts to open the database will fail.
* **SharedLocal**: The database is opened in shared mode. Access is managed using a lockfile and a mutex. Subsequent attempts to open the database from a different machine will fail.
* **Shared**: The database is opened in shared mode. Access is managed using two lockfiles. The database can be used from multiple computers on a network share.

### Transactions
* One writer
* Multiple readers
* Support for nested transactions

### Stream Support
Values greater then 2 GB are supported via stream interface.
    
### Limits
Maximum key size depends on page size (usually 1/4 if the page size).

### Count support
Every tree and subtree keeps a local and a total count of keys.

### Indexed access
Optional indexed access to the keys (not implemented yet).

### Frontends
A MultiDictionary which manages multiple persistent dictionaries in one database file.
    
### Encryption
The database can be encrypted via password and/or a keyfile.

### Tools
coming soon...

    
