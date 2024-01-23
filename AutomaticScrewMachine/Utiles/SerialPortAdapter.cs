using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AutomaticScrewMachine.Utiles {
    public static class SerialPortAdapter {
        public static SerialPort _serialPort;
        public static string ReadData;
        static SerialPortAdapter () {
            //ConnectedSerial();
        }

        public static void ConnectedSerial () {
            _serialPort = new SerialPort {
                PortName = "COM1",
                BaudRate = 57600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8
            };

            if (_serialPort.IsOpen) {
                _serialPort.DataReceived -= Torq_SerialPort_DataReceived;
                _serialPort.Close();
            }

            _serialPort.Open();

            if (_serialPort.IsOpen) {
                _serialPort.DataReceived += Torq_SerialPort_DataReceived;
            }

        }

        public static int receiveStep = 0;
        public static bool readComplete = false;
        public static Dispatcher dispatcher;
        private static void Torq_SerialPort_DataReceived (object sender, SerialDataReceivedEventArgs e) {
            // DataReceived 이벤트 핸들러에서 수신된 데이터를 처리하는 코드를 추가
            SerialPort serialPort = (SerialPort)sender;
            string receivedData = serialPort.ReadExisting();

            ReadData = receivedData;
            //Console.WriteLine($"수신된 메시지: {receivedData}");

            // UI 업데이트를 Dispatcher를 통해 수행 *Dispatcher ISSUE
            dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() => {
                ReadData = receivedData;
                Console.WriteLine($"수신된 메시지: {receivedData}");
            });
        }
        public static void SendData (string inputData) {
            string inputString = inputData;

            // 입력된 문자열을 정수로 변환
            if (int.TryParse(inputString, out int inputValue)) {
                // 3자리로 표현하고 앞을 0으로 채움
                string formattedInput = "P" + inputValue.ToString("D3");

                int sum = 0;

                foreach (char c in formattedInput) {
                    int asciiValue = (int)c;
                    sum += asciiValue;
                }

                char stxChar = (char)2; // STX를 나타내는 ASCII 코드 2
                char etxChar = (char)3; // ETX를 나타내는 ASCII 코드 3

                // 16진수로 변환된 마지막 자리 계산
                string hexSum = sum.ToString("X");
                char lastDigit = hexSum[hexSum.Length - 1];

                string messageToSend = $"{stxChar}{formattedInput}{lastDigit}{etxChar}";

                // 실제로 SerialPort를 통해 데이터를 전송합니다.
                _serialPort.Write(messageToSend);

                Console.WriteLine($"전송한 메시지: {messageToSend}");
            } else {
                Console.WriteLine("유효한 숫자가 아닙니다.");
            }
        }
    }
}
