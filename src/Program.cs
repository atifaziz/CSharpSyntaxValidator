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
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    static class Program
    {
        static int Main()
        {
            try
            {
                var diagnostics =
                    from d in CSharpSyntaxTree
                        .ParseText(Console.In.ReadToEnd(), path: "STDIN")
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
            catch (Exception e)
            {
                Console.Error.WriteLine(e.GetBaseException().Message);
                return 0xbad;
            }
        }
    }
}
