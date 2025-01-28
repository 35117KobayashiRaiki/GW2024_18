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

        public LendingWindow() {
            InitializeComponent();  // 初期化処理
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


        // 貸出情報を登録または更新する
        private void RegisterLendingButton_Click(object sender, RoutedEventArgs e) {
            
        }


        // 会員IDのバーコード読み取り時に会員情報を取得するメソッド
        private void MemberIDTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            
        }

        // 検索ボタンをクリックした際に貸出履歴を検索するメソッド
        private void SearchButton_Click(object sender, RoutedEventArgs e) {
            
        }

        // 更新ボタンがクリックされた際に、選択された貸出履歴を編集できるようにする
        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            
        }

        // 貸出履歴を更新する処理
        private void UpdateLoan(int loanId) {
            
        }

        // 削除ボタンがクリックされた際に貸出履歴を削除
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            
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
