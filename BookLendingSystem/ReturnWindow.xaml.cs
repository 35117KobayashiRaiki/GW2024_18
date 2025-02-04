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
    /// ReturnWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ReturnWindow : Window {
        public ReturnWindow() {
            InitializeComponent();

        }

        private void ReturnRegistrationButton_Click(object sender, RoutedEventArgs e) {
            string isbn = ISBNTextBox.Text.Trim();
            string barcode = BarcodeTextBox.Text.Trim();

            // 入力チェック
            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(barcode)) {
                MessageBox.Show("すべての情報を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = new SQLiteConnection("Data Source=library.db;Version=3;")) {
                connection.Open();

                // 貸出情報があるかチェック
                string loanCheckQuery = "SELECT * FROM Loans WHERE ISBN = @ISBN AND Barcode = @Barcode";
                using (var loanCheckCmd = new SQLiteCommand(loanCheckQuery, connection)) {
                    loanCheckCmd.Parameters.AddWithValue("@ISBN", isbn);
                    loanCheckCmd.Parameters.AddWithValue("@Barcode", barcode);

                    using (var reader = loanCheckCmd.ExecuteReader()) {
                        if (!reader.Read()) {
                            MessageBox.Show("該当の貸出情報が見つかりません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // 取得した貸出情報
                        string memberId = reader["MemberId"].ToString();
                        string bookTitle = reader["BookTitle"].ToString();
                        string author = reader["Author"].ToString();
                        DateTime loanDate = DateTime.Parse(reader["LoanDate"].ToString());
                        DateTime returnDeadline = DateTime.Parse(reader["ReturnDeadline"].ToString());
                        DateTime returnDate = DateTime.Today; // 返却日は本日

                        reader.Close(); // 読み取り終了

                        // 返却テーブルにデータを追加
                        string insertReturnQuery = @"
                        INSERT INTO Returns (ISBN, Barcode, MemberID, BookTitle, Author, LoanDate, ReturnDeadline, ReturnDate)
                        VALUES (@ISBN, @Barcode, @MemberID, @BookTitle, @Author, @LoanDate, @ReturnDeadline, @ReturnDate)";
                        using (var insertCmd = new SQLiteCommand(insertReturnQuery, connection)) {
                            insertCmd.Parameters.AddWithValue("@ISBN", isbn);
                            insertCmd.Parameters.AddWithValue("@Barcode", barcode);
                            insertCmd.Parameters.AddWithValue("@MemberID", memberId);
                            insertCmd.Parameters.AddWithValue("@BookTitle", bookTitle);
                            insertCmd.Parameters.AddWithValue("@Author", author);
                            insertCmd.Parameters.AddWithValue("@LoanDate", loanDate);
                            insertCmd.Parameters.AddWithValue("@ReturnDeadline", returnDeadline);
                            insertCmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                            insertCmd.ExecuteNonQuery();
                        }

                        // 貸出テーブルから削除
                        string deleteLoanQuery = "DELETE FROM Loans WHERE ISBN = @ISBN AND Barcode = @Barcode AND LoanDate = @LoanDate";
                        using (var deleteCmd = new SQLiteCommand(deleteLoanQuery, connection)) {
                            deleteCmd.Parameters.AddWithValue("@ISBN", isbn);
                            deleteCmd.Parameters.AddWithValue("@Barcode", barcode);
                            deleteCmd.Parameters.AddWithValue("@MemberID", memberId);
                            deleteCmd.Parameters.AddWithValue("@LoanDate", loanDate);
                            deleteCmd.Parameters.AddWithValue("@ReturnDate", returnDate);
                            deleteCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("返却が完了しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                        // UIをクリア
                        ClearReturnForm();
                    }
                }
            }
        }

        private void ClearReturnForm() {
            ISBNTextBox.Clear();
            BarcodeTextBox.Clear();
            MemberIDTextBox.Clear();
            BookTitleTextBox.Clear();
            AuthorTextBox.Clear();
            RentalDatePicker.SelectedDate = null;
            ReturnDeadlinePicker.SelectedDate = null;
            ReturnDatePicker.SelectedDate = null;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {

        }


        private void SearchButton_Click(object sender, RoutedEventArgs e) {

        }

        private void FetchLoanInformation() {
            string isbn = ISBNTextBox.Text.Trim();
            string barcode = BarcodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(barcode)) {
                return;  // ISBNとバーコードの両方が入力されていない場合、検索しない
            }

            using (var connection = new SQLiteConnection("Data Source=library.db;Version=3;")) {
                connection.Open();
                string query = "SELECT * FROM Loans WHERE ISBN = @ISBN AND Barcode = @Barcode";
                using (var command = new SQLiteCommand(query, connection)) {
                    command.Parameters.AddWithValue("@ISBN", isbn);
                    command.Parameters.AddWithValue("@Barcode", barcode);
                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            // 貸出情報を取得して UI に反映
                            MemberIDTextBox.Text = reader["MemberID"].ToString();
                            BookTitleTextBox.Text = reader["BookTitle"].ToString();
                            AuthorTextBox.Text = reader["Author"].ToString();
                            RentalDatePicker.SelectedDate = DateTime.Parse(reader["LoanDate"].ToString());
                            ReturnDeadlinePicker.SelectedDate = DateTime.Parse(reader["ReturnDeadline"].ToString());
                            ReturnDatePicker.SelectedDate = DateTime.Today;  // 返却日は本日の日付
                        } else {
                            // 貸出情報が見つからない場合は、エラーメッセージを表示
                            MessageBox.Show("貸出情報が見つかりません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void ISBNTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            FetchLoanInformation();
        }

        private void BarcodeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            FetchLoanInformation();
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
