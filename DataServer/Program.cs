using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;

using PADI_FS_Library;

namespace DataServer
{
    public class DataServer
    {
        static void Main(string[] args)
        {

            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Clear();

            string dataServerName = args[0];
            int dataServerPort = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(dataServerPort);
            ChannelServices.RegisterChannel(channel, true);

            //Registering MetadataServer service
            Console.WriteLine("Registering DataServer as " + dataServerName + " with port " + dataServerPort);
            DataServerRemoting dataServerRemotingObject = new DataServerRemoting(dataServerName, dataServerPort);
            RemotingServices.Marshal((DataServerRemoting)dataServerRemotingObject, dataServerName, typeof(DataServerRemoting));
            /*RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MetadataServerRemoting),
                metadataServerName,
                WellKnownObjectMode.Singleton); --> APAGAR EM PRINCIPIO*/
            Console.WriteLine("Registered!");
            
            //Registo do DataServer nos MetadataServers (basta enviar a um, ele expande o registo)
            Dictionary<string, MetadataServerInterface> metadataServersProxys = new Dictionary<string,MetadataServerInterface>();
            TextReader metadataServersPorts = new StreamReader(@"..\..\..\DataServer\bin\Debug\MetadataServersPorts.txt");
            string metadataServersPortsLine;
            string[] metadataServersPortsLineWords;
            while ((metadataServersPortsLine = metadataServersPorts.ReadLine()) != null)
            {
                metadataServersPortsLineWords = metadataServersPortsLine.Split(' ');
                string metadataServerName = metadataServersPortsLineWords[0];
                string metadataServerPort = metadataServersPortsLineWords[1];
                string metadataServerURL = "tcp://localhost:" + metadataServerPort + "/" + metadataServerName;

                MetadataServerInterface metadataServerToAdd = (MetadataServerInterface)Activator.GetObject(
                                                              typeof(MetadataServerInterface),
                                                              metadataServerURL);

                metadataServersProxys.Add(metadataServerName, metadataServerToAdd);
            }

            
            foreach (string metadataServerName in metadataServersProxys.Keys)
            {
                metadataServersProxys[metadataServerName].registerDataServer(dataServerName, "tcp://localhost:" + dataServerPort + "/" + dataServerName);
            }
            


            
            System.Console.WriteLine("DataServer - " + dataServerName + " -<enter> to leave...");
            System.Console.ReadLine();
        }
    }
}
