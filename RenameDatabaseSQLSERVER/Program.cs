using Microsoft.Data.SqlClient;

namespace RenameDatabaseSQLSERVER
{
    internal class Program
    {

        private static async Task Main(string[] args)
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RCASCRANE;Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=5";

            //Console.WriteLine("Digite a string antiga:");
            string oldString = "RCAS";

            //Console.WriteLine("Digite a string nova:");
            string newString = "AMV";

            try
            {
                await RenameDatabaseObjectsAsync(connectionString, oldString, newString);
                Console.WriteLine("Renomeação completa para tabelas, colunas e dados.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }


        private static async Task RenameDatabaseObjectsAsync(string connectionString, string oldString, string newString)
        {
            using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();

            string[] tables = await GetTablesAsync(connection);
            foreach (string table in tables) // Renomear tabelas
            {
                if (table.Contains(oldString))
                {
                    string newTableName = table.Replace(oldString, newString);
                    Console.WriteLine($"Renomeando tabela {table} para {newTableName}...");
                    await ExecuteCommandAsync(connection, $"EXEC sp_rename '{table}', '{newTableName}'");
                }
            }

            foreach (string table in tables) // Renomear colunas dentro de cada tabela
            {
                string[] columns = await GetColumnsAsync(connection, table);
                foreach (string column in columns)
                {
                    if (column.Contains(oldString))
                    {
                        string newColumnName = column.Replace(oldString, newString);
                        Console.WriteLine($"Renomeando coluna {column} na tabela {table} para {newColumnName}...");
                        await ExecuteCommandAsync(connection, $"EXEC sp_rename '{table}.{column}', '{newColumnName}', 'COLUMN'");
                    }
                }
            }

            foreach (string table in tables) // Atualizar dados dentro das tabelas
            {
                string[] columns = await GetColumnsAsync(connection, table);
                foreach (string column in columns)
                {
                    Console.WriteLine($"Atualizando dados na coluna {column} da tabela {table}...");
                    string updateCommand = $"UPDATE {table} SET {column} = REPLACE({column}, @oldValue, @newValue) WHERE {column} LIKE '%' + @oldValue + '%'";
                    using SqlCommand command = new(updateCommand, connection);
                    _ = command.Parameters.AddWithValue("@oldValue", oldString);
                    _ = command.Parameters.AddWithValue("@newValue", newString);
                    _ = await command.ExecuteNonQueryAsync();
                }
            }
        }


        private static async Task<string[]> GetTablesAsync(SqlConnection connection)
        {
            List<string> tables = [];
            string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using (SqlCommand command = new(query, connection))
            {
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            return tables.ToArray();
        }


        private static async Task<string[]> GetColumnsAsync(SqlConnection connection, string tableName)
        {
            List<string> columns = [];
            string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
            using (SqlCommand command = new(query, connection))
            {
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(0));
                }
            }
            return columns.ToArray();
        }


        private static async Task ExecuteCommandAsync(SqlConnection connection, string commandText)
        {
            using SqlCommand command = new(commandText, connection);
            _ = await command.ExecuteNonQueryAsync();
        }
    }
}
