using System;
using PortScanner;

namespace COMDataExchanger
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                var urs = new URS();
                urs.ConnectToDevice();

                Console.WriteLine("Драйвер успешно запущен! \nОжидание данных...");

            }
            catch (Exception e)
            {
                Console.WriteLine("Во время работы драйвера произошла ошибка: {0}", e.Message);
            }
        }
    }
}