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
            // MainWindowを非表示にしてログイン画面を開く
            this.Hide(); // MainWindowを非表示にする

            MemberRegistrationWindow MemberRegistrationloginWindow = new MemberRegistrationWindow();
            MemberRegistrationloginWindow.Show();

            // ログイン画面が閉じられた後にMainWindowを再表示
            MemberRegistrationloginWindow.Closed += (s, args) => {
                this.Show(); // MainWindowを再表示
            };
        }



        // ログインボタンのクリックイベントハンドラ
        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            // ログイン画面を表示
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // この画面を非表示にする
            this.Hide();

            // ログイン画面が閉じられたとき、この画面を再表示する
            loginWindow.Closed += (s, args) => {
                if (!loginWindow.IsLoggedIn) { // ログイン成功していない場合
                    this.Show();
                }
            };
        }
    }
}
