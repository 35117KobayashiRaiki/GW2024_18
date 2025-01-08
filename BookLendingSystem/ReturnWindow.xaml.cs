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
    /// ReturnWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ReturnWindow : Window {
        public ReturnWindow() {
            InitializeComponent();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e) {

        }

        // ここでウィンドウが閉じられたときの処理が開始されます
        private void Window_Closed(object sender, EventArgs e) {
            // MainWindowが再表示される
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
