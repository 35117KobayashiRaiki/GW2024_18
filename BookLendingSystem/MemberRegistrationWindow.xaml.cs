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


namespace BookLendingSystem {
    /// <summary>
    /// MemberRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MemberRegistrationWindow : Window {

        // SQLiteデータベースの接続文字列
        private string connectionString = "Data Soure=MemberRegistration.db;Version=3;";

        public MemberRegistrationWindow() {
            InitializeComponent();
        }

        // 会員情報をデータベースに登録
        private void RegisterMember(string memberId, string barcode) {
            
            using(SQLiteConnection coon = new SQLiteConnection(connectionString)) {

            }
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e) {

            // バーコードから会員IDを生成
            string barcode = LenderNameTextBox.Text;

            if (!string.IsNullOrEmpty(barcode)) {

                // 会員IDの自動生成
                string memberId = GenerateMemberId(barcode);

                //　会員情報をデータベースに登録
                regi
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            // MainWindowがすでに開かれている場合に再表示する
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();  // MainWindowを表示
            }

            // 現在のウィンドウ（MemberRegistrationWindow）を閉じる
            this.Close();
        }
    }
}
