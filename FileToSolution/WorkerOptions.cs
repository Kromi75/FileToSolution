// -------------------------------------------------------------------------------------------
// <copyright file="WorkerOptions.cs" company="i-SOLUTIONS Health GmbH">
//   <CopyrightText>(C) 1997-2020 i-SOLUTIONS Health GmbH</CopyrightText>
// </copyright>
// -------------------------------------------------------------------------------------------
namespace FileToSolution
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  /// <summary>
  /// Determines what the program has to do actually according to the parameters that where provided by the command line.
  /// </summary>
  public class WorkerOptions
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerOptions" /> class.
    /// Maps the information from the command line arguments.
    /// </summary>
    public WorkerOptions(CommandLineOptions commandLineOptions)
    {
      if (commandLineOptions.SourceFile == null)
      {
        throw new InvalidOperationException("SourceFile is null.");
      }

      this.SourcePath = commandLineOptions.SourceFile;
      if (!File.Exists(this.SourcePath))
      {
        throw new InvalidOperationException($"File not found: <{this.SourcePath}>.");
      }

      if (string.IsNullOrEmpty(commandLineOptions.DestinationSubFolder))
      {
        this.TargetSearchPattern = "*";
        this.SearchOption = SearchOption.TopDirectoryOnly;
      }
      else
      {
        this.TargetSearchPattern = commandLineOptions.DestinationSubFolder;
        this.SearchOption = SearchOption.AllDirectories;
      }

      if (string.IsNullOrEmpty(commandLineOptions.TargetDirectiory))
      {
        this.TargetDirectory = Directory.GetCurrentDirectory();
      }
      else
      {
        this.TargetDirectory = commandLineOptions.TargetDirectiory;
      }

      if (!Directory.Exists(this.TargetDirectory))
      {
        throw new InvalidOperationException($"Directory not found: <{this.TargetDirectory}>.");
      }

      if (commandLineOptions.Replace != null)
      {
        Dictionary<string, string> replaceList = new Dictionary<string, string>();
        List<string> optionsReplace = commandLineOptions.Replace.ToList();
        int optionsReplaceCount = optionsReplace.Count;
        for (int i = 0; i < optionsReplaceCount; i += 2)
        {
          if (i < optionsReplaceCount && optionsReplace[i] != null)
          {
            replaceList.Add(optionsReplace[i], optionsReplace[i + 1]);
          }
        }

        this.ReplaceList = replaceList;
      }
    }

    public string TargetDirectory { get; }

    public string? TargetSearchPattern { get; }

    public SearchOption SearchOption { get; }

    public string SourcePath { get; }

    public IDictionary<string, string>? ReplaceList { get; }
  }
}
