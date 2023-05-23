using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using WebSocketSharp;

namespace PortScanner
{
    public class URS
    {
        private readonly int _ursReadTimeout = 2000;
        private readonly int _ursWriteTimeout = 2000;
        private readonly int _ursBaudRate = 19200;
        
        private readonly string _serverSocketAddress = "ws://localhost:8080/";
        
        private SerialPort? _ursPort;
        private WebSocket? _serverSocket;
        
        public URS()
        {
            _ursPort = null;
            _serverSocket = null;
        }

        private Analysis getАnalysis()
        {
            string data = "Accustrip URS 10\nSeq.No: 0003\nPat.ID:\n2021-12-13 19:56\n* * * * * * * * * * * \n BLD 50 Ery/?l\nUBG NORM\nBIL NEG\nPRO NEG\nNIT NEG\nKET NEG\nGLU NORM\npH 5\nSG 1.025\n* LEU 25 Leu/?l\n";
            var recievedData = data;

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

            return analysis;
        }

        public void ConnectToDevice()
        {
            ConsolePrint("Выберите порт, к которому подключено средство диагностики:");
            
            var ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++) {
                ConsolePrint("[{0}] {1}", i, ports[i]);
            }

            var portName = Console.ReadLine();
            try
            {
                _ursPort = new SerialPort { PortName =  portName };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Неверно был выбран порт для подключения средства диагностики.");
            }

            _ursPort.ReadTimeout = _ursReadTimeout;
            _ursPort.WriteTimeout = _ursWriteTimeout;
            _ursPort.BaudRate = _ursBaudRate;

            _ursPort.DataReceived += ursPortHandler;
            
            ConsolePrint("Средство диагностики успешно подключено!");

            sendTestAnalisys();
        }

        private void ursPortHandler(object sender, SerialDataReceivedEventArgs e) {
            var recievedData = _ursPort.ReadExisting();
	
            Analysis analysis = new Analysis();

            PropertyInfo[] properties = typeof(Analysis).GetProperties();
            foreach (PropertyInfo property in properties) {
                int startIndex = recievedData.IndexOf(property.Name);
                if (startIndex < 0 || startIndex >= recievedData.Length)
                    continue;

                int endIndex = recievedData.IndexOf("\r\n", startIndex);
                if (endIndex < 0 || endIndex >= recievedData.Length)
                    continue;

                string row = recievedData.Substring(startIndex, endIndex - startIndex);

                List<string> values = new List<string>();

                string[] strings = row.Split(new char[] { ' ', '\t' });
                foreach (string s in strings) {
                    if (s.Length > 0)
                        values.Add(s);
                }

                switch (values.Count) {
                    case 2:
                        property.SetValue(analysis, values[1]);
                        break;
                    case 3:
                        property.SetValue(analysis, values[1] + " " + values[2]);
                        break;
                }
				
            }
		
            string jsonString = JsonConvert.SerializeObject(analysis);

            if (!_serverSocket.IsAlive)
            {
                throw new Exception("Невозможно отправить данные на сервер, так как соединение было разорвано.");
            }
            _serverSocket.Send(jsonString);
			
            Thread.Sleep(500);
        }
        
        public void ConnectToServer()
        {
            _serverSocket = new WebSocket(_serverSocketAddress);

            _serverSocket.OnOpen += SocketOnOpen;
            _serverSocket.OnMessage += SocketOnMessage;
            _serverSocket.OnError += SocketOnError;
            _serverSocket.OnClose += SocketOnClose;
            
            _serverSocket.Connect();
        }

        private void SocketOnOpen(object sender, EventArgs e)
        {
            ConsolePrint("Соединение с рабочим сервером успешно установлено!");
        }
        
        private void SocketOnMessage(object sender, EventArgs e)
        {
            ConsolePrint("{0}", (e as MessageEventArgs)?.Data);
        }
        
        private void SocketOnError(object sender, EventArgs e)
        {
            throw new Exception("Произошла ошибка в соединении с сервером: " + (e as ErrorEventArgs).Message);
        }
        
        private void SocketOnClose(object sender, EventArgs e)
        {
            ConsolePrint("Соединение с сервером закрыто!");
        }

        private void ConsolePrint(string message, params object[]? args)
        {
            Console.WriteLine(message, args);
        }
        
        private void sendTestAnalisys()
        {
            var analysis = getАnalysis();
            _serverSocket.Send(JsonConvert.SerializeObject(analysis));
        }
    }
}