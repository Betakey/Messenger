﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NetDLL;

namespace ChatClient
{
    public class Client
    {
        public Guid ID { get; set; }

        public string Name { get; private set; }

        public Thread Thread { get; private set; }

        public StreamWriter Out { get; private set; }

        public StreamReader In { get; private set; }

        public TcpClient TClient { get; private set; }

        private Packet lastSentPacket;

        public Client(TcpClient client, ChatClientForm chatClientForm, string name)
        {
            Name = name;
            TClient = client;
            Out = new StreamWriter(TClient.GetStream());
            In = new StreamReader(TClient.GetStream());
            Thread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        int bytesRead = 0;
                        int bufferSize = 0;
                        byte[] datalength = new byte[4];
                        TClient.GetStream().Read(datalength, 0, datalength.Length);
                        bufferSize = BitConverter.ToInt32(datalength, 0);

                        if (bufferSize != 0)
                        {
                            byte[] bytes = new byte[bufferSize];
                            bytesRead = TClient.GetStream().Read(bytes, 0, bufferSize);
                            if (bytesRead == 0)
                            {
                                continue;
                            }
                            try // This Try Catch is just for the Deserialization because we want to handle this special
                            {
                                chatClientForm.ClearStatus();
                                Packet packet = Packet.ToPacket(bytes);
                                if (packet != null)
                                    chatClientForm.PacketHandler(packet);
                            }
                            catch
                            {
                                chatClientForm.ShowStatusError("Informationen gingen verloren!", "Ups! Dann sind wohl ein paar Informationen verloren gegangen! Wir bitten um entschuldigung und werden versuchen diese Informationen neu anzufordern!");
                                if (lastSentPacket != null)
                                {
                                    if (lastSentPacket.GetType().ToString().Contains("Request"))
                                    {
                                        Write(lastSentPacket);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Verbindung zum Server verloren!", " Verbindung verloren!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MethodInvoker invoker = delegate { chatClientForm.Close(); };
                    chatClientForm.Invoke(invoker);
                }
            });
            Thread.IsBackground = true;
            Thread.Start();
        }

        /// <summary>
        /// Methode closes every connection and thread.
        /// </summary>
        public void Close()
        {
            TClient.GetStream().Close();
            TClient.Close();
            Thread.Abort();
            In.Close();
            Out.Close();
        }

        /// <summary>
        /// Converts packet to String and sends it.
        /// </summary>
        /// <param name="packet"></param>
        public void Write(Packet packet)
        {
            lastSentPacket = packet;
            byte[] bytes = packet.ToBytes();
            TClient.GetStream().Write(BitConverter.GetBytes(bytes.Length), 0,
                BitConverter.GetBytes(bytes.Length).Length);
            TClient.GetStream().Write(bytes, 0, bytes.Length);
        }
    }
}