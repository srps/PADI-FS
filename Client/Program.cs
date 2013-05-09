using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using System.Net.Sockets;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();

            string clientName = args[0];
            int clientPort = Convert.ToInt32(args[1]);

            /*TcpChannel channel = new TcpChannel(metadataServerPort);
            ChannelServices.RegisterChannel(channel, true);*/

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = clientPort;
            props["timeout"] = 200000;

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            //Registering client service
            Console.WriteLine("Registering Client as " + clientName + " with port " + clientPort);
            ClientRemoting clientRemotingObject = new ClientRemoting(clientName, clientPort);
            RemotingServices.Marshal((ClientRemoting)clientRemotingObject, clientName, typeof(ClientRemoting));
            /*RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ClientRemoting),
                clientName,
                WellKnownObjectMode.Singleton); --> APAGAR EM PRINCIPIO*/
            Console.WriteLine("Registered!");

            //Registering client in Metadata Servers (if possible) --> TALVEZ NAO SEJA PRECISO E PODE-SE APAGAR
            Console.WriteLine("Connecting and registering into Metadata Servers (if possible)");
            /*Add code here*/
            Console.WriteLine("Connected!");

            System.Console.WriteLine("client - " + clientName + " -<enter> to leave...");
            System.Console.ReadLine();
        }
    }
}
