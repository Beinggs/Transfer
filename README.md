# Beinggs Transfer Utility

## Overview

This command line utility, `bx`, transfers files or test data between two machines. It was built with .NET 7 and
is a simple example of sending and receiving data at the TCP level using the following .NET 7 classes:

- [`TcpClient` and `TcpListener`](https://learn.microsoft.com/dotnet/fundamentals/networking/sockets/tcp-classes)
- [`ArrayPool<T>`](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)
- [`Memory<T>`](https://learn.microsoft.com/dotnet/api/system.memory-1)

This utility also serves as a learning exercise for
[`System.CommandLine`](https://learn.microsoft.com/dotnet/standard/commandline/).

It includes an instrumeted version of
[`Stream.CopyToAsync()`](https://learn.microsoft.com/dotnet/api/system.io.stream.copytoasync), called
[`InstrumentedCopyToAsync()`](https://github.com/FuzzyPhilip/Transfer/blob/main/Extensions/StreamExtensions.cs#L30),
which tracks the read time and write time of the source and destination streams respectively, using
[`Stopwatch`](https://learn.microsoft.com/dotnet/api/system.diagnostics.stopwatch) timers for the best possible
accuracy.

## Example output

``` shell
Total of 7.741 GB read in 6.174 sec @ 10.03 Gbps, written in 4.985 sec @ 12.42 Gbps into TEST.Windows.22H2.iso
```

## Releases

The following builds can be found under Releases:

- Windows x64
- Linux x64
- OSX Arm64 (currently untested but presumed working because .NET is awesomely multi-platform!)

## Syntax

Example syntax:

``` shell
bx [--measured[=true]] send file {fileName} [--include-filename[=true]] to anyone [--repeat=false]
bx [--measured[=true]] send [test] to clientMachine [--size=[10] [--repeat[=false]]
bx [--measured[=true]] receive file [{fileName}] from serverMachine
bx [--measured[=true]] receive [test] [--max-size=[0]] from serverMachine
```

*Where:*

- {fileName} is the name of a file to send from or receive into
- `[]` indicates an optional command-line option
- `[=value]` indicates the default value for a command-line option

### Options

This section provides an overview of options; run `bx -h` or `bx {command} -h` to see more details.

#### Global Options

`-h`, `/h`, `-?`, `/?`, `--help`: Shows global or command-specific help\
`-v`, `--verbosity`: Specifies the level of detail output to the console\
`-m`, `/m`, `--measured`: Writes performance and timing stats to the console\
`-p`, `/p`, `--port`: Specifies the port to use for sending or receiving

#### Send Options

`-r`, `/r`, `--repeat`: Repeats file sending (by default the utility quits after sending the file or test data)\
`-f`, `/f`, `--include-filename`: Includes the filename as a 'header' at the start of the file data (see below)\
`-s`, `/s`, `--size`: Specifies the size of test data to send in MB, defaulting to 10, up to 10240 (10 GB)

#### Receive Options

`-m`, `/m`, `--max-size`: Specifies the maximum size of data to recieve, in MB\
`-m`, `/m`, `--max-time`: Specifies the maximum time to wait for data to be received, in seconds\
`--version`: Shows the version number

## Operation

Use `-h` or `/h` to get help for specific commands, e.g.:

`bx -h` shows general help\
`bx send -h` shows help for the send command, listing its sub-commands and applicable options\
`bx receive test -h` shows help for receiving test data and applicable options
etc.

### Note

- The `test` command is implicit, so `bx send to anyone` will send test data.

- A `send` command will wait for a connection and then send the specified file or test data once a
valid client has connected.

- A recipient can be a specific client machine name or IP address, or the name `anyone` (aliases:
`any` or `a`), in which case connects are accepted from any client.

- The `--include-filename` option for the `send file` command, which is on by default, prepends the
'header' string `filename:name\n`, where `name` is the file's name, which is terminiated with a
newline. The `filename` header uses UTF8 encoding.

- The `receive file` command looks for the `filename` header in the incoming file data and
automatically creates the file (if it doesn't already exist) with that name. If the incoming file
data doesn't start with the `filename` header the received data will be saved in the file named
`bx.dat` in the current directory.
