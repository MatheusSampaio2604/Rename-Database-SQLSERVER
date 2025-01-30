using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameDatabaseSQLSERVER
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
                        Console.WriteLine($"Arquivo renomeado: {filePath} -> {newFilePath}");
                        //});
                    }
                    catch (IOException ioEx) when (ioEx.Message.Contains("já existente"))
                    {
                        Console.WriteLine($"Erro: O arquivo destino já existe. {newFilePath}");
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
                        Console.WriteLine($"Pasta renomeada: {dirPath} -> {newDirPath}");
                        /* });*/
                    }
                    catch (IOException ioEx) when (ioEx.Message.Contains("já existente"))
                    {
                        Console.WriteLine($"Erro: A pasta destino já existe. {newDirPath}");
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
                    string content = File.ReadAllText(filePath);
                    if (content.Contains(oldName/*, StringComparison.OrdinalIgnoreCase*/))
                    {
                        content = content.Replace(oldName, newName/*, StringComparison.OrdinalIgnoreCase*/);

                        await File.WriteAllTextAsync(filePath, content);
                        Console.WriteLine($"Conteúdo atualizado no arquivo: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar o arquivo {filePath}: {ex.Message}");
                }
            }

        }


    }
}
