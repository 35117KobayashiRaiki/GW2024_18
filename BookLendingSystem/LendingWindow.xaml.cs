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

        // 貸出情報を保持するクラス
        public class Loan {
            public int Id { get; set; }  // 貸出ID
            public string ISBN { get; set; }  // 書籍のISBN
            public string Barcode { get; set; }  // 書籍のバーコード
            public string MemberID { get; set; }  // 会員ID
            public string Title { get; set; }  // 書籍のタイトル
            public string Author { get; set; }  // 著者名
            public DateTime LoanDate { get; set; }  // 貸出日
            public DateTime ReturnDeadline { get; set; }  // 返却予定日
        }

        public LendingWindow() {
            InitializeComponent();  // 初期化処理
            LoadLoanHistory(); // 貸出履歴を読み込む
        }

        // ISBNから書籍情報（タイトルと著者）を非同期で取得
        private async Task<(string Title, string Author)> GetBookInfoFromISBNAsync(string isbn) {
            string apiUrl = $"https://iss.ndl.go.jp/api/opensearch?isbn={isbn}";
            using (HttpClient client = new HttpClient()) {
                try {
                    // APIにリクエストを送信
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode(); // ステータスコードが成功か確認
                    string xmlContent = await response.Content.ReadAsStringAsync(); // レスポンスを文字列として取得

                    // XMLデータを解析
                    XDocument doc = XDocument.Parse(xmlContent);
                    XNamespace dc = "http://purl.org/dc/elements/1.1/"; // 著者やタイトルを取得するための名前空間


                    // タイトルと著者を抽出
                    string title = doc.Descendants(dc + "title").FirstOrDefault()?.Value ?? "タイトル不明";

                    // 著者を抽出（最初の1人だけ取得）
                    string authorRaw = doc.Descendants("author").FirstOrDefault()?.Value ?? "著者不明";
                    // 著者が複数いる場合は最初の著者を取得
                    string author = authorRaw.Split(',').FirstOrDefault()?.Trim() ?? "著者不明";

                    // タイトルと著者を返す
                    return (title, author);
                }
                catch (Exception ex) {
                    MessageBox.Show($"エラー: {ex.Message}");
                    return ("エラー", "エラー");
                }
            }
        }

        // 貸出日と返却日を設定
        private void SetLoanDates() {
            // 現在の日付を取得
            DateTime currentDateTime = DateTime.Now;

            // 貸出日を設定
            LoanDatePicker.SelectedDate = currentDateTime;

            // 返却期限を設定（貸出日の1週間後）
            ReturnDeadlinePicker.SelectedDate = currentDateTime.AddDays(7);
            
        }

        // ISBNのテキストが変更されると呼ばれるイベントハンドラ
        private async void ISBNTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            string isbn = ISBNTextBox.Text.Trim();

            // ISBNが10桁または13桁の場合のみ処理
            if (isbn.Length == 13 || isbn.Length == 10) // ISBNの長さチェック
            {
                // ISBNからタイトルと著者を取得
                var (title, author) = await GetBookInfoFromISBNAsync(isbn);
                // 書籍タイトルを表示
                BookTitleTextBox.Text = title;
                // 著者名を表示
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

        // 「貸出登録」ボタンがクリックされたときの処理
        private void RegisterLendingButton_Click(object sender, RoutedEventArgs e) {
            // 入力されたデータを取得
            string isbn = ISBNTextBox.Text.Trim();
            string barcode = BarcodeTextBox.Text.Trim();
            string memberId = MemberIDTextBox.Text.Trim();
            string bookTitle = BookTitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string loanDate = LoanDatePicker.SelectedDate?.ToString("yyyy-MM-dd HH:mm:ss");  // 時間も含めてフォーマット
            string returnDeadline = ReturnDeadlinePicker.SelectedDate?.ToString("yyyy-MM-dd HH:mm:ss");  // 時間も含めてフォーマット

            // 入力項目がすべて記入されているか確認
            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(memberId) ||
                string.IsNullOrEmpty(bookTitle) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(loanDate) ||
                string.IsNullOrEmpty(returnDeadline)) {
                MessageBox.Show("すべての項目を入力してください。");
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 貸出情報を Loans テーブルに登録するクエリ
                string insertQuery = @"
                INSERT INTO Loans (ISBN, Barcode, MemberId, Title, Author, LoanDate, ReturnDeadline)
                VALUES (@ISBN, @Barcode, @MemberId, @Title, @Author, @LoanDate, @ReturnDeadline);";

                SQLiteCommand cmd = new SQLiteCommand(insertQuery, connection);
                cmd.Parameters.AddWithValue("@ISBN", isbn);
                cmd.Parameters.AddWithValue("@Barcode", barcode);  // ここでバーコードを挿入
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                cmd.Parameters.AddWithValue("@Title", bookTitle);
                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@LoanDate", loanDate);
                cmd.Parameters.AddWithValue("@ReturnDeadline", returnDeadline);

                // SQLクエリを実行
                int result = cmd.ExecuteNonQuery();

                if (result > 0) {
                    MessageBox.Show("貸出登録が完了しました。");

                    LoadLoanHistory(); // 貸出履歴を再読み込み

                    // 入力欄をクリアする
                    ClearInputFields();
                } else {
                    MessageBox.Show("貸出登録に失敗しました。");
                }
            }
        }


        // 入力欄をクリアするメソッド
        private void ClearInputFields() {
            SearchTextBox.Text = "";
            ISBNTextBox.Text = "";
            BarcodeTextBox.Text = "";
            MemberIDTextBox.Text = "";
            BookTitleTextBox.Text = "";
            AuthorTextBox.Text = "";
            LoanDatePicker.SelectedDate = null;
            ReturnDeadlinePicker.SelectedDate = null;
        }

        // 検索ボタンをクリックしたときの処理
        private void SearchButton_Click(object sender, RoutedEventArgs e) {
            string keyword = SearchTextBox.Text.Trim();  // 検索ボックスからキーワードを取得

            // 検索テキストが空でない場合、貸出履歴を検索
            if (string.IsNullOrEmpty(keyword)) {
                MessageBox.Show("検索キーワードを入力してください。");
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 貸出履歴を検索するSQLクエリ
                string query = @"
                SELECT Id, ISBN, Barcode, MemberId, Title, Author, LoanDate, ReturnDeadline
                FROM Loans
                WHERE ISBN LIKE @Keyword 
                OR Barcode LIKE @Keyword 
                OR MemberId LIKE @Keyword 
                OR Title LIKE @Keyword 
                OR Author LIKE @Keyword 
                OR LoanDate LIKE @Keyword 
                OR ReturnDeadline LIKE @Keyword";

                SQLiteCommand cmd = new SQLiteCommand(query, connection);
                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");  // 部分一致検索

                // 結果をリストに変換
                List<Loan> loans = new List<Loan>();
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        loans.Add(new Loan {
                            Id = reader.GetInt32(0),
                            ISBN = reader.GetString(1),
                            Barcode = reader.GetString(2),
                            MemberID = reader.GetString(3),
                            Title = reader.GetString(4),
                            Author = reader.GetString(5),
                            LoanDate = DateTime.Parse(reader.GetString(6)),
                            ReturnDeadline = DateTime.Parse(reader.GetString(7))
                        });
                    }
                }

                // ListViewに貸出履歴を表示
                LoanHistoryListView.ItemsSource = loans;

                // 入力欄をクリアする
                ISBNTextBox.Text = "";
                BarcodeTextBox.Text = "";
                MemberIDTextBox.Text = "";
                BookTitleTextBox.Text = "";
                AuthorTextBox.Text = "";
                LoanDatePicker.SelectedDate = null;
                ReturnDeadlinePicker.SelectedDate = null;
            }
        }

        // 検索リセットボタンをクリックしたときの処理
        private void SearchResetButton_Click(object sender, RoutedEventArgs e) {
            LoadLoanHistory();  // 検索ボックスをクリア
            ClearInputFields(); // 全ての貸出履歴を再読み込み
        }

        // 更新ボタンがクリックされた際に、選択された貸出履歴を編集できるようにする
        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            // 入力されたデータを取得
            string isbn = ISBNTextBox.Text.Trim();
            string barcode = BarcodeTextBox.Text.Trim();
            string memberId = MemberIDTextBox.Text.Trim();
            string bookTitle = BookTitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string loanDate = LoanDatePicker.SelectedDate?.ToString("yyyy-MM-dd HH:mm:ss");  // 時間も含めてフォーマット
            string returnDeadline = ReturnDeadlinePicker.SelectedDate?.ToString("yyyy-MM-dd HH:mm:ss");  // 時間も含めてフォーマット

            // 入力項目がすべて記入されているか確認
            if (string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(memberId) ||
                string.IsNullOrEmpty(bookTitle) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(loanDate) ||
                string.IsNullOrEmpty(returnDeadline)) {
                MessageBox.Show("すべての項目を入力してください。");
                return;
            }

            // ListViewで選択されている項目を取得
            Loan selectedLoan = (Loan)LoanHistoryListView.SelectedItem;

            if (selectedLoan == null) {
                MessageBox.Show("更新する貸出データを選択してください。");
                return;
            }

            // データベースの貸出情報を更新する
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 貸出情報を更新するクエリ
                string updateQuery = @"
                UPDATE Loans
                SET ISBN = @ISBN, Barcode = @Barcode, MemberId = @MemberId, Title = @Title, Author = @Author, LoanDate = @LoanDate, ReturnDeadline = @ReturnDeadline
                WHERE Id = @Id;";

                SQLiteCommand cmd = new SQLiteCommand(updateQuery, connection);
                cmd.Parameters.AddWithValue("@ISBN", isbn);
                cmd.Parameters.AddWithValue("@Barcode", barcode);
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                cmd.Parameters.AddWithValue("@Title", bookTitle);
                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@LoanDate", loanDate);
                cmd.Parameters.AddWithValue("@ReturnDeadline", returnDeadline);
                cmd.Parameters.AddWithValue("@Id", selectedLoan.Id);  // 更新対象のID

                // 実行
                int result = cmd.ExecuteNonQuery();

                if (result > 0) {
                    MessageBox.Show("貸出情報が更新されました。");

                    LoadLoanHistory(); // リストを更新

                    // 入力欄をクリアする
                    ClearInputFields();
                } else {
                    MessageBox.Show("更新に失敗しました。");
                }
            }
        }

        // 選択した貸出情報を削除するメソッド
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

                    // 入力欄をクリアする
                    ClearInputFields();
                } else {
                    MessageBox.Show("削除に失敗しました。");
                }
            }
        }

        // 貸出履歴を全て読み込むメソッド
        private void LoadLoanHistory() {
            using (SQLiteConnection connection = new SQLiteConnection(App.DbConnectionString)) {
                connection.Open();

                // 貸出履歴を取得するクエリ
                string query = "SELECT Id, ISBN, Barcode, MemberId, Title, Author, LoanDate, ReturnDeadline FROM Loans";
                SQLiteCommand cmd = new SQLiteCommand(query, connection);

                // 結果をリストに変換
                List<Loan> loans = new List<Loan>();
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        loans.Add(new Loan {
                            Id = reader.GetInt32(0),
                            ISBN = reader.GetString(1),  // ISBNを設定
                            Barcode = reader.GetString(2),  // Barcodeを設定
                            MemberID = reader.GetString(3),
                            Title = reader.GetString(4),
                            Author = reader.GetString(5),
                            LoanDate = DateTime.Parse(reader.GetString(6)),
                            ReturnDeadline = DateTime.Parse(reader.GetString(7))
                        });
                    }
                }

                // ListView にデータを設定
                LoanHistoryListView.ItemsSource = loans;
            }
        }

        // ListViewで選択された貸出情報をフォームに表示
        private void LoanHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Loan selectedLoan = (Loan)LoanHistoryListView.SelectedItem;

            if (selectedLoan != null) {
                ISBNTextBox.Text = selectedLoan.ISBN;
                BarcodeTextBox.Text = selectedLoan.Barcode;
                MemberIDTextBox.Text = selectedLoan.MemberID;
                BookTitleTextBox.Text = selectedLoan.Title;
                AuthorTextBox.Text = selectedLoan.Author;
                LoanDatePicker.SelectedDate = selectedLoan.LoanDate;
                ReturnDeadlinePicker.SelectedDate = selectedLoan.ReturnDeadline; ;
            }
        }

        // 「キャンセル」ボタンをクリックしたときの処理
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
