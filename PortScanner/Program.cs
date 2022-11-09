using PortScanner;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;



namespace COMDataExchanger
{
    class Program
    {

        public static StringBuilder sb = new StringBuilder();

        static SerialPort _serialPort = null;
        static bool session;




        static void Main(string[] args)
        {

            AllPorts();

            try
            {
                _serialPort = new SerialPort
                {
                    PortName = Console.ReadLine() ?? throw new InvalidOperationException()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            if (_serialPort == null)
            {
                Console.WriteLine("The text format is not correct.");
                return;
            }
            _serialPort.ReadTimeout = -1;
            _serialPort.WriteTimeout = 2000;
            _serialPort.BaudRate = 19200;

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataRecieved);

            try
            {
                _serialPort.Open();
                session = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Write();

            //try
            //{
            //    _serialPort.Close();
            //    session = false;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}


            Console.ReadKey();
        }

        private static void AllPorts()
        {
            var ports = SerialPort.GetPortNames();

            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine("[" + i.ToString() + "] " + ports[i].ToString());
            }
        }

        private static void Write()
        {
            while (session == true)
            {
                var message = Console.ReadLine();

                _serialPort.WriteLine($"<{_serialPort.PortName}>: {message}");
            }
        }









        //регулярное выражение и вывод с записью в json объект
        private static void _serialPort_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            var recievedData = _serialPort.ReadExisting();
            Console.WriteLine(recievedData);
            string json = JsonSerializer.Serialize(recievedData);
            Console.WriteLine(json.Length);

            Regex regex = new Regex(@"^\* \w\w\w", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(recievedData);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                    Console.WriteLine(match.Value);
                Console.WriteLine("Найдено");

            }
            else
            {
                Console.WriteLine("Нету");
            }
        }
    }
}