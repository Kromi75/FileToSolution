// -------------------------------------------------------------------------------------------
// <copyright file="CommandLineOptions.cs" company="i-SOLUTIONS Health GmbH">
//   <CopyrightText>(C) 1997-2020 i-SOLUTIONS Health GmbH</CopyrightText>
// </copyright>
// -------------------------------------------------------------------------------------------
namespace FileToSolution
{
  using System.Collections.Generic;
  using CommandLine;

  public class CommandLineOptions
  {
    [Value(0, MetaName = "source file", Required = true, HelpText = "The path to the source file.")]
    public string? SourceFile { get; set; }

    [Value(1, MetaName = "target directory", Required = false, HelpText = "The directory where the destination solution folders reside.")]
    public string? TargetDirectiory { get; set; }

    [Option('d', "destination-subfolder", Required = false, HelpText = "An optional subfolder name. If not specified, the source file is copied to the solution root folder.")]
    public string? DestinationSubFolder { get; set; }

    [Option('r', "replace-strings")]
    public IEnumerable<string>? Replace { get; set; }
  }
}
