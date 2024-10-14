using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WebSocket4Net;


namespace power_market_app{
    public partial class PowerMarket : Form{

        private WebSocket _webSocket;
        private string _stompDelimiter = "\u0000"; // STOMP-терминатор

        public PowerMarket(){

            InitializeComponent();
            this.Text = "Power Market";
            this.BackColor = Color.LightBlue;

            Button connectButton = new Button();
            connectButton.Text = "Подключиться";
            connectButton.Location = new Point(50, 50);
            connectButton.Size = new Size(100, 50);
            connectButton.Click += ConnectButton_Click;
            this.Controls.Add(connectButton);

            Button sendButton = new Button();
            sendButton.Text = "Отправить";
            sendButton.Location = new Point(50, 120);
            sendButton.Size = new Size(100, 50);
            sendButton.Click += SendButton_Click;
            this.Controls.Add(sendButton);
        }
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            _webSocket = new WebSocket("ws://localhost:8080/portfolio");
            _webSocket.Opened += WebSocket_Opened;
            _webSocket.MessageReceived += WebSocket_MessageReceived;
            _webSocket.Error += WebSocket_Error;
            _webSocket.Closed += WebSocket_Closed;
            _webSocket.Open();
        }

        private void WebSocket_Opened(object sender, EventArgs e)
        {
            // Отправка STOMP CONNECT-фрейма
            string connectFrame =
                "CONNECT\n" +
                "accept-version:1.2\n" +
                "host:localhost\n" +
                "heart-beat:10000,10000\n\n" +
                _stompDelimiter;

            _webSocket.Send(connectFrame);
            Console.WriteLine("Отправлен CONNECT-фрейм");
        }
        private void SendButton_Click(object sender, EventArgs e)
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                // Отправка STOMP SEND-фрейма
                string sendFrame =
                    "SEND\n" +
                    "destination:/app/greeting\n" +
                    "content-type:text/plain\n\n" +
                    "Привет, мир!\n" +
                    _stompDelimiter;

                _webSocket.Send(sendFrame);
                MessageBox.Show("Сообщение отправлено");
            }
            else
            {
                MessageBox.Show("Сначала подключитесь");
            }
        }

        private void WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            MessageBox.Show("Ошибка WebSocket: " + e.Exception.Message);
        }

        private void WebSocket_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("Соединение с WebSocket закрыто");
        }

        private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string message = e.Message.TrimEnd('\0'); // Удаляем терминатор
            Console.WriteLine("Получено сообщение:\n" + message);

            if (message.StartsWith("CONNECTED"))
            {
                // После подключения подписываемся на тему
                string subscribeFrame =
                    "SUBSCRIBE\n" +
                    "id:sub-0\n" +
                    "destination:/topic/greeting\n\n" +
                    _stompDelimiter;

                _webSocket.Send(subscribeFrame);
                Console.WriteLine("Отправлен SUBSCRIBE-фрейм");
            }
            else if (message.StartsWith("MESSAGE"))
            {
                // Обработка входящих сообщений
                string[] lines = message.Split('\n');
                string body = "";
                bool isBody = false;
                foreach (var line in lines)
                {
                    if (isBody)
                    {
                        body += line;
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        isBody = true;
                    }
                }
                MessageBox.Show("Получено сообщение: " + body);
            }
            
        }
    }
}
