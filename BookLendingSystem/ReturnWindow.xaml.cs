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

        public class Loan {
            public int Id { get; set; }  // 貸出ID
            public string ISBN { get; set; }  // 書籍のISBN
            public string Barcode { get; set; }  // 書籍のバーコード
            public string MemberId { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime ReturnDeadline { get; set; }
            public DateTime? ReturnDate { get; set; }
        }

        private DispatcherTimer _fetchInfoTimer;

        public ReturnWindow() {
            InitializeComponent();
            FetchReturnedBooks();  // 返却された書籍情報を読み込む

        }

        private void ReturnRegistrationButton_Click(object sender, RoutedEventArgs e) {
            string isbn = ISBNTextBox.Text;
            string barcode = BarcodeTextBox.Text;
            DateTime returnDate = DateTime.Now;

            using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                conn.Open();
                string updateQuery = "UPDATE Loans SET ReturnDate = @ReturnDate WHERE ISBN = @ISBN AND Barcode = @Barcode AND ReturnDate IS NULL";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn)) {
                    cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                    cmd.Parameters.AddWithValue("@ISBN", isbn);
                    cmd.Parameters.AddWithValue("@Barcode", barcode);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0) {
                        MessageBox.Show("返却登録が完了しました。");
                        ClearInputFields();  // 入力欄をクリア
                        FetchReturnedBooks();
                    } else {
                        MessageBox.Show("返却登録に失敗しました。");
                    }
                }
            }
        }


        private void ISBNTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            FetchLoanInfo();
        }

        private void BarcodeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            FetchLoanInfo();
        }

        private void FetchLoanInfo() {
            string isbn = ISBNTextBox.Text;
            string barcode = BarcodeTextBox.Text;

            // ISBNとバーコードの両方が入力されている場合のみ検索
            if (!string.IsNullOrEmpty(isbn) && !string.IsNullOrEmpty(barcode)) {
                using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                    conn.Open();
                    string query = "SELECT * FROM Loans WHERE ISBN = @ISBN AND Barcode = @Barcode AND ReturnDate IS NULL";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) {
                        cmd.Parameters.AddWithValue("@ISBN", isbn);
                        cmd.Parameters.AddWithValue("@Barcode", barcode);

                        using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                            if (reader.Read()) {
                                // 借りていた情報を表示
                                MemberIDTextBox.Text = reader["MemberId"].ToString();
                                BookTitleTextBox.Text = reader["Title"].ToString();
                                AuthorTextBox.Text = reader["Author"].ToString();
                                LoanDatePicker.SelectedDate = Convert.ToDateTime(reader["LoanDate"]);
                                ReturnDeadlinePicker.SelectedDate = Convert.ToDateTime(reader["ReturnDeadline"]);

                                // 返却日を自動的に設定 (スキャンした日)
                                ReturnDatePicker.SelectedDate = DateTime.Now;
                            } else {
                                MessageBox.Show("該当する貸出情報が見つかりませんでした。");
                            }
                        }
                    }
                }
            }
        }

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


        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            string isbn = ISBNTextBox.Text;
            string barcode = BarcodeTextBox.Text;
            DateTime? returnDate = ReturnDatePicker.SelectedDate;

            // 返却日が選択されている場合にのみ更新処理を実行
            if (returnDate.HasValue) {
                using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                    conn.Open();
                    string updateQuery = "UPDATE Loans SET ReturnDate = @ReturnDate WHERE ISBN = @ISBN AND Barcode = @Barcode";
                    using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn)) {
                        cmd.Parameters.AddWithValue("@ReturnDate", returnDate.Value);
                        cmd.Parameters.AddWithValue("@ISBN", isbn);
                        cmd.Parameters.AddWithValue("@Barcode", barcode);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0) {
                            MessageBox.Show("返却日が更新されました。");
                            ClearInputFields();
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            // 選択された貸出情報を取得
            Loan selectedLoan = (Loan)LoanHistoryListView.SelectedItem;

            if (selectedLoan != null) {
                // 返却日が既にNULLでない場合のみ処理
                if (selectedLoan.ReturnDate != null) {
                    using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                        conn.Open();
                        string updateQuery = "UPDATE Loans SET ReturnDate = NULL WHERE Id = @Id";
                        using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn)) {
                            cmd.Parameters.AddWithValue("@Id", selectedLoan.Id);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0) {
                                MessageBox.Show("返却日を削除しました。");
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

        private void SearchResetButton_Click(object sender, RoutedEventArgs e) {
            ClearInputFields();
            FetchReturnedBooks();
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

        

        private void FetchReturnedBooks() {
            using (SQLiteConnection conn = new SQLiteConnection(App.DbConnectionString)) {
                conn.Open();
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
    }
}
