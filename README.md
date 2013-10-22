jCryptionNET
============

Server side code for jCryption (www.jcryption.org) jQuery plugin implmented using C# and openSSL.NET

Introduction
------------

jCryption is a great jQuery plug-in, created by Daniel Griesser, that allows one to encrypt data using JavaScript prior to submission to the server.
The library and sample implementation provided by the author was originally coded in PHP.
I needed the functionality to operate in a .NET application, and so, I "ported" the PHP implementation to C#.

To Do -- Before Running
-----------------------

1. Download OpenSSL.NET (http://openssl-net.sourceforge.net/) and ensure the reference to
`ManagedOpenSsl.dll` is in tact. Be sure to include `libeay32.dll` and `ssleay32.dll` in your application directory as well.
2. Use openSSL to generate your public and private keys for the server
3. That should be it!

Caveats
-------

The PHP version uses session state facilities on the server side to "cache" the client's AES key.
Since the (PaaS) server that is running the example does not provide in-memory storage,
I've used an (encrypted) cookie in the example to store the client key.
This is definitely not the most secure, or efficient way to store the client key, but it works when
you don't have distributed caching facilities.


  