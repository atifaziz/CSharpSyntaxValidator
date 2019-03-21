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

namespace CSharpSyntaxValidator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Mono.Options;

    static class Program
    {
        static bool _verbose;

        static int Wain(string[] args)
        {
            var symbols = new List<string>();
            var help = false;
            var quiet = false;
            var languageVersion = LanguageVersion.Default;
            var kind = SourceCodeKind.Regular;

            var options = new OptionSet
            {
                { "h|?|help", "display this help",
                   _ => help = true },

                { "v|verbose", "enable verbose output",
                   _ => _verbose = true },

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

                { "script", "validate using scripting rules",
                   _ => kind = SourceCodeKind.Script },

                { "d=|define=",
                  "define {NAME} as a conditional compilation symbol; " +
                  "use semi-colon (;) to define multiple symbols",
                  v => symbols.AddRange(v.Split(';', StringSplitOptions.RemoveEmptyEntries)) },
            };

            options.Parse(args);

            if (help)
            {
                PrintHelp(options, Console.Out);
                return 0;
            }

            var parseOptions =
                CSharpParseOptions.Default
                    .WithPreprocessorSymbols(symbols)
                    .WithLanguageVersion(languageVersion)
                    .WithKind(kind);

            var diagnostics =
                from d in CSharpSyntaxTree
                    .ParseText(Console.In.ReadToEnd(), parseOptions, "STDIN")
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

        static Assembly Assembly => typeof(Program).Assembly;

        static void PrintHelp(OptionSet options, TextWriter output)
        {
            using (var stream = GetManifestResourceStream("Help.txt", typeof(Program)))
            using (var reader = new StreamReader(stream))
            using (var e = reader.ReadLines())
            while (e.MoveNext())
            {
                var line = e.Current;
                switch (line)
                {
                    case "<LOGO>":
                        var version = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                        var languageVersion = CSharpParseOptions.Default.LanguageVersion;
                        output.WriteLine($"C# Syntax Validator, {version} (C# {languageVersion})");
                        output.WriteLine(Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);
                        break;
                    case "<OPTIONS>":
                        options.WriteOptionDescriptions(output);
                        break;
                    case "<CSHARP-VERSION-LIST>":
                        var defaultVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
                        var latestVersion = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
                        foreach (var v in
                            from LanguageVersion v in Enum.GetValues(typeof(LanguageVersion))
                            select new
                            {
                                Display = v.ToDisplayString(),
                                Floating = v == defaultVersion ? " (default)"
                                         : v == latestVersion  ? " (latest)"
                                         : null,
                            })
                        {
                            output.WriteLine("- " + v.Display + v.Floating);
                        }
                        break;
                    default:
                        output.WriteLine(line);
                        break;
                }
            }
        }

        static Stream GetManifestResourceStream(string name, Type type = null) =>
            type != null ? type.Assembly.GetManifestResourceStream(type, name)
                         : Assembly.GetCallingAssembly().GetManifestResourceStream(name);

        static IEnumerator<string> ReadLines(this TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            while (reader.ReadLine() is string line)
                yield return line;
        }

        static int Main(string[] args)
        {
            try
            {
                return Wain(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(_verbose ? e.ToString()
                                                 : e.GetBaseException().Message);
                return 0xbad;
            }
        }
    }
}
