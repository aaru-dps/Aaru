using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aaru.Generators;

[Generator]
public class PluginRegisterGenerator : ISourceGenerator
{
    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context) =>

        // Nothing to do
        context.RegisterForSyntaxNotifications(() => new PluginFinder());

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        /*
        #if DEBUG
            if(!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        #endif
        */

        ClassDeclarationSyntax pluginRegister = ((PluginFinder)context.SyntaxReceiver)?.Register;

        if(pluginRegister == null)
            return;

        string @namespace =
            (pluginRegister.Ancestors().FirstOrDefault(x => x is FileScopedNamespaceDeclarationSyntax) as
                 FileScopedNamespaceDeclarationSyntax)?.Name.ToString();

        @namespace ??=
            (pluginRegister.Ancestors().FirstOrDefault(x => x is NamespaceDeclarationSyntax) as
                 NamespaceDeclarationSyntax)?.ToString();

        string className = pluginRegister.Identifier.Text;

        List<ClassDeclarationSyntax> archives    = ((PluginFinder)context.SyntaxReceiver)?.Archives;
        List<ClassDeclarationSyntax> checksums   = ((PluginFinder)context.SyntaxReceiver)?.Checksums;
        List<ClassDeclarationSyntax> fileSystems = ((PluginFinder)context.SyntaxReceiver)?.FileSystems;
        List<ClassDeclarationSyntax> filters     = ((PluginFinder)context.SyntaxReceiver)?.Filters;

        List<ClassDeclarationSyntax> floppyImagePlugins = ((PluginFinder)context.SyntaxReceiver)?.FloppyImagePlugins;

        List<ClassDeclarationSyntax> partitionPlugins = ((PluginFinder)context.SyntaxReceiver)?.PartitionPlugins;

        List<ClassDeclarationSyntax> mediaImagePlugins = ((PluginFinder)context.SyntaxReceiver)?.MediaImagePlugins;

        List<ClassDeclarationSyntax> readOnlyFileSystems = ((PluginFinder)context.SyntaxReceiver)?.ReadOnlyFileSystems;

        List<ClassDeclarationSyntax> writableFloppyImagePlugins =
            ((PluginFinder)context.SyntaxReceiver)?.WritableFloppyImagePlugins;

        List<ClassDeclarationSyntax> writableImagePlugins =
            ((PluginFinder)context.SyntaxReceiver)?.WritableImagePlugins;

        List<ClassDeclarationSyntax> byteAddressableImagePlugins =
            ((PluginFinder)context.SyntaxReceiver)?.ByteAddressableImagePlugins;

        StringBuilder sb = new();

        sb.AppendLine(@"// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Register.g.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Registers all plugins in this assembly.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     ""Software""), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/");

        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Aaru.CommonTypes.Interfaces;");
        sb.AppendLine();
        sb.AppendLine($"namespace {@namespace};");
        sb.AppendLine();
        sb.AppendLine($"public sealed partial class {className} : IPluginRegister");
        sb.AppendLine("{");

        if(archives?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllArchivePlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in archives)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllArchivePlugins() => null;");

        sb.AppendLine();

        if(checksums?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllChecksumPlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in checksums)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllChecksumPlugins() => null;");

        sb.AppendLine();

        if(fileSystems?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllFilesystemPlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in fileSystems)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllFilesystemPlugins() => null;");

        sb.AppendLine();

        if(filters?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllFilterPlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in filters)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllFilterPlugins() => null;");

        sb.AppendLine();

        if(floppyImagePlugins?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllFloppyImagePlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in floppyImagePlugins)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllFloppyImagePlugins() => null;");

        sb.AppendLine();

        if(mediaImagePlugins?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllMediaImagePlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in mediaImagePlugins)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllMediaImagePlugins() => null;");

        sb.AppendLine();

        if(partitionPlugins?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllPartitionPlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in partitionPlugins)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllPartitionPlugins() => null;");

        sb.AppendLine();

        if(readOnlyFileSystems?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllReadOnlyFilesystemPlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in readOnlyFileSystems)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllReadOnlyFilesystemPlugins() => null;");

        sb.AppendLine();

        if(writableFloppyImagePlugins?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllWritableFloppyImagePlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in writableFloppyImagePlugins)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllWritableFloppyImagePlugins() => null;");

        sb.AppendLine();

        if(writableImagePlugins?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllWritableImagePlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in writableImagePlugins)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllWritableImagePlugins() => null;");

        sb.AppendLine();

        if(byteAddressableImagePlugins?.Count > 0)
        {
            sb.AppendLine("    public List<Type> GetAllByteAddressablePlugins() => new()");
            sb.AppendLine("    {");

            foreach(ClassDeclarationSyntax plugin in byteAddressableImagePlugins)
                sb.AppendLine($"        typeof({plugin.Identifier.Text}),");

            sb.AppendLine("    };");
        }
        else
            sb.AppendLine("    public List<Type> GetAllByteAddressablePlugins() => null;");

        sb.AppendLine("}");

        context.AddSource("Register.g.cs", sb.ToString());
    }

    sealed class PluginFinder : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Archives                    { get; } = new();
        public List<ClassDeclarationSyntax> Checksums                   { get; } = new();
        public List<ClassDeclarationSyntax> FileSystems                 { get; } = new();
        public List<ClassDeclarationSyntax> Filters                     { get; } = new();
        public List<ClassDeclarationSyntax> FloppyImagePlugins          { get; } = new();
        public List<ClassDeclarationSyntax> MediaImagePlugins           { get; } = new();
        public List<ClassDeclarationSyntax> PartitionPlugins            { get; } = new();
        public List<ClassDeclarationSyntax> ReadOnlyFileSystems         { get; } = new();
        public List<ClassDeclarationSyntax> WritableFloppyImagePlugins  { get; } = new();
        public List<ClassDeclarationSyntax> WritableImagePlugins        { get; } = new();
        public List<ClassDeclarationSyntax> ByteAddressableImagePlugins { get; } = new();
        public ClassDeclarationSyntax       Register                    { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is not ClassDeclarationSyntax plugin)
                return;

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IPluginRegister") == true)
                Register = plugin;

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IArchive") == true)
                Archives.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IChecksum") == true)
                Checksums.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IFilesystem") == true)
                FileSystems.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IFilter") == true)
                Filters.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IFloppyImage") == true)
                FloppyImagePlugins.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText is "IMediaImage" or "IOpticalMediaImage" or "IFloppyImage"
                                               or "ITapeImage") == true)
                MediaImagePlugins.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IPartition") == true)
                PartitionPlugins.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IReadOnlyFilesystem") == true)
                ReadOnlyFileSystems.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IWritableFloppyImage") == true)
                WritableFloppyImagePlugins.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText is "IWritableImage" or "IWritableOpticalImage"
                                               or "IWritableTapeImage" or "IByteAddressableImage") == true)
                WritableImagePlugins.Add(plugin);

            if(plugin.BaseList?.Types.Any(t => ((t as SimpleBaseTypeSyntax)?.Type as IdentifierNameSyntax)?.Identifier.
                                               ValueText == "IByteAddressableImage") == true)
                ByteAddressableImagePlugins.Add(plugin);
        }
    }
}