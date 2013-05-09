using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
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

            /*TcpChannel channel = new TcpChannel(metadataServerPort);
            ChannelServices.RegisterChannel(channel, true);*/

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = dataServerPort;
            props["timeout"] = 200000;

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            //Registering DataServer service
            Console.WriteLine("Registering DataServer as " + dataServerName + " with port " + dataServerPort);
            DataServerRemoting dataServerRemotingObject = new DataServerRemoting(dataServerName, dataServerPort);
            RemotingServices.Marshal((DataServerRemoting)dataServerRemotingObject, dataServerName, typeof(DataServerRemoting));
            dataServerRemotingObject.register();
            /*RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MetadataServerRemoting),
                metadataServerName,
                WellKnownObjectMode.Singleton); --> APAGAR EM PRINCIPIO*/
            Console.WriteLine("Registered!");
            
            System.Console.WriteLine("DataServer - " + dataServerName + " -<enter> to leave...");
            System.Console.ReadLine();
        }
    }
}
