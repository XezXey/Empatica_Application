using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using CsvHelper;
using System.Collections.Generic;
using System.Timers;
using System.Globalization;

namespace EmpaticaBLEClient
{
    public static class AsynchronousClient
    {
        // Global variable of filename.
        private static string filename = @"tmp.csv";    // Just initial filename because it need to be declare. But it will change later anyway.

        private static TextWriter textWriter = File.CreateText(filename);

        // The port number for the remote device.
        private const string ServerAddress = "127.0.0.1";
        private const int ServerPort = 27555;

        // ManualResetEvent instances signal completion.
        private static readonly ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone = new ManualResetEvent(false);

        // The response from the remote device.
        private static String _response = String.Empty;
        
        internal class Acc
        {
            public string time { get; set; }
            public int ax { get; set; }
            public int ay { get; set; }
            public int az { get; set; }
        }
        internal class Bvp
        {
            public string time { get; set; }
            public float bvp { get; set; }
        }
        internal class Gsr
        {
            public string time { get; set; }
            public float gsr { get; set; }
        }
        internal class Temp
        {
            public string time { get; set; }
            public float tmp { get; set; }
        }
        internal class Ibi
        {
            public string time { get; set; }
            public float ibi { get; set; }
        }
        internal class Heartbeat
        {
            public string time { get; set; }
            public float hr { get; set; }
        }
        internal class BatteryLevel
        {
            public string time { get; set; }
            public float battery { get; set; }
        }
        internal class Tag
        {
            public string time { get; set; }    //Time that take tag signal on device.
        }

        static void WriteCsvFile_ACC(IEnumerable<Acc> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Acc" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
        static void WriteCsvFile_BVP(IEnumerable<Bvp> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Bvp" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
   
        static void WriteCsvFile_GSR(IEnumerable<Gsr> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Gsr" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
        static void WriteCsvFile_IBI(IEnumerable<Ibi> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Ibi" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
        static void WriteCsvFile_TAG(IEnumerable<Tag> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Tag" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
        static void WriteCsvFile_TEMP(IEnumerable<Temp> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Temp" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
        static void WriteCsvFile_BATTERY(IEnumerable<BatteryLevel> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("BatteryLevel" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }
        static void WriteCsvFile_HR(IEnumerable<Heartbeat> each_value)
        {
            var csvWriter = new CsvWriter(textWriter);
            textWriter = File.CreateText("Heartbeat" + filename);
            csvWriter.WriteRecords(each_value);
            csvWriter.NextRecord();
        }


        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            filename = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssTZD");
            textWriter.Close();
        }

        public static void StartClient()
        {
            // Create the command string for taking sensor input.
            string[] subscribe_str = { "acc", "bvp", "gsr", "ibi", "tmp", "bat", "tag" };
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                var ipHostInfo = new IPHostEntry { AddressList = new[] { IPAddress.Parse(ServerAddress) } };
                var ipAddress = ipHostInfo.AddressList[0];
                var remoteEp = new IPEndPoint(ipAddress, ServerPort);

                // Create a TSCP/IP socket.
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEp, (ConnectCallback), client);
                ConnectDone.WaitOne();

                //Auto Connect to the E4 device
                Console.Write("Auto_Connection...");
                var auto_connect_msg = "device_connect 0C2C64";
                Send(client, auto_connect_msg + Environment.NewLine);
                SendDone.WaitOne();
                Receive(client);
                ReceiveDone.WaitOne();

                // Infinite loop to take user's commands.
                while (true)
                {
                    // Set timer every 30 sec to stop writing an old file and start a new one.
                    System.Timers.Timer aTimer = new System.Timers.Timer();
                    aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                    aTimer.Interval = 31000;
                    aTimer.Enabled = true;

                    Console.Write("Enter your command : ");
                    var msg = Console.ReadLine();   // Store a command from a user.
                    if (msg == "subscribe_all")
                    {
                        for(int i=0; i<subscribe_str.Length; i++)
                        {
                            var subscribe_all_msg = "device_subscribe " + subscribe_str[i] + " ON";
                            Console.WriteLine(subscribe_all_msg);
                            Send(client, subscribe_all_msg + Environment.NewLine);  //Environment.NewLine ==> " /r/n" is newline and move cursor to the beginning of the line.
                            SendDone.WaitOne();
                            Receive(client);
                            ReceiveDone.WaitOne();
                            Thread.Sleep(100);  //Delay 100ms to protect the response message use as command message.
                        }
                    }
                    else if (msg == "unsubscribe_all")
                    {
                        for (int i = 0; i < subscribe_str.Length; i++)
                        {
                            var unsubscribe_all_msg = "device_subscribe " + subscribe_str[i] + " OFF";
                            Console.WriteLine(unsubscribe_all_msg);
                            Send(client, unsubscribe_all_msg + Environment.NewLine);  //Environment.NewLine ==> " /r/n" is newline and move cursor to the beginning of the line.
                            SendDone.WaitOne();
                            Receive(client);
                            ReceiveDone.WaitOne();
                            Thread.Sleep(100);  //Delay 100ms to protect the response message use as command message.
                        }
                    }
                    else
                    {
                        Send(client, msg + Environment.NewLine);  //Environment.NewLine ==> "/r/n" is newline and move cursor to the beginning of the line.
                        SendDone.WaitOne();
                        Receive(client);
                        ReceiveDone.WaitOne();
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

  

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint);

                // Signal that the connection has been made.
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                var state = new StateObject { WorkSocket = client };

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;

                // Read data from the remote device.
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                    _response = state.Sb.ToString();

                    HandleResponseFromEmpaticaBLEServer(_response);

                    state.Sb.Clear();

                    ReceiveDone.Set();

                    // Get the rest of the data.
                    client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.Sb.Length > 1)
                    {
                        _response = state.Sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                client.EndSend(ar);
                // Signal that all bytes have been sent.
                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Called everytime that have response from EmpaticaBLEServer   
        private static void HandleResponseFromEmpaticaBLEServer(string response)
        {
            Console.Write(response);
            string[] sensor_type = response.Split(' ');

            if (sensor_type[0] == "E4_Acc")
            {
                //Write Acc to csv
                Console.WriteLine("This is ACC.");
                List<Acc> acc_list = new List<Acc>() {
                    new  Acc() {
                        time = sensor_type[1],
                        ax = Convert.ToInt32(sensor_type[2]),
                        ay = Convert.ToInt32(sensor_type[2]),
                        az = Convert.ToInt32(sensor_type[2])
                    }
                };
                WriteCsvFile_ACC(acc_list);   // Write each record to .csv file stream.
            }

            else if(sensor_type[0] == "E4_Bvp")
            {
                //Write Bvp to csv
                Console.WriteLine("This is BVP.");
                List<Bvp> bvp_list = new List<Bvp>() {
                    new  Bvp() {
                        time = sensor_type[1],
                        bvp = Convert.ToSingle(sensor_type[2])
                    }
                };
                WriteCsvFile_BVP(bvp_list);   // Write each record to .csv file stream.
            }

            else if (sensor_type[0] == "E4_Gsr")
            {
                //Write Gsr to csv
                Console.WriteLine("This is GSR.");
                List<Gsr> gsr_list = new List<Gsr>() {
                    new  Gsr() {
                        time = sensor_type[1],
                        gsr = Convert.ToSingle(sensor_type[2]),
                    }
                };
                WriteCsvFile_GSR(gsr_list);   // Write each record to .csv file stream.
            }

            else if (sensor_type[0] == "E4_Temp")
            {
                //Write Temp to csv
                Console.WriteLine("This is TEMP.");
                List<Temp> tmp_list = new List<Temp>() {
                    new  Temp() {
                        time = sensor_type[1],
                        tmp = Convert.ToSingle(sensor_type[2])
                    }
                };
                WriteCsvFile_TEMP(tmp_list);   // Write each record to .csv file stream.
            }

            else if (sensor_type[0] == "E4_Ibi")
            {                
                //Write Ibi to csv
                Console.WriteLine("This is IBI.");
                List<Ibi> ibi_list = new List<Ibi>() {
                    new  Ibi() {
                        time = sensor_type[1],
                        ibi = Convert.ToSingle(sensor_type[2])
                    }
                };
                WriteCsvFile_IBI(ibi_list);   // Write each record to .csv file stream.
            }

            else if (sensor_type[0] == "E4_Hr")
            {
                //Write Hr to csv
                Console.WriteLine("This is HR.");
                List<Heartbeat> hr_list = new List<Heartbeat>() {
                    new  Heartbeat() {
                        time = sensor_type[1],
                        hr = Convert.ToSingle(sensor_type[2])
                    }
                };
                WriteCsvFile_HR(hr_list);   // Write each record to .csv file stream.
            }

            else if (sensor_type[0] == "E4_Battery")
            {                
                //Write Batter to csv
                Console.WriteLine("This is BATTERYLEVEL.");

                List<BatteryLevel> battery_list = new List<BatteryLevel>() {
                    new  BatteryLevel() {
                        time = sensor_type[1],
                        battery = Convert.ToSingle(sensor_type[2])
                    }
                };
                WriteCsvFile_BATTERY(battery_list);   // Write each record to .csv file stream.
            }

            else if (sensor_type[0] == "E4_Tag")
            {
                //Write Tag to csv
                Console.WriteLine("This is TAG.");
                List<Tag> tag_list = new List<Tag>() {
                    new  Tag() {
                        time = sensor_type[1],
                    }
                };
                WriteCsvFile_TAG(tag_list);   // Write each record to .csv file stream.
            }
        }
    }
}

