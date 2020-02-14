// -------------------------------------------------------------------------------------------
// <copyright file="Worker.cs" company="i-SOLUTIONS Health GmbH">
//   <CopyrightText>(C) 1997-2020 i-SOLUTIONS Health GmbH</CopyrightText>
// </copyright>
// -------------------------------------------------------------------------------------------
namespace FileToSolution
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  /// <summary>
  /// Is responsible for doing the actual work. This means, reading the source file content,
  /// performing the requested replacements and writing the result to all target folders.
  /// </summary>
  public class Worker
  {
    /// <summary>
    /// Reads the source file content, performs the requested replacements and writes the result to all target folders.
    /// </summary>
    /// <param name="workerOptions">Determines what exactly has to be done.</param>
    /// <param name="reportProgressAction">An optional action that is used to report progress to the caller.</param>
    /// <returns>A <see cref="Task"/> representing the ongoing task.</returns>
    public async Task CopyAsync(WorkerOptions workerOptions, Action<string>? reportProgressAction)
    {
      IEnumerable<string> entries = Directory.EnumerateDirectories(workerOptions.TargetDirectory, workerOptions.TargetSearchPattern, workerOptions.SearchOption);
      if (entries == null)
      {
        throw new InvalidOperationException("No target directories found.");
      }

      string sourceFileContent = await this.ReadTextAsync(workerOptions.SourcePath);
      if (sourceFileContent == null)
      {
        throw new InvalidOperationException("No source file content found.");
      }

      IEnumerable<string> destinationPathList = entries.Select(e => Path.Combine(e, Path.GetFileName(workerOptions.SourcePath)));
      await this.WriteFilesAsync(destinationPathList, sourceFileContent, workerOptions.ReplaceList, reportProgressAction);
    }

    private async Task WriteFilesAsync(IEnumerable<string> destinationPathList, string sourceFileContent, IDictionary<string, string>? contentReplaceList, Action<string> reportProgressAction)
    {
      if (destinationPathList == null)
      {
        throw new ArgumentNullException(nameof(destinationPathList));
      }

      if (sourceFileContent == null)
      {
        throw new ArgumentNullException(nameof(sourceFileContent));
      }

      List<Task> tasks = new List<Task>();
      List<FileStream> sourceStreams = new List<FileStream>();

      try
      {
        foreach (string destinationPath in destinationPathList)
        {
          if (destinationPath == null)
          {
            continue;
          }

          reportProgressAction?.Invoke($"Copy file to {destinationPath}.");
          
          string targetFileContent = Worker.BuildTargetFileContent(sourceFileContent, contentReplaceList, destinationPath);

          byte[] encodedText = Encoding.UTF8.GetBytes(targetFileContent);

          FileStream sourceStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

          Task theTask = sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
          sourceStreams.Add(sourceStream);

          tasks.Add(theTask);
        }

        await Task.WhenAll(tasks);
      }
      finally
      {
        foreach (FileStream sourceStream in sourceStreams)
        {
          sourceStream.Close();
        }
      }
    }

    private static string BuildTargetFileContent(string sourceFileContent, IDictionary<string, string>? contentReplaceList, string destinationPath)
    {
      string targetFileContent = sourceFileContent;
      if (contentReplaceList != null)
      {
        foreach (KeyValuePair<string, string> replacePair in contentReplaceList)
        {
          // TODO: Provide a dictionary with placeholder keywords.
          string newValue = replacePair.Value;
          if (newValue.Contains("#SolutionName#"))
          {
            // TODO: Determine the solution name by searching for a .sln file in the parent folders.
            string solutionName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(destinationPath))) ?? "SolutionName";
            newValue = newValue.Replace("#SolutionName#", solutionName);
          }

          targetFileContent = targetFileContent.Replace(replacePair.Key, newValue);
        }
      }

      return targetFileContent;
    }

    private async Task<string> ReadTextAsync(string filePath)
    {
      if (filePath == null)
      {
        throw new ArgumentNullException(nameof(filePath));
      }

      await using FileStream sourceStream = new FileStream(
        filePath,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        bufferSize: 4096,
        useAsync: true);
      StringBuilder sb = new StringBuilder();

      byte[] buffer = new byte[0x1000];
      int numRead;
      while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
      {
        string text = Encoding.UTF8.GetString(buffer, 0, numRead);
        sb.Append(text);
      }

      return sb.ToString();
    }
  }
}
