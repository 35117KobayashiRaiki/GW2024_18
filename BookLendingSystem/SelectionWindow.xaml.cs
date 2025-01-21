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
    /// SelectionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectionWindow : Window {
        public SelectionWindow() {
            InitializeComponent();
        }

        // 貸出ボタンがクリックされた時の処理
        private void LendingButton_Click(object sender, RoutedEventArgs e) {
            // 貸出画面を開く
            LendingWindow lendingWindow = new LendingWindow();
            lendingWindow.Show();  // 新しいウィンドウを表示

            // メイン画面を非表示にする
            this.Hide();

            // LendingWindowが閉じられたら、メイン画面を再表示する
            lendingWindow.Closed += (s, args) => {
                this.Show();
            };
        }

        // 返却ボタンのクリックイベントハンドラ
        private void ReturnButton_Click(object sender, RoutedEventArgs e) {
            // 返却画面を開く
            ReturnWindow returnWindow = new ReturnWindow();
            returnWindow.Show();

            // メイン画面を非表示にする
            this.Hide();

            // returnWindowが閉じられたら、メイン画面を再表示する
            returnWindow.Closed += (s, args) => {
                this.Show();
            };
        }

        private void Back_Click(object sender, RoutedEventArgs e) {
            // MainWindowがすでに開かれている場合に再表示する
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();  // MainWindowを表示
            }

            // 現在のウィンドウ（MemberRegistrationWindow）を閉じる
            this.Close();
        }

        // ウィンドウが閉じられたときにMainWindowを表示する
        private void Window_Closed(object sender, EventArgs e) {
            // MainWindowを再表示する
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null) {
                mainWindow.Show();  // MainWindowを表示
            }
        }

    }
}
