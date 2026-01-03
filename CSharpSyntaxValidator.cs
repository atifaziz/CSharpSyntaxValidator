#!/usr/bin/env dotnet

#:property ToolCommandName=csval
#:property VersionPrefix=2.0.0
#:property Copyright=Copyright 2019 Atif Aziz. All rights reserved.
#:property Description=Utility to validate syntax of C# source.
#:property Authors=Atif Aziz
#:property PackageLicenseExpression=Apache-2.0
#:property PackageTags=csharp;syntax

#:package docopt.net@0.8.3
#:package ThisAssembly.AssemblyInfo@2.1.2
#:package Microsoft.CodeAnalysis.CSharp@5.0.0

#region Copyright (c) 2019 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Diagnostics;
using DocoptNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

var verbose = false;

AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    switch (verbose, args.ExceptionObject)
    {
        case (false, Exception e): Console.Error.WriteLine(e.GetBaseException().Message); break;
        case (_, { } e): Console.Error.WriteLine(e); break;
    }
    Environment.Exit(1);
};

return ProgramArguments.CreateParser()
                       .WithVersion(ThisAssembly.Info.Version)
                       .Parse(args)
                       .Match(Main,
                              result => PrintHelp(Console.Out, result.Help),
                              result => Print(Console.Out, result.Version),
                              result => Print(Console.Error, FormatHelp(result.Usage), exitCode: 1));

int Main(ProgramArguments args)
{
    if (args.OptDebug)
        Debugger.Launch();

    verbose = args.OptVerbose;

    if (verbose)
    {
        foreach (var arg in args)
            Console.Error.WriteLine($"{arg.Key}: {(arg.Value is { } v ? v : "(null)")}");
    }

    switch (args)
    {
        case { OptLangversions: true }:
        {
            foreach (var v in Enum.GetValues<LanguageVersion>())
                Console.WriteLine(v.ToDisplayString());
            break;
        }
        default:
        {
            var quiet = args.OptQuiet;
            var languageVersion = LanguageVersion.Default;
            var kind = args.OptScript ? SourceCodeKind.Script : SourceCodeKind.Regular;

            if (args.OptLangversion is { } ver)
            {
                if (!LanguageVersionFacts.TryParse(ver, out languageVersion))
                    return Print(Console.Error, "Invalid C# language version specification: " + ver, exitCode: 1);
            }

            var parseOptions =
                CSharpParseOptions.Default
                    .WithPreprocessorSymbols(args.OptDefine)
                    .WithLanguageVersion(languageVersion)
                    .WithKind(kind)
                    .WithFeatures(from f in args.OptFeature
                                  select KeyValuePair.Create(f, "true"));

            var (path, source) = args.ArgFile switch
            {
                null => ("STDIN", SourceText.From(Console.In.ReadToEnd())),
                var p => (p, SourceText.From(File.ReadAllText(p))),
            };

            var diagnostics =
                from d in CSharpSyntaxTree.ParseText(source, parseOptions, path)
                                        .GetDiagnostics()
                where d.Severity == DiagnosticSeverity.Error
                select d;

            var result = 0;
            foreach (var d in diagnostics)
            {
                result = 1;
                if (quiet)
                    break;
                Console.WriteLine(d);
            }

            return result;
        }
    }

    return 0;
}

static string FormatHelp(string text) =>
    text.Replace(ProgramArguments.Bin,
                 AppContext.GetData("EntryPointFilePath") switch
                 {
                     string p => $"dotnet {Path.GetFileName(p)}", // file-based app
                     _ => Environment.ProcessPath // published app
                          ?? ProgramArguments.Bin
                 },
                 StringComparison.OrdinalIgnoreCase);

static IEnumerable<(string Display, string? Floating)> GetSupportedLanguageVersions()
{
    var @default = LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
    var latest = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
    return from v in Enum.GetValues<LanguageVersion>()
           select (v.ToDisplayString(), v == @default ? "(default)" : v == latest ? "(latest)" : null);
}

static int PrintHelp(TextWriter writer, string helpText)
{
    var languageVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion().ToDisplayString();
    if (LanguageVersion.Latest.MapSpecifiedToEffectiveVersion() != LanguageVersion.Default.MapSpecifiedToEffectiveVersion())
        languageVersion += "; latest = " + LanguageVersion.Latest.MapSpecifiedToEffectiveVersion().ToDisplayString();
    writer.WriteLine($"C# Syntax Validator, v{ThisAssembly.Info.InformationalVersion} (C# {languageVersion})");
    writer.WriteLine(ThisAssembly.Info.Copyright);
    writer.WriteLine();
    writer.WriteLine(FormatHelp(helpText));
    writer.WriteLine("C# language versions supported are:");
    writer.WriteLine();
    foreach (var v in GetSupportedLanguageVersions())
        writer.WriteLine($"- {v.Display}{(v.Floating is { } f ? " " + f : "")}");
    writer.WriteLine();
    writer.Write(License.Text);
    return 0;
}

static int Print(TextWriter writer, string message, int exitCode = 0)
{
    writer.WriteLine(message);
    return exitCode;
}

[DocoptArguments]
sealed partial class ProgramArguments
{
    public const string Bin = "BIN";

    const string Help = $"""
        Validates C# syntax and returns a non-zero exit code if the supplied C#
        source contains syntax errors.

        Usage:
          {Bin} [--debug] [-v] --langversions
          {Bin} [--debug] [-vq] [--langversion VERSION] [--script]
                [-d NAME...] [-f NAME...] [FILE]
          {Bin} --help
          {Bin} --version

        If FILE is not supplied then C# source is read from the standard input
        (STDIN) stream.

        Options:
          -h, --help                 display this help
          -v, --verbose              enable verbose output
              --debug                break into debugger on start
          -q, --quiet                suppress printing of syntax errors
              --langversion VERSION  use C# language version syntax rules where
                                       VERSION is "default" to mean latest major
                                       version, or "latest" to mean latest version,
                                       including minor versions, or a specific version
                                       like "6" or "7.1"
              --langversions         list supported C# language versions
              --script               validate using scripting rules
          -f, --feature NAME         enable feature NAME;
                                       may be specified multiple times
          -d, --define NAME          define NAME as a conditional compilation symbol;
                                       may be specified multiple times

        """;
}

static class License
{
    public const string Text = """
        Licensed under the Apache License, Version 2.0 (the "License");
        you may not use this file except in compliance with the License.
        You may obtain a copy of the License at

            http://www.apache.org/licenses/LICENSE-2.0

        Unless required by applicable law or agreed to in writing, software
        distributed under the License is distributed on an "AS IS" BASIS,
        WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
        See the License for the specific language governing permissions and
        limitations under the License.

        """;
}
