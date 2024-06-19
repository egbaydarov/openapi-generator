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

    private static readonly DiagnosticDescriptor ErrorDescriptor = new(
        id: "OCGERRO01",
        title: "Source Generator Error",
        messageFormat: "{0}",
        category: "SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var (configPath, specPath, projDir) = GetPaths(context);
                
            if (projDir == null || specPath == null || configPath == null)
            {
                ReportError(context, "Failed to retrieve necessary paths.");
                return;
            }

            specPath = Path.GetFullPath(Path.Combine(projDir, specPath));
            configPath = Path.GetFullPath(Path.Combine(projDir, configPath));

            Log(context, $"Config: {configPath}");
            Log(context, $"Spec: {specPath}");
            Log(context, $"Output: {outputPath}");

            GenerateCodeFromJava(context, configPath, specPath, outputPath);
            AddGeneratedFilesToCompilation(context, outputPath);

            Directory.Delete(outputPath, true);
        }
        catch (Exception ex)
        {
            ReportError(context, ex.ToString());
        }
    }

    private static (string? configPath, string? specPath, string? projDir) GetPaths(GeneratorExecutionContext context)
    {
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            "build_property.ConfigOckoloCodeGenOpenApiServerGenerator",
            out var configPath);
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            "build_property.SpecOckoloCodeGenOpenApiServerGenerator",
            out var specPath);
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            "build_property.projectDir",
            out var projDir);

        return (configPath, specPath, projDir);
    }

    private void GenerateCodeFromJava(GeneratorExecutionContext context, string configPath, string specPath, string outputPath)
    {
        var jarFilePath = GetJarFilePath(context);

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

        using var reader = process.StandardOutput;
        using var errorReader = process.StandardError;

        Log(context, $"Java Output: {reader.ReadToEnd()}");

        var error = errorReader.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        CleanupJarFile(jarFilePath, context);
    }

    private string GetJarFilePath(GeneratorExecutionContext context)
    {
        const string resourcePath = "Ockolo.CodeGen.OpenApiServerGenerator.openapi-generator-cli.jar";
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            "build_property.JarOckoloCodeGenOpenApiServerGenerator",
            out var jarFilePath);

        if (string.IsNullOrEmpty(jarFilePath))
        {
            jarFilePath = Path.GetTempFileName();
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using var fileStream = new FileStream(jarFilePath, FileMode.Create, FileAccess.Write);
            resourceStream!.CopyTo(fileStream);
        }

        return jarFilePath!;
    }

    private static void CleanupJarFile(string jarFilePath, GeneratorExecutionContext context)
    {
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            "build_property.JarOckoloCodeGenOpenApiServerGenerator",
            out var jarFilePathConfig);

        if (string.IsNullOrEmpty(jarFilePathConfig))
        {
            File.Delete(jarFilePath);
        }
    }

    private static void AddGeneratedFilesToCompilation(GeneratorExecutionContext context, string outputPath)
    {
        foreach (var file in Directory.EnumerateFiles(outputPath, "*.cs", SearchOption.AllDirectories))
        {
            var sourceText = File.ReadAllText(file);
            context.AddSource(Path.GetFileNameWithoutExtension(file) + ".g.cs", sourceText);
        }
    }

    private static void Log(GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: InformationalMessageDescriptor,
            location: Location.None,
            messageArgs: message));
    }

    private static void ReportError(GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: ErrorDescriptor,
            location: Location.None,
            messageArgs: message));
    }
}