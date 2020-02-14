// -------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="i-SOLUTIONS Health GmbH">
//   <CopyrightText>(C) 1997-2020 i-SOLUTIONS Health GmbH</CopyrightText>
// </copyright>
// -------------------------------------------------------------------------------------------
namespace FileToSolution
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Threading.Tasks;
  using CommandLine;

  internal class Program
  {
    private static async Task<int> Main(string[] args)
    {
      // Use custom parser to have the help screen directed to std-out.
      StringWriter helpWriter = new StringWriter();
      Parser parser = new Parser(with => with.HelpWriter = helpWriter);
      return await parser.ParseArguments<CommandLineOptions>(args)
                         .MapResult(
                           async options => await Program.RunAsync(options),
                           async errors => await Program.DisplayHelp(errors, helpWriter));
    }

#pragma warning disable 1998
    private static async Task<int> DisplayHelp(IEnumerable<Error> errors, TextWriter helpWriter)
#pragma warning restore 1998
    {
      if (errors.IsVersion() || errors.IsHelp())
      {
        Console.WriteLine(helpWriter.ToString());
        return 0;
      }

      Console.Error.WriteLine(helpWriter.ToString());
      return 1;
    }

    private static async Task<int> RunAsync([NotNull] CommandLineOptions commandLineOptions)
    {
      WorkerOptions workerOptions = new WorkerOptions(commandLineOptions);

      try
      {
        Worker worker = new Worker();
        await worker.CopyAsync(workerOptions, Console.WriteLine);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return 1;
      }

      return 0;
    }
  }
}
