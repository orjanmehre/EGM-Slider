using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using abb.egm;
using System.Windows.Forms;
using EgmSmallTest;
using System.Diagnostics;


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
//
// Copyright (c) 2014, ABB
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that 
// the following conditions are met:
//
//    * Redistributions of source code must retain the 
//      above copyright notice, this list of conditions 
//      and the following disclaimer.
//    * Redistributions in binary form must reproduce the 
//      above copyright notice, this list of conditions 
//      and the following disclaimer in the documentation 
//      and/or other materials provided with the 
//      distribution.
//    * Neither the name of ABB nor the names of its 
//      contributors may be used to endorse or promote 
//      products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF 
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
namespace egmtest
{
    class Program
    {
        // listen on this port for inbound messages
        public static int _ipPortNumber = 6510;  

        static void Main(string[] args)
        {
            Sensor s = new Sensor();
            //s.Start();
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

        public double Height { get; set; }
        public int DistanceSquare = 200;
        public int xStartPoint = 50;
        public int yStartPoint = -400;
        public float x = 50;
        public float y = -400;
        public int robotX = 0;
        public int robotY = 0;

        
        

        public void SensorThread()
        {
            // create an udp client and listen on any address and the port _ipPortNumber
            _udpServer = new UdpClient(Program._ipPortNumber);
            var remoteEP = new IPEndPoint(IPAddress.Any, Program._ipPortNumber);

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

                    //Debug.WriteLine(robot); //To read message from robot
                    robotX = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.X)); // In relation to WobjBordN
                    robotY = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Y)); // In relation to WobjBordN
                    int robotZ = Convert.ToInt32((robot.FeedBack.Cartesian.Pos.Z));
                    Debug.WriteLine("Robot x-pos = " + robotX + " Robot y-pos = " + robotY + " Robot z-pos " + robotZ);

                
                    // create a new outbound sensor message
                    EgmSensor.Builder sensor = EgmSensor.CreateBuilder();
                    CreateSensorMessage(sensor);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        EgmSensor sensorMessage = sensor.Build();
                        sensorMessage.WriteTo(memoryStream);

                        //Debug.WriteLine(sensorMessage); //To read message from sensor
                        // send the udp message to the robot
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
               .SetTm((uint)DateTime.Now.Ticks) //  Timestamp in milliseconds (can be used for monitoring delays)
               .SetMtype(EgmHeader.Types.MessageType.MSGTYPE_CORRECTION); // Sent by sensor, MSGTYPE_DATA if sent from robot controller

            sensor.SetHeader(hdr);

            // create some sensor data
            EgmPlanned.Builder planned = new EgmPlanned.Builder();
            EgmPose.Builder pos = new EgmPose.Builder();
            EgmQuaternion.Builder pq = new EgmQuaternion.Builder();
            EgmCartesian.Builder pc = new EgmCartesian.Builder();

            x = X_Values_Square();
            Debug.WriteLine("x =" + x.ToString());
            y = Y_Values_Square();
            Debug.WriteLine("y = " + y.ToString());

            Debug.WriteLine("z = " + Height.ToString());

            pc.SetX(0)
              .SetY(0)
              .SetZ(Height);

            pq.SetU0(0.0)
              .SetU1(0.0)
              .SetU2(0.0)
              .SetU3(0.0);

            pos.SetPos(pc)
                .SetOrient(pq);

            planned.SetCartesian(pos);  // bind pos object to planned
            sensor.SetPlanned(planned); // bind planned to sensor object

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

        
        //Set y-values square
        public float Y_Values_Square()
        {
            if (robotX == xStartPoint + DistanceSquare && robotY == yStartPoint)
            {
                y = yStartPoint - DistanceSquare;

            }
            else if (robotX == xStartPoint && robotY == yStartPoint - DistanceSquare && y == yStartPoint - DistanceSquare)
            {
                y = y + DistanceSquare;
            
            }
             return y;
        }


        // Set x-values square
        public float X_Values_Square()
        {
            if (robotX == xStartPoint && robotY == yStartPoint && x == xStartPoint)
            {
                x = x + DistanceSquare;
            }

            else if(robotX == xStartPoint + DistanceSquare && robotY == yStartPoint - DistanceSquare && x == xStartPoint + DistanceSquare)
            {
                x = x - DistanceSquare;
            }
            return x; 
        }


    }
    
}


