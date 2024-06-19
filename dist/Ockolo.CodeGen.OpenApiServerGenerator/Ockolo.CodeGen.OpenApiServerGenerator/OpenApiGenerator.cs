using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Ockolo.CodeGen.OpenApiServerGenerator;

[Generator]
public class OpenApiCodeGenerator : ISourceGenerator
{
    private static readonly DiagnosticDescriptor InformationalMessageDescriptor = new(
        id: "OCGINFO01", 
        title: "Informational Message", 
        messageFormat: "{0}", 
        category: "SourceGenerator", 
        defaultSeverity: DiagnosticSeverity.Info, 
        isEnabledByDefault: true);
    
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var outputPath = Path.GetTempPath() + Guid.NewGuid();
            
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                key: "build_property.ConfigOckoloCodeGenOpenApiServerGenerator",
                value: out var configPath);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                key: "build_property.SpecOckoloCodeGenOpenApiServerGenerator",
                value: out var specPath);
            
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                key: "build_property.projectDir",
                value: out var projDir);

            specPath = Path.GetFullPath(Path.Combine(projDir!, specPath!));
            configPath = Path.GetFullPath(Path.Combine(projDir, configPath!));

            Log(context, $"Config: {configPath}");
            Log(context, $"Spec: {specPath}");
            Log(context, $"Output: {outputPath}");
            
            GenerateCodeFromJava(context, configPath, specPath, outputPath);
            AddGeneratedFilesToCompilation(context, outputPath);
            
            Directory.Delete(outputPath, true);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                id: "OCGERRO01",
                title: "Source Generator Error",
                messageFormat: ex.ToString(),
                category: "SourceGenerator",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true), Location.None));
        }
    }

    private void GenerateCodeFromJava(GeneratorExecutionContext context, string configPath, string specPath, string outputPath)
    {
        var resourcePath = $"Ockolo.CodeGen.OpenApiServerGenerator.openapi-generator-cli.jar";
        
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            key: "build_property.JarOckoloCodeGenOpenApiServerGenerator",
            value: out var jarFilePath);

        var isLocalFile = true; 
        if (string.IsNullOrEmpty(jarFilePath))
        {
            isLocalFile = false;
            jarFilePath = Path.GetTempFileName();
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using var fileStream = new FileStream(jarFilePath, FileMode.Create, FileAccess.Write);
            resourceStream!.CopyTo(fileStream);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = $"-jar \"{jarFilePath}\" generate -g aspnetcore -i \"{specPath}\" -o \"{outputPath}\" -c \"{configPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo);
        
        if (process == null || !process.WaitForExit(10000))
        {
            throw new Exception("Failed to start Java process");
        }

        if (!isLocalFile)
        {
            File.Delete(jarFilePath);
        }

        using var reader = process.StandardOutput;
        using var errorReader = process.StandardError;

        var result = reader.ReadToEnd();
        Log(context, $"Java Output: {result}");

        var error = errorReader.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }
    }

    private static void AddGeneratedFilesToCompilation(GeneratorExecutionContext context, string outputPath)
    {
        foreach (var file in Directory.EnumerateFiles(outputPath, "*.cs", SearchOption.AllDirectories))
        {
            context.AddSource(
                hintName: Path.GetFileNameWithoutExtension(file) +".g.cs",
                source: File.ReadAllText(file));
        }
    }

    private static void Log(GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: InformationalMessageDescriptor,
            location: Location.None,
            messageArgs: message));
    }
}
