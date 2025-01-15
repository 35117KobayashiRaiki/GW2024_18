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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookLendingSystem {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        // 会員登録ボタンがクリックされた時の処理
        private void MemberRegistrationButton_Click(object sender, RoutedEventArgs e) {
            // 会員登録画面を開く
            OpenMemberRegistrationWindow();
        }

        // 会員登録画面を開くためのメソッド
        private void OpenMemberRegistrationWindow() {
            // MainWindow が開かれている場合は非表示にする
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Hide();  // MainWindowを非表示にする
            }

            // MemberRegistrationWindow を開く
            MemberRegistrationWindow registrationWindow = new MemberRegistrationWindow();
            registrationWindow.Show();
        }

        // ログインボタンのクリックイベントハンドラ
        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            // MainWindowを非表示にしてログイン画面を開く
            this.Hide(); // MainWindowを非表示にする

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // ログイン画面が閉じられた後にMainWindowを再表示
            loginWindow.Closed += (s, args) => {
                this.Show(); // MainWindowを再表示
            };
        }
    }
}
