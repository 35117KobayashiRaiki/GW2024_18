using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace BookLendingSystem {
    /// <summary>
    /// LoginWindow.xaml の相互作用ロジック
    /// ログイン画面の処理を担当するクラス
    /// </summary>
    public partial class LoginWindow : Window {
        public bool IsLoggedIn { get; private set; } = false;

        public LoginWindow() {
            InitializeComponent();

            // BarcodeTextBoxのTextChangedイベントを登録
            BarcodeTextBox.TextChanged += BarcodeTextBox_TextChanged;
        }

        // キャンセルボタンがクリックされた時の処理
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            // MainWindowが開かれている場合に再表示する
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();  // MainWindowを表示
            }

            // 現在のウィンドウ（LoginWindow）を閉じる
            this.Close();
        }

        // BarcodeTextBoxのTextChangedイベントハンドラ
        private void BarcodeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            string barcode = BarcodeTextBox.Text.Trim();

            // バーコードが空の場合は処理を中断
            if (string.IsNullOrEmpty(barcode)) {
                MemberIDTextBox.Clear();
                return;
            }

            // バーコードがデータベースに存在するか確認
            if (IsBarcodeExist(barcode)) {
                // バーコードに対応する会員IDを取得して表示
                string dbMemberId = GetMemberIdByBarcode(barcode);
                MemberIDTextBox.Text = dbMemberId;
            } else {
                // バーコードが存在しない場合、MemberIDTextBoxをクリア
                MemberIDTextBox.Clear();
            }
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e) {
            // 入力されたバーコード、会員ID、パスワードを取得
            string barcode = BarcodeTextBox.Text.Trim();
            string memberId = MemberIDTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // パスワードが正しいか確認
            if (password != "Ota2024") {
                MessageBox.Show("パスワードが間違っています。");
                return;
            }

            if (IsBarcodeExist(barcode)) {
                // バーコードに対応する会員IDをデータベースから取得
                string dbMemberId = GetMemberIdByBarcode(barcode);

                // 会員IDをテキストボックスに表示
                MemberIDTextBox.Text = dbMemberId;

                // 入力された会員IDがデータベースから取得した会員IDと一致するか確認
                if (memberId == dbMemberId) {
                    // ログインが成功した場合
                    IsLoggedIn = true;
                    // 会員IDが一致した場合、次の画面（SelectionWindow）を表示
                    SelectionWindow selectionWindow = new SelectionWindow();
                    selectionWindow.Show();
                    this.Close(); // 現在の画面を閉じる
                } else {
                    // 会員IDが一致しない場合、エラーメッセージを表示
                    MessageBox.Show("会員IDが一致しません。");
                }
            } else {
                // バーコードが登録されていない場合、エラーメッセージを表示
                MessageBox.Show("バーコードが登録されていません。");

                // 入力フィールドをクリア
                BarcodeTextBox.Clear();
                MemberIDTextBox.Clear();
            }
        }

        // バーコードがデータベースに存在するか確認するメソッド
        private bool IsBarcodeExist(string barcode) {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // Membersテーブルでバーコードの存在を確認
                string query = "SELECT COUNT(*) FROM Members WHERE Barcode = @Barcode";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Barcode", barcode);

                // バーコードが存在すれば1以上の数を返す
                long count = (long)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // バーコードに対応する会員IDをデータベースから取得するメソッド
        private string GetMemberIdByBarcode(string barcode) {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // Membersテーブルからバーコードに対応する会員IDを取得
                string query = "SELECT MemberId FROM Members WHERE Barcode = @Barcode";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);

                // 会員IDを返す（なければnull）
                cmd.Parameters.AddWithValue("@Barcode", barcode);
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        // LoginWindowが閉じられたときの処理
        private void Window_Closed(object sender, EventArgs e) {
            // MainWindowが開かれていれば再表示
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();
            }
        }
    }
}
