using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Xml.Linq;


namespace BookLendingSystem {
    /// <summary>
    /// LendingWindow.xaml の相互作用ロジック
    /// 貸出システムのメインウィンドウで貸出情報を管理する機能を提供します。
    /// </summary>
    public partial class LendingWindow : Window {

        public class Loan {
            public int Id { get; set; }
            public string MemberID { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime ReturnDate { get; set; }
        }

        public LendingWindow() {
            InitializeComponent();  // 初期化処理
            LoadLoanHistory(); // 貸出履歴を読み込む
        }

        private async Task<(string Title, string Author)> GetBookInfoFromISBNAsync(string isbn) {
            string apiUrl = $"https://iss.ndl.go.jp/api/opensearch?isbn={isbn}";
            using (HttpClient client = new HttpClient()) {
                try {
                    // APIにリクエストを送信
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string xmlContent = await response.Content.ReadAsStringAsync();

                    // XMLデータを解析
                    XDocument doc = XDocument.Parse(xmlContent);
                    XNamespace dc = "http://purl.org/dc/elements/1.1/";

                    // タイトルと著者を抽出
                    string title = doc.Descendants(dc + "title").FirstOrDefault()?.Value ?? "タイトル不明";

                    // 著者を抽出（最初の1人だけ取得）
                    string authorRaw = doc.Descendants("author").FirstOrDefault()?.Value ?? "著者不明";
                    string author = authorRaw.Split(',').FirstOrDefault()?.Trim() ?? "著者不明";

                    return (title, author);
                }
                catch (Exception ex) {
                    MessageBox.Show($"エラー: {ex.Message}");
                    return ("エラー", "エラー");
                }
            }
        }

        private void SetLoanDates() {
            // 現在の日付を取得
            DateTime currentDate = DateTime.Now;

            // 貸出日を設定
            LoanDatePicker.Text = currentDate.ToString("yyyy-MM-dd");

            // 返却期限を設定（貸出日の1週間後）
            DateTime dueDate = currentDate.AddDays(7);
            ReturnDatePicker.Text = dueDate.ToString("yyyy-MM-dd");
        }

        private async void ISBNTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            string isbn = ISBNTextBox.Text.Trim();

            if (isbn.Length == 13 || isbn.Length == 10) // ISBNの長さチェック
            {
                var (title, author) = await GetBookInfoFromISBNAsync(isbn);
                BookTitleTextBox.Text = title;
                AuthorTextBox.Text = author;

                // 貸出日と返却期限を設定
                SetLoanDates();
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

        private void RegisterLendingButton_Click(object sender, RoutedEventArgs e) {
            // 入力されたデータを取得
            string isbn = ISBNTextBox.Text.Trim();
            string barcode = BarcodeTextBox.Text.Trim();
            string memberId = MemberIDTextBox.Text.Trim();
            string bookTitle = BookTitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string loanDate = LoanDatePicker.Text.Trim();
            string returnDate = ReturnDatePicker.Text.Trim();

            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(memberId) ||
                string.IsNullOrEmpty(bookTitle) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(loanDate) ||
                string.IsNullOrEmpty(returnDate)) {
                MessageBox.Show("すべての項目を入力してください。");
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 貸出情報を Loans テーブルに登録するクエリ
                string insertQuery = @"
                INSERT INTO Loans (ISBN, MemberId, Title, Author, LoanDate, ReturnDate)
                VALUES (@ISBN, @MemberId, @Title, @Author, @LoanDate, @ReturnDate);";

                SQLiteCommand cmd = new SQLiteCommand(insertQuery, connection);
                cmd.Parameters.AddWithValue("@ISBN", isbn);
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                cmd.Parameters.AddWithValue("@Title", bookTitle);
                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@LoanDate", loanDate);
                cmd.Parameters.AddWithValue("@ReturnDate", returnDate);

                // 実行
                int result = cmd.ExecuteNonQuery();

                if (result > 0) {
                    MessageBox.Show("貸出登録が完了しました。");

                    LoadLoanHistory(); // リストを更新

                    // 入力欄をクリアする
                    ClearInputFields();

                } else {
                    MessageBox.Show("貸出登録に失敗しました。");
                }
            }
        }

        // 入力欄をクリアする
        private void ClearInputFields() {
            ISBNTextBox.Text = "";
            BarcodeTextBox.Text = "";
            MemberIDTextBox.Text = "";
            BookTitleTextBox.Text = "";
            AuthorTextBox.Text = "";
            LoanDatePicker.SelectedDate = null;
            ReturnDatePicker.SelectedDate = null;
        }

        // 検索ボタンをクリックした際に貸出履歴を検索するメソッド
        private void SearchButton_Click(object sender, RoutedEventArgs e) {
            
        }

        // 更新ボタンがクリックされた際に、選択された貸出履歴を編集できるようにする
        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            
        }



        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            // ListViewで選択されている項目を取得
            Loan selectedLoan = (Loan)LoanHistoryListView.SelectedItem;

            if (selectedLoan == null) {
                MessageBox.Show("削除する貸出データを選択してください。");
                return;
            }

            // 削除の確認ダイアログを表示
            var result = MessageBox.Show("選択された貸出データを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) {
                return;
            }

            // データベースから該当のデータを削除
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string deleteQuery = "DELETE FROM Loans WHERE Id = @Id";
                SQLiteCommand cmd = new SQLiteCommand(deleteQuery, connection);
                cmd.Parameters.AddWithValue("@Id", selectedLoan.Id);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0) {
                    MessageBox.Show("削除が完了しました。");

                    // リストビューを更新する
                    LoadLoanHistory();
                } else {
                    MessageBox.Show("削除に失敗しました。");
                }
            }
        }

        private void LoadLoanHistory() {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 貸出履歴を取得するクエリ
                string query = "SELECT Id, MemberId, Title, Author, LoanDate, ReturnDate FROM Loans";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);

                // 結果をリストに変換
                List<Loan> loans = new List<Loan>();
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        loans.Add(new Loan {
                            Id = reader.GetInt32(0),
                            MemberID = reader.GetString(1),
                            Title = reader.GetString(2),
                            Author = reader.GetString(3),
                            LoanDate = DateTime.Parse(reader.GetString(4)),
                            ReturnDate = DateTime.Parse(reader.GetString(5))
                        });
                    }
                }

                // ListView にデータを設定
                LoanHistoryListView.ItemsSource = loans;
            }
        }



        // キャンセルボタンがクリックされた際にウィンドウを閉じる処理
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            var selectionWindow = Application.Current.Windows.OfType<SelectionWindow>().FirstOrDefault();
            if (selectionWindow != null) {
                selectionWindow.Show();  // MainWindowを表示
            }

            // 現在のウィンドウを閉じる
            this.Close();
        }

    }
}
