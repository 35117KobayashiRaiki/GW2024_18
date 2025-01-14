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
            MemberRegistrationWindow memberRegistrationWindow = new MemberRegistrationWindow();
            memberRegistrationWindow.Show();  // 新しいウィンドウを表示

            this.Hide();  // MainWindowを非表示にする
        }

        // ログインボタンのクリックイベントハンドラ
        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            // ログイン画面を開く
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // メイン画面を非表示にする
            this.Hide();

            // loginWindowが閉じられたら、メイン画面を再表示する
            loginWindow.Closed += (s, args) => {
                this.Show();
            };
        }
    }
}
