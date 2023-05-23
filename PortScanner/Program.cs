using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using WebSocketSharp;
using PortScanner;

namespace COMDataExchanger
{
    class Program
    {
        static SerialPort _serialPort = null;
        static WebSocket ws = null;
        static bool session;

        static void Main(string[] args)
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
                    default:
                        break;
                }

            }

            string jsonString = JsonConvert.SerializeObject(analysis);
            Console.WriteLine(jsonString);

            Thread.Sleep(500);
            ws = new WebSocket("ws://localhost:8080/");
            ws.OnOpen += (sender, e) => {
                Console.WriteLine("Сокет открыт");
            };
            ws.OnMessage += (sender, e) => {
                Console.WriteLine("WebSocket получил сообщение: " + e.Data);
            };
            ws.OnError += (sender, e) => {
                Console.WriteLine("Ошибка WebSocket: " + e.Message);
            };
            ws.OnClose += (sender, e) => {
                Console.WriteLine("WebSocket закрыт: " + e.Reason);
            };
            ws.Connect();

           
            if (ws.IsAlive)
            {
                ws.Send(jsonString);
                Console.WriteLine("Отправленное сообщение от сервера-получаетелся: " + jsonString);
            }
        }
    }
}