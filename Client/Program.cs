﻿using System;
using System.IO;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

public class Client
{
    const int dataArraySize = 200;
    const int streamBufferSize = 1000;
    const int numberOfLoops = 10000;

    static void Main(string[] args)
    {
        // Check that an argument was specified when the 
        // program was invoked. 
        //if (args.Length == 0)
        //{
        //    Console.WriteLine("Error: The name of the host computer" +
        //        " must be specified when the program is invoked.");
        //    return;
        //}

      //  string remoteName = args[0];

        // Create the underlying socket and connect to the server.
        Console.WriteLine("OS {0} {1} processors count {2}\n", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "x64" : "x32", Environment.ProcessorCount);
        Socket clientSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        clientSocket.Connect(new IPEndPoint(
            IPAddress.Parse("127.0.0.1"), 1800));

        Console.WriteLine("Client is connected.\n");

        // Create a NetworkStream that owns clientSocket and 
        // then create a BufferedStream on top of the NetworkStream. 
        // Both streams are disposed when execution exits the 
        // using statement. 
        using (Stream
            netStream = new NetworkStream(clientSocket, true),
            bufStream =
                  new BufferedStream(netStream, streamBufferSize))
        {
            // Check whether the underlying stream supports seeking.
            Console.WriteLine("NetworkStream {0} seeking.\n",
                bufStream.CanSeek ? "supports" : "does not support");

            // Send and receive data. 
            if (bufStream.CanWrite)
            {
                SendData(netStream, bufStream);
            }
            if (bufStream.CanRead)
            {
                ReceiveData(netStream, bufStream);
            }

            // When bufStream is closed, netStream is in turn 
            // closed, which in turn shuts down the connection 
            // and closes clientSocket.
            Console.WriteLine("\nShutting down the connection.");
            bufStream.Close();
        }
        Console.ReadKey();
    }

    static void 
        SendData(Stream netStream, Stream bufStream)
    {
        DateTime startTime;
        double networkTime, bufferedTime;

        // Create random data to send to the server. 
        byte[] dataToSend = new byte[dataArraySize];
        new Random().NextBytes(dataToSend);

        // Send the data using the NetworkStream.
        Console.WriteLine("Sending data using NetworkStream.");
        startTime = DateTime.Now;
        for (int i = 0; i < numberOfLoops; i++)
        {
            netStream.Write(dataToSend, 0, dataToSend.Length);
        }
        networkTime = (DateTime.Now - startTime).TotalMilliseconds;
        Console.WriteLine("{0} bytes sent in {1} milliseconds.\n",
            numberOfLoops * dataToSend.Length,
            networkTime.ToString("F1"));

        // Send the data using the BufferedStream.
        Console.WriteLine("Sending data using BufferedStream.");
        startTime = DateTime.Now;
        for (int i = 0; i < numberOfLoops; i++)
        {
            bufStream.Write(dataToSend, 0, dataToSend.Length);
        }
        bufStream.Flush();
        bufferedTime = (DateTime.Now - startTime).TotalMilliseconds;
        Console.WriteLine("{0} bytes sent in {1} milliseconds.\n",
            numberOfLoops * dataToSend.Length,
            bufferedTime.ToString("F1"));

        // Print the ratio of write times.
        Console.WriteLine("Sending data using the buffered " +
            "network stream was {0} {1} than using the network " +
            "stream alone.\n",
            (networkTime / bufferedTime).ToString("P0"),
            bufferedTime < networkTime ? "faster" : "slower");
    }

    static void ReceiveData(Stream netStream, Stream bufStream)
    {
        DateTime startTime;
        double networkTime, bufferedTime = 0;
        int bytesReceived = 0;
        byte[] receivedData = new byte[dataArraySize];

        // Receive data using the NetworkStream.
        Console.WriteLine("Receiving data using NetworkStream.");
        startTime = DateTime.Now;
        while (bytesReceived < numberOfLoops * receivedData.Length)
        {
            bytesReceived += netStream.Read(
                receivedData, 0, receivedData.Length);
        }
        networkTime = (DateTime.Now - startTime).TotalMilliseconds;
        Console.WriteLine("{0} bytes received in {1} milliseconds.\n",
            bytesReceived.ToString(),
            networkTime.ToString("F1"));

        // Receive data using the BufferedStream.
        Console.WriteLine("Receiving data using BufferedStream.");
        bytesReceived = 0;
        startTime = DateTime.Now;

        int numBytesToRead = numberOfLoops * receivedData.Length;

        while (numBytesToRead > 0)
        {
            // Read may return anything from 0 to numBytesToRead. 
            int n = bufStream.Read(receivedData, 0, receivedData.Length);
            // The end of the file is reached. 
            if (n == 0)
                break;
            bytesReceived += n;
            numBytesToRead -= n;
        }

        bufferedTime = (DateTime.Now - startTime).TotalMilliseconds;
        Console.WriteLine("{0} bytes received in {1} milliseconds.\n",
            bytesReceived.ToString(),
            bufferedTime.ToString("F1"));

        // Print the ratio of read times.
        Console.WriteLine("Receiving data using the buffered network" +
            " stream was {0} {1} than using the network stream alone.",
            (networkTime / bufferedTime).ToString("P0"),
            bufferedTime < networkTime ? "faster" : "slower");
    }
}
