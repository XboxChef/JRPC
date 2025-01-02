# General Improvements - Huskeyyy
- Addressed and resolved various code issues to enhance overall reliability and maintainability.

- All functions have for the most part stayed the same in functionality, but the new async methods have some slight changes too so users can use as normal.

# Asynchronous Operations
- Added asynchronous counterparts for every function in JRPC.
- Since the IXboxConsole interface relies on COM interop, native asynchronous support is a pain. To get round this, Task.Run is employed as a workaround, enabling certain methods to run on background threads. This provides better performance and responsiveness, particularly for operations that would otherwise block the main thread.

# Error handling
- Improved error handling throught the library, main with connection and sending commands.

# New Functions
XBL Connection Status Check, returns True or False

This is a work in progress and is mainly for my own use and is being released for archive purposes, feel free to submit any issues or raise a PR if you have anything you'd like to contribute.
