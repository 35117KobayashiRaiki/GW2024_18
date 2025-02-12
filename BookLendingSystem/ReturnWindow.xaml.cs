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
using static BookLendingSystem.LendingWindow;
using System.Windows.Threading;


namespace BookLendingSystem {
    /// <summary>
    /// ReturnWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ReturnWindow : Window {

        // 貸出情報を保持するクラス
        public class Loan {
            public int Id { get; set; }  // 貸出ID
            public string ISBN { get; set; }  // 書籍のISBN
            public string Barcode { get; set; }  // 書籍のバーコード
            public string MemberId { get; set; }  // 会員ID
            public string Title { get; set; }  // 書籍タイトル
            public string Author { get; set; }  // 著者名
            public DateTime LoanDate { get; set; }  // 貸出日
            public DateTime ReturnDeadline { get; set; }  // 返却期限
            public DateTime? ReturnDate { get; set; }  // 返却日（NULLの場合は未返却）
        }

        // ISBN・バーコード入力時の遅延処理用タイマー
        private DispatcherTimer _inputTimer;

        public ReturnWindow() {
            InitializeComponent();
            FetchReturnedBooks();  // 返却された書籍情報を読み込む

            // 入力遅延処理のためのタイマーを設定
            _inputTimer = new DispatcherTimer();
            _inputTimer.Interval = TimeSpan.FromMilliseconds(300);  // 300msの遅延
            _inputTimer.Tick += InputTimer_Tick;
        }

        // ISBNの入力が変更されたときの処理
        private void ISBNTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            _inputTimer.Stop();
            _inputTimer.Start();  // タイマーをリセットして再スタート
        }

        // バーコードの入力が変更されたときの処理
        private void BarcodeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            _inputTimer.Stop();
            _inputTimer.Start();
        }

        // タイマーが発火した際に貸出情報を検索
        private void InputTimer_Tick(object sender, EventArgs e) {
            _inputTimer.Stop();
            FetchLoanInfo();  // ISBNとバーコードから貸出情報を取得
        }

        // 返却登録処理
        private void ReturnRegistrationButton_Click(object sender, RoutedEventArgs e) {
            string isbn = ISBNTextBox.Text;
            string barcode = BarcodeTextBox.Text;
            DateTime returnDate = DateTime.Now; // 返却日は現在の日付

            using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                conn.Open();

                // ISBNとバーコードが一致し、未返却のレコードの返却日を更新
                string updateQuery = "UPDATE Loans SET ReturnDate = @ReturnDate WHERE ISBN = @ISBN AND Barcode = @Barcode AND ReturnDate IS NULL";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn)) {
                    cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                    cmd.Parameters.AddWithValue("@ISBN", isbn);
                    cmd.Parameters.AddWithValue("@Barcode", barcode);

                    int rowsAffected = cmd.ExecuteNonQuery();   // 実行して影響を受けた行数を取得
                    if (rowsAffected > 0) {
                        MessageBox.Show("返却登録が完了しました。");
                        ClearInputFields();  // 入力欄をクリア
                        FetchReturnedBooks();   // 返却リストを更新
                    } else {
                        MessageBox.Show("返却登録に失敗しました。");
                    }
                }
            }
        }

        // ISBNを標準化（ハイフン削除・空白削除）
        private string NormalizeISBN(string isbn) {
            return isbn.Replace("-", "").Trim();  // ハイフン削除 + 余分な空白削除
        }

        // ISBNとバーコードを元に貸出情報を取得
        private void FetchLoanInfo() {
            // 入力されたISBNとバーコードを取得し、不要な空白を削除
            string isbn = NormalizeISBN(ISBNTextBox.Text).Trim();
            string barcode = BarcodeTextBox.Text.Trim();

            // ISBNとバーコードの両方が入力されている場合のみデータベース検索を実行
            if (!string.IsNullOrEmpty(isbn) && !string.IsNullOrEmpty(barcode)) {
                using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                    conn.Open();    // SQLiteデータベースに接続

                    // ISBNとバーコードが一致し、まだ返却されていないレコードを検索
                    string query = "SELECT * FROM Loans WHERE CAST(ISBN AS TEXT) = @ISBN AND CAST(Barcode AS TEXT) = @Barcode AND ReturnDate IS NULL";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) {
                        cmd.Parameters.AddWithValue("@ISBN", isbn);
                        cmd.Parameters.AddWithValue("@Barcode", barcode);

                        using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                            // データが存在する場合
                            if (reader.Read()) {
                                // データベースから取得した値を対応するテキストボックスに表示
                                MemberIDTextBox.Text = reader["MemberId"].ToString(); // 会員ID
                                BookTitleTextBox.Text = reader["Title"].ToString(); // 書籍タイトル
                                AuthorTextBox.Text = reader["Author"].ToString(); // 著者名
                                LoanDatePicker.SelectedDate = Convert.ToDateTime(reader["LoanDate"]); // 貸出日
                                ReturnDeadlinePicker.SelectedDate = Convert.ToDateTime(reader["ReturnDeadline"]); // 返却期限

                                // 返却日を自動的に設定 (スキャンした日)
                                ReturnDatePicker.SelectedDate = DateTime.Now;
                            } else {
                                //MessageBox.Show("該当する貸出情報が見つかりませんでした。");
                            }
                        }
                    }
                }
            }
        }

        // 入力フィールドをクリア
        private void ClearInputFields() {
            ISBNTextBox.Clear();
            BarcodeTextBox.Clear();
            MemberIDTextBox.Clear();
            BookTitleTextBox.Clear();
            AuthorTextBox.Clear();
            LoanDatePicker.SelectedDate = null;
            ReturnDeadlinePicker.SelectedDate = null;
            ReturnDatePicker.SelectedDate = null;
            SearchTextBox.Clear();
        }

        // 選択した書籍の返却日を更新（返却日を変更）
        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            string isbn = ISBNTextBox.Text;
            string barcode = BarcodeTextBox.Text;
            DateTime? returnDate = ReturnDatePicker.SelectedDate;   // ユーザーが選択した返却日

            // 返却日が選択されている場合にのみ更新処理を実行
            if (returnDate.HasValue) {
                using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                    conn.Open();

                    // 返却日を更新するSQLクエリ
                    string updateQuery = "UPDATE Loans SET ReturnDate = @ReturnDate WHERE ISBN = @ISBN AND Barcode = @Barcode";
                    using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn)) {
                        cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                        cmd.Parameters.AddWithValue("@ISBN", isbn);
                        cmd.Parameters.AddWithValue("@Barcode", barcode);

                        int rowsAffected = cmd.ExecuteNonQuery();   // クエリ実行
                        if (rowsAffected > 0) {
                            MessageBox.Show("返却日が更新されました。");
                            ClearInputFields();     // 入力フィールドをクリア
                            FetchReturnedBooks();  // 返却された書籍リストを更新
                        } else {
                            MessageBox.Show("返却日の更新に失敗しました。");
                        }
                    }
                }
            } else {
                MessageBox.Show("返却日を選択してください。");
            }
        }

        // 選択した貸出履歴の返却日を削除（返却情報をリセット）
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            // ユーザーが選択した貸出情報を取得
            Loan selectedLoan = (Loan)LoanHistoryListView.SelectedItem;

            // 選択されたデータがある場合のみ処理
            if (selectedLoan != null) {
                // 返却日が既に登録されている場合のみ削除可能
                if (selectedLoan.ReturnDate != null) {
                    using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                        conn.Open();

                        // 返却日をNULLにするSQLクエリ（返却日をリセット）
                        string updateQuery = "UPDATE Loans SET ReturnDate = NULL WHERE Id = @Id";
                        using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn)) {
                            cmd.Parameters.AddWithValue("@Id", selectedLoan.Id);

                            int rowsAffected = cmd.ExecuteNonQuery();   // クエリ実行
                            if (rowsAffected > 0) {
                                MessageBox.Show("返却日を削除しました。");
                                // 入力フィールドをクリア
                                ClearInputFields();
                                // 再度返却された書籍を表示
                                FetchReturnedBooks();
                            } else {
                                MessageBox.Show("返却日の削除に失敗しました。");
                            }
                        }
                    }
                } else {
                    MessageBox.Show("返却日は既に削除されています。");
                }
            } else {
                MessageBox.Show("返却する書籍を選択してください。");
            }
        }

        // 検索ボタンが押されたときに、入力されたキーワードで貸出履歴を検索する
        private void SearchButton_Click(object sender, RoutedEventArgs e) {
            string keyword = SearchTextBox.Text;  // ユーザーが入力した検索キーワードを取得
            string query = @"
            SELECT Id, ISBN, Barcode, MemberId, Title, Author, LoanDate, ReturnDeadline, ReturnDate
            FROM Loans
            WHERE ISBN LIKE @Keyword 
            OR Barcode LIKE @Keyword 
            OR MemberId LIKE @Keyword 
            OR Title LIKE @Keyword 
            OR Author LIKE @Keyword 
            OR LoanDate LIKE @Keyword 
            OR ReturnDeadline LIKE @Keyword
            OR ReturnDate LIKE @Keyword";

            // キーワードに % を追加して部分一致検索を実行
            keyword = "%" + keyword + "%";  // 部分一致検索のため両端に % を追加

            using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@Keyword", keyword);

                    using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                        List<Loan> loanHistory = new List<Loan>();
                        while (reader.Read()) {
                            Loan loan = new Loan {
                                Id = Convert.ToInt32(reader["Id"]),
                                ISBN = reader["ISBN"].ToString(),
                                Barcode = reader["Barcode"].ToString(),
                                MemberId = reader["MemberId"].ToString(),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                LoanDate = Convert.ToDateTime(reader["LoanDate"]),
                                ReturnDeadline = Convert.ToDateTime(reader["ReturnDeadline"]),
                                ReturnDate = reader["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnDate"]) : (DateTime?)null
                            };
                            loanHistory.Add(loan);
                        }
                        // 検索結果を表示（例えば、ListViewなどに表示）
                        LoanHistoryListView.ItemsSource = loanHistory;

                        ISBNTextBox.Clear();
                        BarcodeTextBox.Clear();
                        MemberIDTextBox.Clear();
                        BookTitleTextBox.Clear();
                        AuthorTextBox.Clear();
                        LoanDatePicker.SelectedDate = null;
                        ReturnDeadlinePicker.SelectedDate = null;
                        ReturnDatePicker.SelectedDate = null;
                    }
                }
            }
        }

        // すべて入力欄リセットと返却履歴のリストの更新
        private void SearchResetButton_Click(object sender, RoutedEventArgs e) {
            ClearInputFields();
            FetchReturnedBooks();
        }

        // 貸出履歴を取得し、返却済みの書籍情報を表示する
        private void FetchReturnedBooks() {
            using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                conn.Open();

                // 返却された書籍のみ取得
                string query = "SELECT * FROM Loans WHERE ReturnDate IS NOT NULL";  // 返却された書籍をフィルタリング
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) {
                    using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                        List<Loan> returnedBooks = new List<Loan>();
                        while (reader.Read()) {
                            Loan loan = new Loan {
                                Id = Convert.ToInt32(reader["Id"]),  // 貸出IDを取得
                                ISBN = reader["ISBN"].ToString(),    // ISBNを取得
                                Barcode = reader["Barcode"].ToString(), // バーコードを取得
                                MemberId = reader["MemberId"].ToString(),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                LoanDate = Convert.ToDateTime(reader["LoanDate"]),
                                ReturnDeadline = Convert.ToDateTime(reader["ReturnDeadline"]),
                                ReturnDate = reader["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnDate"]) : (DateTime?)null
                            };
                            returnedBooks.Add(loan);
                        }

                        // LoanHistoryListViewに返却された書籍情報を表示
                        LoanHistoryListView.ItemsSource = returnedBooks;
                    }
                }
            }
        }

        // 返却履歴のリストが選択されたときの処理
        private void LoanHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Loan selectedLoan = (Loan)LoanHistoryListView.SelectedItem;

            if (selectedLoan != null) {
                ISBNTextBox.Text = selectedLoan.ISBN;
                BarcodeTextBox.Text = selectedLoan.Barcode;
                MemberIDTextBox.Text = selectedLoan.MemberId;
                BookTitleTextBox.Text = selectedLoan.Title;
                AuthorTextBox.Text = selectedLoan.Author;
                LoanDatePicker.SelectedDate = selectedLoan.LoanDate;
                ReturnDeadlinePicker.SelectedDate = selectedLoan.ReturnDeadline;
                ReturnDatePicker.SelectedDate = selectedLoan?.ReturnDate;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            // MainWindowがすでに開かれている場合に再表示する
            var selectionWindow = Application.Current.Windows.OfType<SelectionWindow>().FirstOrDefault();
            if (selectionWindow != null) {
                selectionWindow.Show();  // MainWindowを表示
            }

            // 現在のウィンドウ（MemberRegistrationWindow）を閉じる
            this.Close();
        }
    }
}
