using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;

namespace MetadataServer
{
    public class MetadataServer
    {
        static void Main(string[] args)
        {
            //Console.BackgroundColor = ConsoleColor.Red;
            //Console.Clear();

            string metadataServerName = args[0];
            int metadataServerPort = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(metadataServerPort);
            ChannelServices.RegisterChannel(channel, true);

            //Registering MetadataServer service
            Console.WriteLine("Registering MetadataServer as " + metadataServerName + " with port " + metadataServerPort);
            MetadataServerRemoting metadataServerRemotingObject = new MetadataServerRemoting(metadataServerName, metadataServerPort);
            RemotingServices.Marshal((MetadataServerRemoting)metadataServerRemotingObject, metadataServerName, typeof(MetadataServerRemoting));
            /*RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MetadataServerRemoting),
                metadataServerName,
                WellKnownObjectMode.Singleton); --> APAGAR EM PRINCIPIO*/
            Console.WriteLine("Registered!");

            System.Console.WriteLine("MetadataServer - " + metadataServerName + " -<enter> to leave...");
            System.Console.ReadLine();
        }
    }
}
