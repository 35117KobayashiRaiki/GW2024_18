using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SQLite;

namespace BookLendingSystem {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            CreateTable();
            // MainWindowを表示するなどの処理
        }

        public static string DbConnectionString => "Data Source=members.db;Version=3;";

        public void CreateTable() {
            using (SQLiteConnection conn = new SQLiteConnection(DbConnectionString)) {
                conn.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Members (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Barcode TEXT NOT NULL UNIQUE,
                    MemberId TEXT NOT NULL
                );";

                SQLiteCommand cmd = new SQLiteCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
