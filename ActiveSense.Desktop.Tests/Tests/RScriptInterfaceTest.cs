using System;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.ViewModels;
using JetBrains.Annotations;
using ActiveSense.Desktop.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActiveSense.Desktop.Tests.Tests;

[TestClass]
public class RScriptInterfaceTest
{
    private IRScriptService _scriptService;
    private IFileService _fileService;
    private IResultParserService _resultParserService;

    [TestInitialize]
    public void Setup()
    {
        _scriptService = new RScriptService("Rscript");
        _fileService = new FileService();
        _resultParserService = new ResultParserService();
    }

    [TestMethod]
    public void GetRScriptBasePath()
    {
        // // Arrange
        // var expectedPath = "expected/base/path";
        //
        // // Act
        // var result = _scriptService.GetRScriptBasePath();
        //
        // // Assert
        // Assert.AreEqual(expectedPath, result);
        Console.WriteLine("RScriptBasePath: " + _scriptService.GetRScriptBasePath());
    }
    
    [TestMethod]
    public void GetRDataPath()
    {
        Console.WriteLine("RDataPath: " + _scriptService.GetRDataPath());
    }
    
    [TestMethod]
    public void GetROutputPath()
    {
        Console.WriteLine("ROutputPath: " + _scriptService.GetROutputPath());
    }

    [TestMethod]
    public void CopyFilesToDirectory()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string solutionDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
        string testFilesDirectory = Path.Combine(solutionDirectory, "Tests/AnalysisTestFiles");
        
        var destinationDirectory = _scriptService.GetRDataPath();
        
        var sourceFiles = _fileService.GetFilesInDirectory(testFilesDirectory, "*.bin");
        foreach (var file in sourceFiles)
        {
            Console.WriteLine("File: " + file);
        }

        var success = _fileService.CopyFilesToDirectoryAsync(sourceFiles, destinationDirectory).Result;
        Assert.IsTrue(success, "Failed to copy files to directory");
    }
    
    [TestMethod]
    public async Task ExecuteRScript()
    {
        var rScriptPath = Path.Combine(_scriptService.GetRScriptBasePath(), "_main.R");
        var (scriptSuccess, output, error) = await _scriptService.ExecuteScriptAsync(
            rScriptPath, _scriptService.GetRScriptBasePath(), "");
        Console.WriteLine(scriptSuccess);
        Console.WriteLine(output);
        Console.WriteLine(error);
    }
    
    [TestMethod]
    public void ParseResults()
    {
        var outputDirectory = _scriptService.GetROutputPath();
        var results = _resultParserService.ParseScript(outputDirectory);
        
        foreach (var result in results.Result)
        {
            Console.WriteLine($"Name: {result.FileName}, Type: {result.AnalysisType}");
        }
    }
}