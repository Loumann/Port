using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortScanner
{
    public class URS
    {
        private readonly string serverURL = "http://localhost:8081";

        private readonly int _ursReadTimeout = 2000;
        private readonly int _ursWriteTimeout = 2000;
        private readonly int _ursBaudRate = 19200;
        static bool session;

        private SerialPort? _ursPort;

        public URS()
        {
            _ursPort = null;
        }

        public void ConnectToDevice()
        {
            ConsolePrint("Выберите порт, к которому подключено средство диагностики:");

            var ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                ConsolePrint("[{0}] {1}", i, ports[i]);
            }

            var portName = Console.ReadLine();
            try
            {
                _ursPort = new SerialPort { PortName = portName };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Неверно был выбран порт для подключения средства диагностики.");
            }
            _ursPort.ReadTimeout = _ursReadTimeout;
            _ursPort.WriteTimeout = _ursWriteTimeout;
            _ursPort.BaudRate = _ursBaudRate;

            _ursPort.DataReceived += UrsPortHandler;

            _ursPort.Open();
            ConsolePrint("Средство диагностики успешно подключено!");

            while (true) { Thread.Sleep(1000); }
        }

        private async void UrsPortHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var recievedData = _ursPort.ReadExisting();

            Analysis analysis = new Analysis();

            PropertyInfo[] properties = typeof(Analysis).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                int startIndex = recievedData.IndexOf(property.Name);
                if (startIndex < 0 || startIndex >= recievedData.Length)
                    continue;

                int endIndex = recievedData.IndexOf("\r\n", startIndex);
                if (endIndex < 0 || endIndex >= recievedData.Length)
                    continue;

                string row = recievedData.Substring(startIndex, endIndex - startIndex);

                List<string> values = new List<string>();

                string[] strings = row.Split(new char[] { ' ', '\t' });
                foreach (string s in strings)
                {
                    if (s.Length > 0)
                        values.Add(s);
                }

                switch (values.Count)
                {
                    case 2:
                        property.SetValue(analysis, values[1]);
                        break;
                    case 3:
                        property.SetValue(analysis, values[1] + " " + values[2]);
                        break;
                }

            }
            Console.WriteLine(recievedData);

            using (var http = new HttpClient())
            {

                try
                {
                    var response = await http.GetAsync("http://localhost:8081/waiting-users");
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<User>>(responseBody);
                    if (users.Count == 0)
                    {
                        Console.WriteLine("Ожидаемых пользователей нет, процесс был приостановлен.");
                    }

                    var selectedId = SelectWaitingUser(users);
                    if (selectedId == -1)
                    {
                        ConsolePrint("Возникла ошибка при выборе пациента");
                        return;
                    }

                    var body = new FulfillUserAnalyse { User = users[selectedId].ID, Analyse = analysis };
                    var content = new StringContent(JsonConvert.SerializeObject(body));
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var postResponse = await http.PostAsync($"{serverURL}/fulfill-waiting-user", content);
                    postResponse.EnsureSuccessStatusCode();
                }
                catch (Exception err)
                {
                    Console.WriteLine($"HTTP error occurred: {err.Message}");
                }
            }

            Thread.Sleep(500);
        }

        private int SelectWaitingUser(List<User> users)
        {
            ConsolePrint("Выберите пользователя, к которому относится текущий анализ:");
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                ConsolePrint($"[{i}] {user.FirstName} {user.LastName} {user.Patronymic} ({user.Snils})");
            }

            var selectedUser = Console.ReadLine();
            if (selectedUser == null)
            {
                return -1;
            }
            var selectedId = int.Parse(selectedUser);
            if (selectedId >= users.Count || selectedId < 0)
            {
                return -1;
            }
            Console.WriteLine("Анализ добален");
            return selectedId;
        }

        private void ConsolePrint(string message, params object[]? args)
        {
            Console.WriteLine(message, args);
        }
    }
}