using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceStringOptions
{
    internal class Local
    {

        internal static void RenameOnlyArchives(string rootPath, string oldName, string newName)
        {
            foreach (string filePath in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
            {
                // Renomeia o arquivo
                string fileName = Path.GetFileName(filePath);
                if (fileName.Contains(oldName/*, StringComparison.OrdinalIgnoreCase*/))
                {
                    string newFileName = fileName.Replace(oldName, newName/*, StringComparison.OrdinalIgnoreCase*/);
                    string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);
                    try
                    {
                        //await Task.Run(() => { 
                        File.Move(filePath, newFilePath);
                        Console.WriteLine($"File renamed: {filePath} -> {newFilePath}");
                        //});
                    }
                    catch (IOException ioEx)/* when (ioEx.Message.Contains("já existente"))*/
                    {
                        Console.WriteLine($"Error: Destination file already exists. {newFilePath}");
                    }
                }
            }
        }

        internal static void RenameOnlyFolders(string rootPath, string oldName, string newName)
        {
            foreach (string dirPath in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
            {
                string dirName = Path.GetFileName(dirPath);
                if (dirName.Contains(oldName/*, StringComparison.OrdinalIgnoreCase*/))
                {
                    string parentDir = Path.GetDirectoryName(dirPath);
                    string newDirName = dirName.Replace(oldName, newName/*, StringComparison.OrdinalIgnoreCase*/);
                    string newDirPath = Path.Combine(parentDir, newDirName);
                    try
                    {
                        /*await Task.Run(() => {*/
                        Directory.Move(dirPath, newDirPath);
                        Console.WriteLine($"Folder renamed: {dirPath} -> {newDirPath}");
                        /* });*/
                    }
                    catch (IOException ioEx) when (ioEx.Message.Contains("já existente"))
                    {
                        Console.WriteLine($"Error: Destination folder already exists. {newDirPath}");
                    }
                }
            }
        }

        internal static async Task RenameOnlyContent(string rootPath, string oldName, string newName)
        {
            foreach (string filePath in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    string extension = Path.GetExtension(filePath);
                    if (Utils.TextFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string content = File.ReadAllText(filePath);
                    if (content.Contains(oldName/*, StringComparison.OrdinalIgnoreCase*/))
                    {
                        content = content.Replace(oldName, newName/*, StringComparison.OrdinalIgnoreCase*/);

                        await File.WriteAllTextAsync(filePath, content);
                        Console.WriteLine($"Updated content in the archive: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
            }

        }


    }
}
