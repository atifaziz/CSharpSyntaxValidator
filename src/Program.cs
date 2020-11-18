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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Mono.Options;

var verbose = false;

try
{
    return Wain(args);
}
catch (Exception e)
{
    Console.Error.WriteLine(verbose ? e.ToString()
                                    : e.GetBaseException().Message);
    return 0xbad;
}

int Wain(string[] args)
{
    var symbols = new List<string>();
    var help = false;
    var quiet = false;
    var languageVersion = LanguageVersion.Default;
    var listLanguageVersions = false;
    var kind = SourceCodeKind.Regular;

    var options = new OptionSet
    {
        { "h|?|help", "display this help",
            _ => help = true },

        { "v|verbose", "enable verbose output",
            _ => verbose = true },

        { "debug", "break into debugger on start",
            _ => Debugger.Launch() },

        { "q|quiet", "suppress printing of syntax errors",
            _ => quiet = true },

        { "langversion=",
            @"use C# language version syntax rules where {VERSION} " +
            @"is ""default"" to mean latest major version, " +
            @"or ""latest"" to mean latest version, including minor versions, " +
            @"or a specific version like ""6"" or ""7.1""",
            v => languageVersion = LanguageVersionFacts.TryParse(v, out var ver) ? ver
                                : throw new Exception("Invalid C# language version specification: " + v) },

        { "langversions", "list supported C# language versions",
            _ => listLanguageVersions = true },

        { "script", "validate using scripting rules",
            _ => kind = SourceCodeKind.Script },

        { "d=|define=",
            "define {NAME} as a conditional compilation symbol; " +
            "use semi-colon (;) to define multiple symbols",
            v => symbols.AddRange(v.Split(';', StringSplitOptions.RemoveEmptyEntries)) },
    };

    var tail = options.Parse(args);

    if (help)
    {
        PrintHelp(options, Console.Out);
        return 0;
    }

    if (listLanguageVersions)
    {
        foreach (var v in (LanguageVersion[])Enum.GetValues(typeof(LanguageVersion)))
            Console.WriteLine(v.ToDisplayString());
        return 0;
    }

    var parseOptions =
        CSharpParseOptions.Default
            .WithPreprocessorSymbols(symbols)
            .WithLanguageVersion(languageVersion)
            .WithKind(kind);

    var (path, source) = tail.Count switch
    {
        0 => ("STDIN", SourceText.From(Console.In.ReadToEnd())),
        1 when tail[0] is {} p => (p, SourceText.From(File.ReadAllText(p))),
        _ => throw new Exception("Too many files specified as input when only one is allowed.")
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

static void PrintHelp(OptionSet options, TextWriter output)
{
    const string resourceName = "Help.txt";
    var assembly = Assembly.GetExecutingAssembly();
    using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream is null)
        throw new Exception("Missing help text.");
    using var reader = new StreamReader(stream);
    using var e = reader.ReadLines();
    while (e.MoveNext())
    {
        var line = e.Current;
        switch (line)
        {
            case "<LOGO>":
            {
                var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() is {} va
                            ? va.InformationalVersion
                            : throw new Exception($"Missing {nameof(AssemblyInformationalVersionAttribute)}.");
                var languageVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion().ToDisplayString();
                if (LanguageVersion.Latest.MapSpecifiedToEffectiveVersion() != LanguageVersion.Default.MapSpecifiedToEffectiveVersion())
                    languageVersion += "; latest = " + LanguageVersion.Latest.MapSpecifiedToEffectiveVersion().ToDisplayString();
                output.WriteLine($"C# Syntax Validator, v{version} (C# {languageVersion})");
                var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>() is {} ca
                                ? ca.Copyright
                                : throw new Exception($"Missing {nameof(AssemblyCopyrightAttribute)}.");
                output.WriteLine(copyright);
                break;
            }
            case "<OPTIONS>":
            {
                options.WriteOptionDescriptions(output);
                break;
            }
            case "<CSHARP-VERSION-LIST>":
            {
                var defaultVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
                var latestVersion = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
                foreach (var v in
                    from LanguageVersion v in Enum.GetValues(typeof(LanguageVersion))
                    select new
                    {
                        Display  = v.ToDisplayString(),
                        Floating = v == defaultVersion ? " (default)"
                                 : v == latestVersion  ? " (latest)"
                                 : null,
                    })
                {
                    output.WriteLine("- " + v.Display + v.Floating);
                }
                break;
            }
            default:
            {
                output.WriteLine(line);
                break;
            }
        }
    }
}

static class Extensions
{
    public static IEnumerator<string> ReadLines(this TextReader reader)
    {
        if (reader is null) throw new ArgumentNullException(nameof(reader));

        while (reader.ReadLine() is {} line)
            yield return line;
    }
}
