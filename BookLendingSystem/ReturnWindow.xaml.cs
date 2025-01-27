﻿using System;
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

        private void ReturnRegistrationButton_Click(object sender, RoutedEventArgs e) {

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {

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

        private void SearchButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
