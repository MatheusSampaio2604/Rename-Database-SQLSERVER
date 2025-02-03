using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Resources;

//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RCASCRANE;Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=5

namespace ReplaceStringOptions
{

    internal class Program
    {
        private static ResourceManager resManager = new ResourceManager("ReplaceStringOptions.Resources.Strings", typeof(Program).Assembly);
        private static int typeServiceSelected = 0;

        private static string connectionString = string.Empty; // Database
        private static string rootPath = string.Empty; // File Route

        private static string oldString = string.Empty;
        private static string newString = string.Empty;

        static SqlConnection connection = new();

        private static async Task Main()
        {
            SetLanguage();
            Console.WriteLine(A("WelcomeMessage"));

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
                            Console.WriteLine($"{A("AccessDB")}...\n");
                            await RenameDatabaseObjectsAsync();
                            break;
                        case 2: // Files
                            Utils.WriteOnlyCharacter('*', 60);
                            Console.WriteLine($"\n\n{A("AccessFolder")}...\n\n");
                            await RenameFilesAndFolders(rootPath, oldString, newString);
                            break;
                        default:
                            return;
                    }
                    Console.WriteLine(A("CompleteRename"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (AskToRetry());
        }

        static bool AskToRetry()
        {
            Console.Write($"\n{A("tryAgain")} (y/n): ");
            string? response = Console.ReadLine()?.Trim().ToLower();
            return response == "y" || response == "yes";
        }

        static void SelectServiceType()
        {
            int[] validValues = [1, 2];
            while (true)
            {
                Console.WriteLine($"\n{A("ChooseOption")}\n    1) {A("Rename")} {A("db")};\n    2) {A("Rename")} {A("files")} {A("&")} {A("folders")};\n ");
                Console.Write(A("value"));
                if (int.TryParse(Console.ReadLine()?.Trim(), out int value) && validValues.Contains(value))
                {
                    typeServiceSelected = value;
                    break;
                }
                else
                {
                    Utils.ConsoleErrorText();
                    Console.WriteLine(A("InvalidInput"));
                    Utils.ConsoleErrorText();
                }
            }
        }

        /// <summary>
        /// get text by key to view the program in the selected language
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string? A(string key)
        {
            return resManager.GetString(key);
        }



        static async Task DefineProperties()
        {
            switch (typeServiceSelected)
            {
                case 1:
                    while (true)
                    {
                        connectionString = Utils.ReturnReadLineConsole(A("InsertDB")!, A("value")!);
                        try
                        {
                            connection = await Database.GetDatabaseConnectionAsync(connectionString);
                            break;
                        }
                        catch
                        {
                            Console.WriteLine(A("EnterAgain"));
                        }
                    }
                    break;
                case 2:
                    while (true)
                    {
                        rootPath = Utils.ReturnReadLineConsole($"{A("EnterValue")} a {A("dirPath")}:", A("value")!);

                        if (Directory.Exists(rootPath)) break;

                        Utils.ConsoleErrorText();
                        Console.WriteLine(A("DirNotFound"));
                        Utils.ConsoleErrorText();
                    }

                    Utils.ConsoleWarningText();
                    Console.WriteLine($"{A("CaseSensEnabled")}\nEx. 'B' {A("IsDiffFrom")} 'b'.");
                    Utils.ConsoleWarningText();
                    break;
            }

            //Utils.ConsoleWarningText();
            //Console.WriteLine($"{A("CaseSensEnabled")}\nEx. 'B' {A("IsDiffFrom")} 'b'.");
            //Utils.ConsoleWarningText();

            oldString = Utils.ReturnReadLineConsole(A("OldVal")!, A("value")!);
            newString = Utils.ReturnReadLineConsole($"\n{A("NewVal")}", A("value")!);
        }

        static void SetLanguage()
        {
            Console.WriteLine("Choose a language / Escolha um idioma / Elige un idioma: \n1) English \n2) Português \n3) Español");
            string? choice = Console.ReadLine()?.Trim();

            CultureInfo.CurrentUICulture = choice switch
            {
                "2" => new CultureInfo("pt-BR"),
                "3" => new CultureInfo("es-ES"),
                _ => new CultureInfo("en-US"),
            };


        }

        #region Rename DataBase itens

        private static async Task RenameDatabaseObjectsAsync()
        {
            while (true)
            {
                int[] options = [0, 1, 2, 3];

                string writeLine = $"{A("ChooseOptionReplace")}\n    " +
                                   $"1) {A("OptDBOnlyValues")}\n    " +
                                   $"2) {A("OptDBOnlyColumns")}\n    " +
                                   $"3) {A("OptDBOnlyTables")}\n    " +
                                   $"0) {A("OptReplaceAll")}\n";


                if (int.TryParse(Utils.ReturnReadLineConsole(writeLine, A("value")!), out int option) && options.Contains(option))
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
                    Console.WriteLine(A("WarnNumberOption"));
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

                string writeLine = $"{A("ChooseOptionReplace")}\n    " +
                                   $"1) {A("OptLocalOnlyFolders")}\n    " +
                                   $"2) {A("OptLocalOnlyArchives")}\n    " +
                                   $"3) {A("OptLocalOnlyContents")}\n    " +
                                   $"0) {A("OptReplaceAll")}\n";

                if (int.TryParse(Utils.ReturnReadLineConsole(writeLine, A("value")!), out int option) && options.Contains(option))
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
                    Console.WriteLine(A("WarnNumberOption"));
                    Utils.ConsoleErrorText();
                }
            }
        }

        #endregion Rename Folder and itens
    }
}
