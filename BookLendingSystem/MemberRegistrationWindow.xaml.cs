using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace BookLendingSystem {
    /// <summary>
    /// MemberRegistrationWindow.xaml の相互作用ロジック
    /// 会員登録ウィンドウの処理を担当するクラス
    /// </summary>
    public partial class MemberRegistrationWindow : Window {

        public MemberRegistrationWindow() {
            InitializeComponent();

        }

        // 会員登録ボタンがクリックされたときの処理
        private void RegistrationButton_Click(object sender, RoutedEventArgs e) {

            // バーコードから会員IDを生成
            string barcode = BarcodeTextBox.Text.Trim();

            // バーコード番号が入力されていない場合はエラーメッセージ
            if (string.IsNullOrEmpty(barcode)) {
                MessageBox.Show("バーコードを入力してください。");
                return;
            }

            // 重複するバーコードが既に登録されていないか確認
            if (IsBarcodeExist(barcode)) {
                ClearInputFields();
                MessageBox.Show("このバーコード番号は既に登録されています。");
                return;
            }

            // 会員IDを生成
            string memberId = GenerateMemberId();

            // 会員IDを表示
            MemberIDTextBox.Text = memberId;

            // データベースに登録
            RegisterMember(barcode, memberId);

            MessageBox.Show("会員登録が完了しました。");

            // 入力フィールドをクリア
            ClearInputFields();
        }

        // 入力フィールドをクリア
        private void ClearInputFields() {
            BarcodeTextBox.Clear();
            MemberIDTextBox.Clear();
        }

            // 新しい会員IDを生成するメソッド
            private string GenerateMemberId() {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // データベースから最大の会員IDを取得
                string query = "SELECT MAX(Id) FROM Members";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                var result = cmd.ExecuteScalar();

                // 新しい会員IDを生成（最大値 + 1）
                int newId = result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;

                // 会員IDは5桁でゼロ埋め（例: 00001, 00002...）
                return newId.ToString("D5");
            }
        }

        // 会員情報をデータベースに登録するメソッド
        private void RegisterMember(string barcode, string memberId) {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 会員情報をMembersテーブルに挿入
                string query = "INSERT INTO Members (Barcode, MemberId) VALUES (@Barcode, @MemberId)";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Barcode", barcode);
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                cmd.ExecuteNonQuery();
            }
        }

        // バーコードがすでに登録されているか確認するメソッド
        private bool IsBarcodeExist(string barcode) {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // データベースでバーコードが既に登録されているかをカウント
                string query = "SELECT COUNT(*) FROM Members WHERE Barcode = @Barcode";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Barcode", barcode);
                long count = (long)cmd.ExecuteScalar();

                // バーコードが存在すればtrueを返す
                return count > 0;
            }
        }

        // キャンセルボタンのクリックイベント
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            // MainWindowがすでに開かれている場合に再表示する
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();  // MainWindowを表示
            }

            // 現在のウィンドウ（MemberRegistrationWindow）を閉じる
            this.Close();  // MemberRegistrationWindowを閉じる
        }

        // ウィンドウが閉じられたときにMainWindowを表示する
        private void Window_Closed(object sender, EventArgs e) {
            // MainWindowを再表示する
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();  // MainWindowを表示
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            string barcode = BarcodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(barcode)) {
                MessageBox.Show("削除するバーコードを入力してください。");
                return;
            }

            if (!IsBarcodeExist(barcode)) {
                ClearInputFields();
                MessageBox.Show("このバーコード番号は登録されていません。");
                return;
            }

            DeleteMember(barcode);

            MessageBox.Show("会員情報が削除されました。");

            BarcodeTextBox.Clear();
            MemberIDTextBox.Clear();
        }

        // バーコードに対応する会員情報を削除するメソッド
        private void DeleteMember(string barcode) {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string query = "DELETE FROM Members WHERE Barcode = @Barcode";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Barcode", barcode);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
