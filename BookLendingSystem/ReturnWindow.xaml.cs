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

            // ISBN と バーコードが両方とも入力されている場合に検索を行う
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
                                RentalDatePicker.SelectedDate = Convert.ToDateTime(reader["LoanDate"]);
                                ReturnDeadlinePicker.SelectedDate = Convert.ToDateTime(reader["ReturnDeadline"]);

                                // 返却日を自動的に設定 (スキャンした日)
                                ReturnDatePicker.SelectedDate = DateTime.Now;
                            } else {
                                MessageBox.Show("貸出情報が見つかりませんでした。");
                            }
                        }
                    }
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {

        }


        private void SearchButton_Click(object sender, RoutedEventArgs e) {

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

        private void SearchResetButton_Click(object sender, RoutedEventArgs e) {

        }

        private void LoanHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
    }
}
