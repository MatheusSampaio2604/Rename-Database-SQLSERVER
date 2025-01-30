using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameDatabaseSQLSERVER
{
    internal class Database
    {
        #region Get Connection

        internal static async Task<SqlConnection> GetDatabaseConnectionAsync(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("\nConnection established successfully!");
                Console.ResetColor();
                return connection;
            }
            catch (Exception ex)
            {
                Utils.ConsoleErrorText();
                Console.WriteLine($"Error: Unable to connect to the database.\n{ex.Message}");
                Utils.ConsoleErrorText();
                throw;
            }
        }


        #endregion


        #region Get data from DB
        private static async Task<string[]> GetColumnsAsync(SqlConnection connection, string schema, string table)
        {
            var columns = new List<string>();
            string query = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table";

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@schema", schema);
            command.Parameters.AddWithValue("@table", table);

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return columns.ToArray();
        }

        private static async Task<string[]> GetStringColumnsAsync(SqlConnection connection, string schema, string table)
        {
            var columns = new List<string>();
            string query = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @schema 
            AND TABLE_NAME = @table
            AND DATA_TYPE IN ('varchar', 'nvarchar', 'text', 'char', 'nchar')"; //Filtra apenas colunas de texto

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@schema", schema);
            command.Parameters.AddWithValue("@table", table);

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return [.. columns];
        }

        internal static async Task<List<(string schema, string table)>> GetTablesWithSchemaAsync(SqlConnection connection)
        {
            var tables = new List<(string schema, string table)>();

            string query = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

            using SqlCommand command = new(query, connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add((reader.GetString(0), reader.GetString(1))); // schema, table
            }

            return tables;
        }
        #endregion

        #region Exec Action

        private static async Task ExecuteCommandAsync(SqlConnection connection, string commandText)
        {
            using SqlCommand command = new(commandText, connection);
            _ = await command.ExecuteNonQueryAsync();
        }

        #endregion

        internal static async Task RenameColumns(SqlConnection connection, List<(string schema, string table)> tables, string oldString, string newString)
        {

            foreach (var (schema, table) in tables)
            {
                string[] columns = await GetColumnsAsync(connection, schema, table);
                foreach (string column in columns)
                {
                    if (column.Contains(oldString))
                    {
                        string newColumnName = column.Replace(oldString, newString);
                        Console.WriteLine($"Renomeando coluna {column} na tabela {schema}.{table} para {newColumnName}...");
                        try
                        {
                            string renameQuery = $"EXEC sp_rename '{schema}.{table}.{column}', '{newColumnName}', 'COLUMN'";
                            await ExecuteCommandAsync(connection, renameQuery);
                            Console.WriteLine("Coluna renomeada com sucesso.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao renomear coluna {column}: {ex.Message}");
                        }
                    }
                }
            }
        }

        internal static async Task RenameValues(SqlConnection connection, List<(string schema, string table)> tables, string oldString, string newString)
        {
            foreach (var (schema, table) in tables)
            {
                string[] columns = await GetStringColumnsAsync(connection, schema, table);
                foreach (string column in columns)
                {
                    Console.WriteLine($"Atualizando dados na coluna {column} da tabela {schema}.{table}...");
                    try
                    {
                        string updateCommand = $@"
                        UPDATE [{schema}].[{table}]
                        SET [{column}] = REPLACE([{column}], @oldValue, @newValue)
                        WHERE [{column}] LIKE '%' + CAST(@oldValue AS NVARCHAR(MAX)) + '%'";

                        using SqlCommand command = new(updateCommand, connection);
                        command.Parameters.AddWithValue("@oldValue", oldString);
                        command.Parameters.AddWithValue("@newValue", newString);
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        Console.WriteLine($"{rowsAffected} registros atualizados na tabela {schema}.{table}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao atualizar dados na tabela {schema}.{table}: {ex.Message}");
                    }
                }
            }
        }

        internal static async Task RenameTables(SqlConnection connection, List<(string schema, string table)> tables, string oldString, string newString)
        {
            foreach (var (schema, table) in tables.OrderBy(x => x.table))
            {
                if (table.Contains(oldString))
                {
                    string newTableName = table.Replace(oldString, newString);
                    Console.WriteLine($"Renomeando tabela {schema}.{table} para {schema}.{newTableName}...");

                    try
                    {
                        string renameQuery = $"EXEC sp_rename '{schema}.{table}', '{newTableName}'";
                        await ExecuteCommandAsync(connection, renameQuery);
                        Console.WriteLine("Renomeação bem-sucedida.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao renomear {schema}.{table}: {ex.Message}");
                    }
                }
            }

        }



    }
}
