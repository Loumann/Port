using System;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace COMDataExchanger
{
    class Program
    {
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


        // Доделать еще надо будет регулярные выражения.

        private static void _serialPort_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            var recievedData = _serialPort.ReadExisting();
            Console.WriteLine(recievedData);
            Regex regex = new Regex(@"BLD(\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection match = regex.Matches(recievedData);
            if (match.Count > 0)
            {

            }
        }
    }
}