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
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Mono.Options;

    static class Program
    {
        static int Wain(string[] args)
        {
            var symbols = new List<string>();

            var options = new OptionSet
            {
                { "d=|define=",
                  "define {NAME} as a conditional compilation symbol; " +
                  "use semi-colon (;) to define multiple symbols",
                  v => symbols.AddRange(v.Split(';', StringSplitOptions.RemoveEmptyEntries)) },
            };

            options.Parse(args);

            var parseOptions =
                CSharpParseOptions.Default.WithPreprocessorSymbols(symbols);

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
                Console.WriteLine(d);
            }

            return result;
        }

        static int Main(string[] args)
        {
            try
            {
                return Wain(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.GetBaseException().Message);
                return 0xbad;
            }
        }
    }
}
