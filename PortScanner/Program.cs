using System;
using System.IO.Ports;

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
            _serialPort.ReadTimeout = 300;
            _serialPort.WriteTimeout = 300;

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

        private static void _serialPort_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            var recievedData = _serialPort.ReadExisting();

            Console.WriteLine(recievedData);
        }
    }
}
