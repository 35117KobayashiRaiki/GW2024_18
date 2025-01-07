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

        // 貸出ボタンがクリックされた時の処理
        private void LendingButton_Click(object sender, RoutedEventArgs e) {
            // 貸出画面を開く
            LendingWindow lendingWindow = new LendingWindow();
            lendingWindow.Show();  // 新しいウィンドウを表示
            this.Close();       // メイン画面を閉じる場合（必要なら）
        }

        // 返却ボタンのクリックイベントハンドラ
        private void ReturnButton_Click(object sender, RoutedEventArgs e) {
            // 返却画面を開く
            ReturnWindow returnWindow = new ReturnWindow();
            returnWindow.Show();
            this.Close(); // メイン画面を閉じる場合（必要なら）
        }

        // 管理貸出権限ボタンのクリックイベント
        private void LoanAdministratorPrivilegesButton_Click(object sender, RoutedEventArgs e) {
            // 管理者貸出権限画面を開く
            LoanAdministratorPrivilegesWindow adminWindow = new LoanAdministratorPrivilegesWindow();
            adminWindow.Show();  // 新しいウィンドウを表示
            this.Close();        // メインウィンドウを閉じる（必要に応じて）
        }

        // 管理返却権限ボタンのクリックイベント
        private void ReturnAdministratorPrivilegesButton_Click(object sender, RoutedEventArgs e) {
            // 管理者返却権限画面を開く
            ReturnAdministratorPrivilegesWindow adminReturnWindow = new ReturnAdministratorPrivilegesWindow();
            adminReturnWindow.Show();
            this.Close(); // メインウィンドウを閉じる（必要に応じて）
        }
    }
}
