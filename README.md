# C# Syntax Validator

CSharpSyntaxValidator is a [.NET Core CLI tool][dotnet-tool] that validates a
C# source for syntax errors and sets its exit code to zero (0) if the C#
source is valid and one (1) if invalid.

It is not a [linter] for C#.


## Installation

You will need .NET Core SDK 2.2 or later installed.

Install [CSharpSyntaxValidator][csval] as follows:

    dotnet tool install --global CSharpSyntaxValidator


## Usage

Simply invoke supplying C# source on standard input:

    csval < Program.cs && echo OK

If the source is valid, the above will print `OK` on your terminal.

Alternatively, supply the file name directly:

    csval Program.cs && echo OK

For more help on usage, run with the `-h` option:

    csval -h


## Building

The .NET Core SDK is the minimum requirement.

To build just the binaries on Windows, run:

    .\build.cmd

On Linux or macOS, run instead:

    ./build.sh

To build the binaries and the NuGet package on Windows, run:

    .\pack.cmd

On Linux or macOS, run instead:

    ./pack.sh


[csval]: https://www.nuget.org/packages/CSharpSyntaxValidator/
[dotnet-tool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
[linter]: https://en.wikipedia.org/wiki/Lint_(software)
