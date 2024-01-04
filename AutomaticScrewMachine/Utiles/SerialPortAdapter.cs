using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticScrewMachine.Utiles {
    public static class SerialPortAdapter {
        public static SerialPort _serialPort;
        static SerialPortAdapter () {
            //ConnectedSerial();
        }

        public static void ConnectedSerial () {

            
            // 기본 시리얼 포트 설정
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
        private static void Torq_SerialPort_DataReceived (object sender, SerialDataReceivedEventArgs e) {
            int readCount = 0;
            byte[] readData;

            readComplete = false;

            readCount = _serialPort.BytesToRead;
            readData = new byte[readCount];

            _serialPort.Read(readData, 0, readCount);
            Console.WriteLine("_serialPort.Read(readData, 0, readCount)  ::: " + _serialPort.Read(readData, 0, readCount));
        }

        public static void WriteTorqSerial () {
            //SendPacket(_serialPort,1,'#',"STX",);
            string asc = "STX";
            byte ASCII_STX = 0x02;

            List<byte> listValue = new List<byte>();

            listValue.Add(ASCII_STX);

            listValue.Add(0x01);
            //listValue.Add(0x01);

            listValue.Add(0x23);

            listValue.AddRange(Encoding.UTF8.GetBytes("SET"));

            listValue.Add(0x00);
            //listValue.Add(0x00);
            //listValue.Add(0x00);

            listValue.Add(0x3A);

            listValue.Add(0x42);

            listValue.Add(0x03);


            _serialPort.Write(listValue.ToArray(), 0, listValue.Count);
            
        }
        static void SendPacket (SerialPort serialPort, ushort deviceId, char direction, string command, byte[] data) {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            // 패킷 생성
            writer.Write((byte)':');
            writer.Write(deviceId);
            writer.Write(direction);
            writer.Write(Encoding.ASCII.GetBytes(command.PadRight(3, ' '))); // Command은 3바이트로 맞추고 공백으로 채움
            if (data != null && data.Length > 0)
                writer.Write(data);
            writer.Write((byte)':');
            writer.Write((byte)0xB5);

            // 시리얼 포트를 통해 데이터 전송
            serialPort.Write(stream.ToArray(), 0, (int)stream.Length);
        }


        private static bool make_CheckSum (List<byte> writeData, ref byte ckH, ref byte ckL) {

            byte temp_byte = 0;
            string checkSum = "";
            for (int i = 1; i < writeData.Count; i++) {
                temp_byte = (byte)(temp_byte + writeData[i]);
            }

            checkSum = temp_byte.ToString("X2");

            ckH = ((byte)checkSum[0]);
            ckL = ((byte)checkSum[1]);

            return true;

        }

        private static bool get_CheckSum (List<byte> readData, ref byte ckH, ref byte ckL) {
           
                byte temp_byte = 0;
                string checkSum = "";
                for (int i = 0; i < readData.Count - 2; i++) {
                    temp_byte = (byte)(temp_byte + readData[i]);
                }

                checkSum = temp_byte.ToString("X2");

                ckH = ((byte)checkSum[0]);
                ckL = ((byte)checkSum[1]);

                return true;
        }


    }
}
