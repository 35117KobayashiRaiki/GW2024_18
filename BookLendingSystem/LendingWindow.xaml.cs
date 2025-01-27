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

namespace BookLendingSystem {
    /// <summary>
    /// LendingWindow.xaml の相互作用ロジック
    /// 貸出システムのメインウィンドウで貸出情報を管理する機能を提供します。
    /// </summary>
    public partial class LendingWindow : Window {

        // 貸出情報を保持するクラス
        public class Loan {
            public int Id { get; set; }  // 貸出履歴のID
            public string ISBN { get; set; }  // 書籍のISBN
            public string MemberID { get; set; }  // 会員ID
            public string Title { get; set; }  // 書籍のタイトル
            public string Author { get; set; }  // 書籍の著者
            public string LoanDate { get; set; }  // 貸出日
            public string ReturnDate { get; set; }  // 返却予定日
        }

        public LendingWindow() {
            InitializeComponent();  // 初期化処理
        }

        // 貸出履歴をデータベースから読み込み、ListViewに表示するメソッド
        private void LoadLoanHistory() {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string query = "SELECT * FROM Loans";  // 貸出履歴を取得するSQL
                SQLiteCommand cmd = new SQLiteCommand(query, connection);

                var reader = cmd.ExecuteReader();
                var loans = new List<Loan>();

                // データベースから取得した貸出履歴をリストに追加
                while (reader.Read()) {
                    loans.Add(new Loan {
                        Id = reader.GetInt32(0),
                        ISBN = reader.GetString(1),
                        MemberID = reader.GetString(2),
                        Title = reader.GetString(3),
                        Author = reader.GetString(4),
                        LoanDate = reader.GetString(5),
                        ReturnDate = reader.GetString(6)
                    });
                }

                // ListViewに貸出履歴を表示
                LoanHistoryListView.ItemsSource = loans;
            }
        }

        // 入力フィールドをクリアするメソッド
        private void ClearInputFields() {
            ISBNTextBox.Clear();
            MemberIDTextBox.Clear();
            BookTitleTextBox.Clear();
            AuthorTextBox.Clear();
            LoanDatePicker.SelectedDate = null;
            ReturnDatePicker.SelectedDate = null;
        }

        // 貸出情報を登録または更新する
        private void RegisterLendingButton_Click(object sender, RoutedEventArgs e) {
            // 入力された値を取得
            string isbn = ISBNTextBox.Text.Trim();
            string memberId = MemberIDTextBox.Text.Trim();
            string title = BookTitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string loanDate = LoanDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            string returnDate = ReturnDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            // 入力値が不足していた場合、警告を表示
            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author)) {
                MessageBox.Show("すべての項目を入力してください。");
                return;
            }

            // 既存の貸出履歴を更新する場合
            if (LoanHistoryListView.SelectedItem != null) {
                var selectedLoan = LoanHistoryListView.SelectedItem as Loan;
                UpdateLoan(selectedLoan.Id);  // 貸出情報の更新
            } else {
                // 新規登録の場合、データベースに追加
                using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                    connection.Open();

                    string query = "INSERT INTO Loans (ISBN, MemberId, Title, Author, LoanDate, ReturnDate) VALUES (@ISBN, @MemberId, @Title, @Author, @LoanDate, @ReturnDate)";
                    SQLiteCommand cmd = new SQLiteCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ISBN", isbn);
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Author", author);
                    cmd.Parameters.AddWithValue("@LoanDate", loanDate);
                    cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                    cmd.ExecuteNonQuery();

                    // 登録完了メッセージ
                    MessageBox.Show("貸出が登録されました。");
                    ClearInputFields();  // 入力フィールドのクリア
                    LoadLoanHistory();  // 貸出履歴の再読み込み
                }
            }
        }

        // 書籍情報を外部APIで取得するメソッド
        private async Task<(string Title, string Author)> FetchBookInfo(string isbn) {
            using (HttpClient client = new HttpClient()) {
                string url = $"https://ndlsearch.ndl.go.jp/api/openurl?isbn={isbn}";  // 国立国会図書館のAPIを使用
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode) {
                    var content = await response.Content.ReadAsStringAsync();
                    // 書籍情報をパースしてタイトルと著者を返す（仮のデータ）
                    string title = "サンプルタイトル";
                    string author = "サンプル著者";
                    return (title, author);
                }
            }
            // APIからの応答がなかった場合、デフォルト値を返す
            return ("", "");
        }

        // 会員IDのバーコード読み取り時に会員情報を取得するメソッド
        private void MemberIDTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            string barcode = MemberIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(barcode)) return;

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string query = "SELECT MemberId FROM Members WHERE Barcode = @Barcode";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Barcode", barcode);

                var result = cmd.ExecuteScalar();

                // 会員情報が見つからない場合
                if (result != null) {
                    MemberIDTextBox.Text = result.ToString();
                } else {
                    MessageBox.Show("該当する会員が見つかりません。");
                }
            }
        }

        // 検索ボタンをクリックした際に貸出履歴を検索するメソッド
        private void SearchButton_Click(object sender, RoutedEventArgs e) {
            string searchText = SearchTextBox.Text.Trim();

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string query = "SELECT * FROM Loans WHERE Title LIKE @SearchText OR Author LIKE @SearchText";  // 検索用のSQL
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@SearchText", $"%{searchText}%");

                var reader = cmd.ExecuteReader();
                var loans = new List<Loan>();

                // 検索結果をListViewに表示
                while (reader.Read()) {
                    loans.Add(new Loan {
                        Id = reader.GetInt32(0),
                        ISBN = reader.GetString(1),
                        MemberID = reader.GetString(2),
                        Title = reader.GetString(3),
                        Author = reader.GetString(4),
                        LoanDate = reader.GetString(5),
                        ReturnDate = reader.GetString(6)
                    });
                }

                LoanHistoryListView.ItemsSource = loans;
            }
        }

        // 更新ボタンがクリックされた際に、選択された貸出履歴を編集できるようにする
        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            var selectedLoan = LoanHistoryListView.SelectedItem as Loan;

            if (selectedLoan == null) {
                MessageBox.Show("更新する貸出履歴を選択してください。");
                return;
            }

            // 入力フィールドに選択された貸出履歴のデータを表示
            ISBNTextBox.Text = selectedLoan.ISBN;
            MemberIDTextBox.Text = selectedLoan.MemberID;
            BookTitleTextBox.Text = selectedLoan.Title;
            AuthorTextBox.Text = selectedLoan.Author;
            LoanDatePicker.SelectedDate = DateTime.Parse(selectedLoan.LoanDate);
            ReturnDatePicker.SelectedDate = DateTime.Parse(selectedLoan.ReturnDate);
        }

        // 貸出履歴を更新する処理
        private void UpdateLoan(int loanId) {
            string isbn = ISBNTextBox.Text.Trim();
            string memberId = MemberIDTextBox.Text.Trim();
            string title = BookTitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string loanDate = LoanDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            string returnDate = ReturnDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string query = "UPDATE Loans SET ISBN = @ISBN, MemberId = @MemberId, Title = @Title, Author = @Author, LoanDate = @LoanDate, ReturnDate = @ReturnDate WHERE Id = @Id";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@ISBN", isbn);
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@LoanDate", loanDate);
                cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                cmd.Parameters.AddWithValue("@Id", loanId);

                cmd.ExecuteNonQuery();

                MessageBox.Show("貸出情報が更新されました。");
                ClearInputFields();  // 入力フィールドのクリア
                LoadLoanHistory();  // 貸出履歴の再読み込み
            }
        }

        // 削除ボタンがクリックされた際に貸出履歴を削除
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var selectedLoan = LoanHistoryListView.SelectedItem as Loan;

            if (selectedLoan == null) {
                MessageBox.Show("削除する貸出履歴を選択してください。");
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                string query = "DELETE FROM Loans WHERE Id = @Id";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", selectedLoan.Id);
                cmd.ExecuteNonQuery();

                MessageBox.Show("貸出履歴が削除されました。");
                LoadLoanHistory();  // 貸出履歴の再読み込み
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
