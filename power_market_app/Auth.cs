using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;
using System.Text.Json;
using System.Drawing;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace power_market_app{
    public partial class Auth : Form
    {

        private TextBox emailTextBox;
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button registerButton;


        public Auth()
        {
            InitializeComponent();
            CenterToScreen();
            this.emailTextBox = new TextBox();
            this.passwordTextBox = new TextBox();
            this.loginButton = new Button();
            this.registerButton = new Button();

            // 
            // emailTextBox
            // 

            this.emailTextBox.Location = new System.Drawing.Point(90, 50);
            this.emailTextBox.Size = new System.Drawing.Size(200, 20);
            Program.SetPlaceholderBehavior(this.emailTextBox, "email");

            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(90, 100);
            this.passwordTextBox.Size = new System.Drawing.Size(200, 20);
            Program.SetPlaceholderBehavioForHiden(this.passwordTextBox, "пароль");

            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(90, 150);
            this.loginButton.Size = new System.Drawing.Size(200, 30);
            this.loginButton.Text = "Войти";
            this.loginButton.Click += new EventHandler(LoginButton_Click);

            // 
            // registerButton
            // 
            this.registerButton.Location = new System.Drawing.Point(90, 200);
            this.registerButton.Size = new System.Drawing.Size(200, 30);
            this.registerButton.Text = "Зарегистрироваться";
            this.registerButton.Click += new EventHandler(RegisterButton_Click);

            // 
            // PowerMarket
            // 
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.emailTextBox);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.registerButton);
            this.Text = "Авторизация";
            this.StartPosition = FormStartPosition.CenterScreen;

            this.ResumeLayout(false);
            this.PerformLayout();


        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string email = emailTextBox.Text;
            string password = passwordTextBox.Text;

            var authDto = new AuthorizationDTO
            {
                login = email,
                password = password
            };

            string json = JsonSerializer.Serialize(authDto);
            var data = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var response = await client.PostAsync("http://localhost:8080/auth/login", data);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
                if (responseData.ContainsKey("token"))
                {
                    MessageBox.Show("Успешно аутентифицировано!");
                    string token = responseData["token"];
                    SaveTokenSecurely(token);
                    Program.OpenForm(new Power_Market(), this);
                }
                else
                {
                    MessageBox.Show("Неправильные учетные данные.");
                }
            }
            else
            {
                MessageBox.Show("Ошибка соединения с сервером.");
            }
        }



        private void SaveTokenSecurely(string token)
        {
            string tempDir = Path.GetTempPath();
            // Определяем временный каталог
            string filePath = Path.Combine(tempDir, "token.dat");

            // Шифруем токен
            byte[] encryptedToken = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(token),
                null,
                DataProtectionScope.CurrentUser);

            // Сохраняем шифрованный токен в файл
            File.WriteAllBytes(filePath, encryptedToken);
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            Program.OpenForm(new Register(), this);
        }

    }
}
