KeyValium

KeyValium is a key-value store that stores data in a recursive B+-tree. Data is stored as byte arrays. Conceptually based on Lmdb.

Features

Recursive B+-Tree 
    every key can be the root of another B+-tree

Exclusive or shared access
    multiple processes on multiple computers
    works on networkdrive 

Transactions
    One writer
    Multiple readers
    Unlimited Nesting

Stream Support
    Values greater then Array.MaxLength are supported via Stream interface
    
Limits
    Maximum key size depends on page size (usually pagesize/4)

Count support
    Every tree and subtree keeps a local and a global count of keys

Indexed access
    Optional indexed access to the keys (not implemented yet)

Frontends
    Persistent Dictionary
    more possible
    
Encryption
    Database can be encrypted

Inspector
    Filemap
    Page statistics
    annotated Hexview

Tools
    coming soon
    
