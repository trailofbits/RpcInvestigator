//
// Copyright (c) 2023-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace RpcInvestigator.Util
{
    public static class ClientCompiler
    {
        public static Assembly Compile(string Source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(Source);
            string assemblyName = Path.GetRandomFileName();
            var basePath = Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location);

            var refPaths = new[] {
                typeof(System.Object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                "NtApiDotNet.dll",
                Path.Combine(basePath, "netstandard.dll"),
                Path.Combine(basePath, "system.collections.dll"),
                Path.Combine(basePath, "system.runtime.dll")
            };
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    StringBuilder errors = new StringBuilder();
                    foreach (var e in result.Diagnostics)
                    {
                        errors.AppendLine(e.GetMessage());
                    }
                    throw new Exception("Compiler errors:" + errors.ToString());
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    return AssemblyLoadContext.Default.LoadFromStream(ms);
                }
            }
        }

        public static string GetBaseClientCode(RpcServer Server)
        {
            //
            // TODO: It would be nice if RpcClientBuilder exposed its internal
            // routines for different parts of the source code creation process,
            // such that we could add method definitions to the resulting Client
            // class prior to source code generation.
            // As is stands, all those things are marked private and we can only
            // either get back an immutable assembly object or the finalized source
            // code as text, neither of which are ideal for modifying the code.
            // So this becomes a text search problem.
            //
            RpcClientBuilderArguments args = new RpcClientBuilderArguments();
            args.Flags = RpcClientBuilderFlags.UnsignedChar |
                RpcClientBuilderFlags.NoNamespace |
                RpcClientBuilderFlags.ExcludeVariableSourceText;
            args.NamespaceName = "RpcInvestigator";
            args.ClientName = "Client1";
            var src = RpcClientBuilder.BuildSource(Server, args);
            src = "using System;" + Environment.NewLine +
                "using System.Threading.Tasks;" + Environment.NewLine +
                "using NtApiDotNet.Win32;" + Environment.NewLine +
                "using NtApiDotNet;" + Environment.NewLine +
                "using NtApiDotNet.Ndr.Marshal;" + Environment.NewLine +
                src;
            int loc = src.IndexOf("public Client1()");
            src = src.Insert(loc,
                "public async Task<bool> Run()" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            return true;" + Environment.NewLine +
                "        }" + Environment.NewLine +
                "        ");
            return src;

        }
    }
}
