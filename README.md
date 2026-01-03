# C# Syntax Validator

CSharpSyntaxValidator is a [.NET CLI tool] that validates a
C# source for syntax errors and sets its exit code to zero (0) if the C#
source is valid and one (1) if invalid.

It is not a [linter] for C#.


## Installation

You will need .NET SDK 10 or later installed.

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

### Development Usage

CSharpSyntaxValidator is a [file-based application], so you can also run it
using `dotnet` as follows:

    dotnet CSharpSyntaxValidator.cs < Program.cs && echo OK

On Unix-like systems, `./CSharpSyntaxValidator.cs` can also be invoked directly as an executable:

    ./CSharpSyntaxValidator.cs < Program.cs && echo OK

For help, run:

    dotnet CSharpSyntaxValidator.cs -- -h


## Building

Since CSharpSyntaxValidator is a [file-based application], an explicit build
step is not necessary for usage, but the .NET SDK is the minimum requirement.
However, you can package it as a [.NET CLI tool] using:

    dotnet pack CSharpSyntaxValidator.cs

Yu can also publish it as a self-contained application that does not require a .NET runtime installation using:

    dotnet publish CSharpSyntaxValidator.cs


[csval]: https://www.nuget.org/packages/CSharpSyntaxValidator/
[.NET CLI tool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
[linter]: https://en.wikipedia.org/wiki/Lint_(software)
[file-based application]: https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps
