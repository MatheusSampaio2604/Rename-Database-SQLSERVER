using Microsoft.Data.SqlClient;

//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RCASCRANE;Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=5

namespace RenameDatabaseSQLSERVER
{
    internal class Program
    {
        private static int typeServiceSelected = 0;

        private static string connectionString = string.Empty; // Database
        private static string rootPath = string.Empty; // File Route

        private static string oldString = string.Empty;
        private static string newString = string.Empty;

        static SqlConnection connection = new();

        private static async Task Main()
        {
            do
            {
                SelectServiceType();
                await DefineProperties();
                try
                {
                    switch (typeServiceSelected)
                    {
                        case 1: //Database
                            Utils.WriteOnlyCharacter('*', 60);
                            Console.WriteLine("Accessing the database...\n");
                            await RenameDatabaseObjectsAsync();
                            break;
                        case 2: // Files
                            Utils.WriteOnlyCharacter('*', 60);
                            Console.WriteLine("\n\nAccessing the folder...\n\n");
                            await RenameFilesAndFolders(rootPath, oldString, newString);
                            break;
                        default:
                            return;
                    }
                    Console.WriteLine("Renomeação completa.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }
            } while (AskToRetry());
        }

        static bool AskToRetry()
        {
            Console.Write("\n Do you want to perform another renaming operation? (y/n): ");
            string? response = Console.ReadLine()?.Trim().ToLower();
            return response == "y" || response == "yes";
        }

        static void SelectServiceType()
        {
            int[] validValues = [1, 2];
            while (true)
            {
                Console.WriteLine("\n Please, Select:\n 1) Rename Database;\n 2) Rename Files and Folders;\n ");
                Console.Write("Value: ");
                if (int.TryParse(Console.ReadLine()?.Trim(), out int value) && validValues.Contains(value))
                {
                    typeServiceSelected = value;
                    break;
                }
                else
                {
                    Utils.ConsoleErrorText();
                    Console.WriteLine("Invalid input. Please select a valid option.");
                    Utils.ConsoleErrorText();
                }
            }
        }

        static async Task DefineProperties()
        {
            switch (typeServiceSelected)
            {
                case 1:
                    while (true)
                    {
                        Console.WriteLine("Please, Insert a connectionString from DB:");
                        Console.Write("Connection String: ");
                        connectionString = Console.ReadLine()?.Trim()!;
                        try
                        {
                            connection = await Database.GetDatabaseConnectionAsync(connectionString);
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("Please try again.");
                        }
                    }
                    break;
                case 2:
                    while (true)
                    {
                        Console.WriteLine("Please, Insert a directory path:");
                        Console.Write("Path: ");
                        rootPath = Console.ReadLine()?.Trim()!;

                        if (Directory.Exists(rootPath)) break;

                        Utils.ConsoleErrorText();
                        Console.WriteLine("O diretório especificado não existe.");
                        Utils.ConsoleErrorText();
                    }
                    break;

            }

            Utils.ConsoleWarningText();
            Console.WriteLine("String Comparison is case-sensitive.\nEx. 'S' is diferent from 's'.");
            Utils.ConsoleWarningText();

            Console.WriteLine("Please, Insert an existing string to be replaced:");
            Console.Write("Old string: ");
            oldString = Console.ReadLine()?.Trim()!;
            Console.WriteLine("\nPlease, Insert a new string for replacement:");
            Console.Write("New string: ");
            newString = Console.ReadLine()?.Trim()!;
        }

        #region Rename DataBase itens
        private static async Task RenameDatabaseObjectsAsync()
        {
            while (true)
            {
                int[] options = [0, 1, 2, 3];

                Console.WriteLine("Select one option for rename:\n1) Rename Only Values from Tables.\n2) Rename Only Columns from Tables.\n3) Rename Only Tables Name.\n0) Rename All Options.\n");
                Console.Write("Option: ");

                if (int.TryParse(Console.ReadLine()?.Trim(), out int option))
                {
                    if (options.Contains(option))
                    {
                        List<(string schema, string table)> tables = await Database.GetTablesWithSchemaAsync(connection);
                        switch (option)
                        {
                            case 0:
                                //Renomear o nome das colunas
                                await Database.RenameColumns(connection, tables, oldString, newString);
                                // Atualizar dados dentro das tabelas
                                await Database.RenameValues(connection, tables, oldString, newString);
                                // Renomear tabelas
                                await Database.RenameTables(connection, tables, oldString, newString);
                                break;
                            case 1:
                                // Atualizar dados dentro das tabelas
                                await Database.RenameValues(connection, tables, oldString, newString);
                                break;
                            case 2:
                                //Renomear o nome das colunas
                                await Database.RenameColumns(connection, tables, oldString, newString);
                                break;
                            case 3:
                                // Renomear tabelas
                                await Database.RenameTables(connection, tables, oldString, newString);
                                break;
                        }
                        break;
                    }
                    else
                    {
                        Utils.ConsoleErrorText();
                        Console.WriteLine($"Option {option} is not valid!, please insert again");
                        Utils.ConsoleErrorText();
                    }
                }
                else
                {
                    Utils.ConsoleErrorText();
                    Console.WriteLine("Please, insert only number option!");
                    Utils.ConsoleErrorText();
                }
            }

            if (connection.State == System.Data.ConnectionState.Open || connection.State == System.Data.ConnectionState.Executing)
            {
                await connection.DisposeAsync();
            }
        }

        #endregion Rename DataBase itens

        #region Rename Folder and itens

        private static async Task RenameFilesAndFolders(string rootPath, string oldName, string newName)
        {
            while (true)
            {
                int[] options = [0, 1, 2, 3];

                Console.WriteLine("Select one option for replace:\n1) Replace Only Folders.\n2) Replace Only Archives.\n3) Replace Only Contents from Archives.\n0) Replace All Options.\n");
                Console.Write("Option: ");

                if (int.TryParse(Console.ReadLine()?.Trim(), out int option))
                {
                    if (options.Contains(option))
                    {
                        switch (option)
                        {
                            case 0:
                                // Renomeia os arquivos
                                Local.RenameOnlyArchives(rootPath, oldName, newName);
                                // Renomeia as pastas
                                Local.RenameOnlyFolders(rootPath, oldName, newName);
                                // Substitui o conteúdo do arquivo
                                await Local.RenameOnlyContent(rootPath, oldName, newName);
                                break;
                            case 1:
                                // Renomeia as pastas
                                Local.RenameOnlyFolders(rootPath, oldName, newName);
                                break;
                            case 2:
                                // Renomeia os arquivos
                                Local.RenameOnlyArchives(rootPath, oldName, newName);
                                break;
                            case 3:
                                // Substitui o conteúdo do arquivo
                                await Local.RenameOnlyContent(rootPath, oldName, newName);
                                break;
                        }
                        break;
                    }
                    else
                    {
                        Utils.ConsoleErrorText();
                        Console.WriteLine($"Option {option} is not valid!, please insert again");
                        Utils.ConsoleErrorText();
                    }
                }
                else
                {
                    Utils.ConsoleErrorText();
                    Console.WriteLine("Please, insert only number option!");
                    Utils.ConsoleErrorText();
                }
            }
        }
        #endregion Rename Folder and itens
    }
}
