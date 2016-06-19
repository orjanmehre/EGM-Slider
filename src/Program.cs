using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using abb.egm;

//////////////////////////////////////////////////////////////////////////
// Sample program using protobuf-csharp-port 
// (http://code.google.com/p/protobuf-csharp-port/wiki/GettingStarted)
//
// 1) Download protobuf-csharp binaries from https://code.google.com/p/protobuf-csharp-port/
// 2) Unpack the zip file
// 3) Copy the egm.proto file to a sub catalogue where protobuf-csharp was un-zipped, e.g. ~\protobuf-csharp\tools\egm
// 4) Generate an egm C# file from the egm.proto file by typing in a windows console: protogen .\egm\egm.proto --proto_path=.\egm
// 5) Create a C# console application in Visual Studio
// 6) Install Nuget, in Visual Studio, click Tools and then Extension Manager. Goto to Online, find the NuGet Package Manager extension and click Download.
// 7) Install protobuf-csharp via NuGet, select in Visual Studio, Tools Nuget Package Manager and then Package Manager Console and type PM>Install-Package Google.ProtocolBuffers
// 8) Add the generated file egm.cs to the Visual Studio project (add existing item)
// 9) Copy the code below and then compile, link and run.
//////////////////////////////////////////////////////////////////////////

namespace EgmSmallTest
{
    class Program
    {
        // listen on this port for inbound messages
        public static int IpPortNumber = 6510;  

        static void Main(string[] args)
        {
            Sensor s = new Sensor();
            Application.Run(new ControlPanel(s));

            Console.WriteLine("Press any key to Exit");
            Console.ReadLine();  
        }
    }

    public class Sensor
    {
        private Thread _sensorThread = null;
        private UdpClient _udpServer = null;
        private bool _exitThread = false;
        private uint _seqNumber = 0;
        private int _distanceSquare;
        private int _xStartPoint;
        private int _yStartPoint;
        private float _x;
        private float _y;
        private int _robotX;
        private int _robotY;
        private int _robotZ; 

        public double Height { get; set; }

        // Set the measurements for the square and initialize the start points
        public Sensor()
        {
            _distanceSquare = 200;
            _xStartPoint = 50;
            _yStartPoint = -400;
            _x = 50;
            _y = -400;
            _robotX = 0;
            _robotY = 0;
            _robotZ = 0; 
        }
        

        public void SensorThread()
        {
            // create an udp client and listen on any address and the port _ipPortNumber
            _udpServer = new UdpClient(Program.IpPortNumber);
            var remoteEP = new IPEndPoint(IPAddress.Any, Program.IpPortNumber);

            while (_exitThread == false)
            {
                // get the message from robot
                var data = _udpServer.Receive(ref remoteEP);

                if (data != null)
                {
                    // de-serialize inbound message from robot using Google Protocol Buffer
                    EgmRobot robot = EgmRobot.CreateBuilder().MergeFrom(data).Build();

                    // display inbound message
                    DisplayInboundMessage(robot);

                    // Get the robots X-position
                    _robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X)); 
                    // Get the robots Y-position
                    _robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y)); 
                    // Get the robots Z-position
                    _robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));


                
                    // create a new outbound sensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    CreateSensorMessage(sensor);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        EgmSensor sensorMessage = sensor.Build();
                        sensorMessage.WriteTo(memoryStream);

                        // send the UDP message to the robot
                        int bytesSent = _udpServer.Send(memoryStream.ToArray(),
                                                       (int)memoryStream.Length, remoteEP);
                        if (bytesSent < 0)
                        {
                            Console.WriteLine("Error send to robot");
                        }
                    }
                }
            }
        }

        // Display message from robot
        void DisplayInboundMessage(EgmRobot robot)
        {
            if (robot.HasHeader && robot.Header.HasSeqno && robot.Header.HasTm)
            {
                Console.WriteLine("Seq={0} tm={1}",
                    robot.Header.Seqno.ToString(), robot.Header.Tm.ToString());
            }
            else
            {
                Console.WriteLine("No header in robot message");
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Create a sensor message to send to the robot
        void CreateSensorMessage(EgmSensor.Builder sensor)
        {
            // create a header
            EgmHeader.Builder hdr = new EgmHeader.Builder();
            hdr.SetSeqno(_seqNumber++)
               //Timestamp in milliseconds (can be used for monitoring delays)
               .SetTm((uint)DateTime.Now.Ticks)
               //Sent by sensor, MSGTYPE_DATA if sent from robot controller
               .SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION); 

            sensor.SetHeader(hdr);

            // create some sensor data
            EgmPlanned.Builder planned = new EgmPlanned.Builder();
            EgmPose.Builder pos = new EgmPose.Builder();
            EgmQuaternion.Builder pq = new EgmQuaternion.Builder();
            EgmCartesian.Builder pc = new EgmCartesian.Builder();

            _x = X_Values_Square();
            _y = Y_Values_Square();

            pc.SetX(_x)
              .SetY(_y)
              .SetZ(Height);

            pq.SetU0(0.0)
              .SetU1(0.0)
              .SetU2(0.0)
              .SetU3(0.0);

            pos.SetPos(pc)
                .SetOrient(pq);

            // bind pos object to planned
            planned.SetCartesian(pos);
            // bind planned to sensor object 
            sensor.SetPlanned(planned); 

            return;
        }

        // Start a thread to listen on inbound messages
        public void Start()
        {
            _sensorThread = new Thread(new ThreadStart(SensorThread));
            _sensorThread.Start();
        }

        // Stop and exit thread
        public void Stop()
        {
            _exitThread = true;
            _sensorThread.Abort();
        }

        
        //Set the Y-positions for the square
        public float Y_Values_Square()
        {
            if (_robotX == _xStartPoint + _distanceSquare && _robotY == _yStartPoint)
            {
                _y = _yStartPoint - _distanceSquare;

            }
            else if (_robotX == _xStartPoint && _robotY == _yStartPoint - _distanceSquare && _y == _yStartPoint - _distanceSquare)
            {
                _y = _y + _distanceSquare;
            
            }
             return _y;
        }


        // Set the X-positions for the square
        public float X_Values_Square()
        {
            if (_robotX == _xStartPoint && _robotY == _yStartPoint && _x == _xStartPoint)
            {
                _x = _x + _distanceSquare;
            }

            else if(_robotX == _xStartPoint + _distanceSquare && _robotY == _yStartPoint - _distanceSquare && _x == _xStartPoint + _distanceSquare)
            {
                _x = _x - _distanceSquare;
            }
            return _x; 
        }
    }  
}


