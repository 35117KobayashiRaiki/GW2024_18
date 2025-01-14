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
    /// LoginWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginWindow : Window {
        public LoginWindow() {
            InitializeComponent();
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

        private void RegistrationButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
