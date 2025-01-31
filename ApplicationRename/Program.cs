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
                        connectionString = Utils.ReturnReadLineConsole("Please, Insert a connectionString for DB:", A("value")!);
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
                    break;

            }

            Utils.ConsoleWarningText();
            Console.WriteLine("String Comparison is case-sensitive.\nEx. 'S' is diferent from 's'.");
            Utils.ConsoleWarningText();

            oldString = Utils.ReturnReadLineConsole("Please, Insert an existing string to be replaced:", "Old string: ");
            newString = Utils.ReturnReadLineConsole("\nPlease, Insert a new string for replacement:", "New string: ");
        }

        static void SetLanguage()
        {
            Console.WriteLine("Choose a language / Escolha um idioma / Elige un idioma: \n1) English \n2) Português \n3) Español");
            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "2":
                    CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
                    break;
                case "3":
                    CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
                    break;
                default:
                    CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                    break;
            }
        }

        #region Rename DataBase itens
        private static async Task RenameDatabaseObjectsAsync()
        {
            while (true)
            {
                int[] options = [0, 1, 2, 3];

                string writeLine = "Select one option for rename:\n1) Rename Only Values from Tables.\n2) Rename Only Columns from Tables.\n3) Rename Only Tables Name.\n0) Rename All Options.\n";
                string write = "Option: ";

                if (int.TryParse(Utils.ReturnReadLineConsole(writeLine, write), out int option) && options.Contains(option))
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

                string writeLine = "Select one option for replace:\n1) Replace Only Folders.\n2) Replace Only Archives.\n3) Replace Only Contents from Archives.\n0) Replace All Options.\n";
                string write = "Option: ";

                if (int.TryParse(Utils.ReturnReadLineConsole(writeLine, write), out int option) && options.Contains(option))
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
                    Console.WriteLine("Please, insert only number option!");
                    Utils.ConsoleErrorText();
                }
            }
        }

        #endregion Rename Folder and itens
    }
}
