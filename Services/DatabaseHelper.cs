using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    public class DatabaseHelper
    {
        private static readonly string _dbFileName = "liteclinic.db";
        private static readonly string _folderPath;
        private static readonly string _dbPath;
        private static readonly string _connectionString;
        private static SqliteConnection? _connection;

        // Static constructor runs once when the class is first used
        static DatabaseHelper()
        {
            _folderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path; ;
            //Directory.CreateDirectory(_folderPath); // ensures folder exists

            _dbPath = Path.Combine(_folderPath, _dbFileName);
            _connectionString = $"Data Source={_dbPath}";
        }

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open(); // always open before returning
            return conn;
        }

        public static void CloseConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
        // Sync methods can be added here if needed
        public static async Task<SqliteConnection> GetConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(); // async open
            }

            return _connection;
        }

        public static async Task CloseConnectionAsync()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync(); // async close
                await _connection.DisposeAsync(); // async dispose
                _connection = null;
            }
        }
    }
}