﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CrallyClient.Packets.Client;
using CrallyClient.Packets.Server;

namespace CrallyClient
{
    class Connection
    {
        TcpClient client;
        NetworkStream stream;
        byte[] buffer;

        public Connection(string ip, int port)
        {
            client = new TcpClient();
            buffer = new byte[16384];

            Log.Info($"Connecting to {ip}:{port}");
            client.Connect(ip, port);

            stream = client.GetStream();
        }

        public void Connect(string ip, int port)
        {
            if (client.Connected)
                client.Close();

            client = new TcpClient();

            client.Connect(ip, port);
            stream = client.GetStream();
        }

        public void Send(string msg)
        {
            stream.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
        }

        public void Send(ClientPacket packet)
        {
            byte[] data = packet.build();
            stream.Write(data, 0, data.Length);
        }

        public string ReadString()
        {
            stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer).TrimEnd((char)0);
        }

        public byte[] ReadBytes()
        {
            stream.Read(buffer, 0, 5);
            int size = ((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | (buffer[3]) - 5);

            if (size == -5)
                size = 0;

            Log.Info($"Read packet [ID:{(ServerPacket.ID)buffer[4]}]: size={size}");

            int read = 0;
            while (read != size)
            {
                read += stream.Read(buffer, 5 + read, size - read);
            }

            return buffer;
        }

        public void Dispose()
        {
            client.Close();
            stream.Close();
        }
    }
}
