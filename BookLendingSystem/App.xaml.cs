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

                // 会員情報テーブルの作成
                string createMembersTableQuery = @"
                CREATE TABLE IF NOT EXISTS Members (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Barcode TEXT UNIQUE NOT NULL,
                    MemberId TEXT NOT NULL
                );";

                SQLiteCommand createMembersCmd = new SQLiteCommand(createMembersTableQuery, conn);
                createMembersCmd.ExecuteNonQuery();

                // 貸出履歴テーブルの作成
                string createLoansTableQuery = @"
                CREATE TABLE IF NOT EXISTS Loans (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ISBN TEXT NOT NULL,
                    MemberId TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Author TEXT NOT NULL,
                    LoanDate TEXT NOT NULL,
                    ReturnDate TEXT NOT NULL
                );";

                SQLiteCommand createLoansCmd = new SQLiteCommand(createLoansTableQuery, conn);
                createLoansCmd.ExecuteNonQuery();
            }
        }
    }
}
