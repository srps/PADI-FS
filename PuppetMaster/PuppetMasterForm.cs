using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;


using System.Diagnostics;

using PADI_FS_Library;

namespace PuppetMaster
{
    /// <summary>
    /// Puppet Master Class
    /// </summary>

    public partial class PuppetMasterForm : Form
    {
        //Puppet Master channel for components connections
        TcpChannel channel = null; 

        //Every client and server is stored with 2 items: 
        // - name of process
        // - tuple with url and process network link
        private Dictionary<string, Tuple<string, ClientInterface>> clients;
        private Dictionary<string, Tuple<string, MetadataServerInterface>> metadataServers;
        private Dictionary<string, Tuple<string, DataServerInterface>> dataServers;

        //Ports to register clients and servers, anytime someone registers, this ports increments
        int clientStartPort;
        int metadataServerStartPort;
        int dataServerStartPort;

        TextReader script;

        public PuppetMasterForm()
        {
            InitializeComponent();

            //Inicializations
            puppet_master_history_TextBox.Text = "";
            dump_history_TextBox.Text = "";
            command_TextBox.Text = "";

            channel = new TcpChannel(8400);
            ChannelServices.RegisterChannel(channel, true);

            clients = new Dictionary<string, Tuple<string, ClientInterface>>();
            metadataServers = new Dictionary<string, Tuple<string, MetadataServerInterface>>();
            dataServers = new Dictionary<string, Tuple<string, DataServerInterface>>();

            clientStartPort = 8100;
            metadataServerStartPort = 8200;
            dataServerStartPort = 8300;

            script = null;
        }


        void LogPrint(string text)
        {
            puppet_master_history_TextBox.Text += text + "\r\n";
        }

        void DumpPrint(string text)
        {
            dump_history_TextBox.Text += text + "\r\n";
        }

        void runCommand(string command)
        {
            String commandType = null;
            String commandProcess = null;
            String textToWrite;
            ArrayList commandParameters = new ArrayList();
          
            //Deals with the command, saving some of its properties
            command = command.Replace(",", "");
            string[] commandWords = command.Split(' ');
            commandType = commandWords[0];
            commandProcess = commandWords[1];

            //CREATE COMMAND - (ONLY TO CLIENTS)
            if (commandType.ToUpper() == "CREATE")
            {
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if(!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        /*Falta aqui a passagem dos links para os metadataservers ???*/
                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }
                    
                   //call to the client for file creation
                   FileMetadata fileMetadata = clients[commandProcess].Item2.create(commandWords[2], Convert.ToInt32(commandWords[3]), Convert.ToInt32(commandWords[4]), Convert.ToInt32(commandWords[5]));


                   
                   LogPrint(fileMetadata.ToString());

                }

                return;
            }


            //OPEN COMMAND - (ONLY TO CLIENTS)
            if (commandType.ToUpper() == "OPEN")
            {
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for file opening
                    FileMetadata fileMetadata = clients[commandProcess].Item2 .open(commandWords[2]);

                    LogPrint(fileMetadata.ToString());
                }

                return;
            }

            //CLOSE COMMAND - (ONLY TO CLIENTS) 
            if (commandType.ToUpper() == "CLOSE")
            {
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for file closing
                    clients[commandProcess].Item2.close(commandWords[2]);

                    LogPrint("Closing of file " + commandWords[2] + " by client " + commandProcess + " sucessful!");
                }

                return;
            }

            //DELETE COMMAND - (ONLY TO CLIENTS) 
            if (commandType.ToUpper() == "DELETE")
            {
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for file deleting
                    clients[commandProcess].Item2.delete(commandWords[2]);

                    LogPrint("Deleting of file " + commandWords[2] + " by client " + commandProcess + " sucessful!");
                }

                return;
            }

            //READ COMMAND - (ONLY TO CLIENTS) 
            if (commandType.ToUpper() == "READ")
            {
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for file reading
                    string read = clients[commandProcess].Item2.read(Convert.ToInt32(commandWords[2]), commandWords[3], Convert.ToInt32(commandWords[4]));

                    LogPrint("File Register contents: " + read);

                    LogPrint("Reading of file register " + commandWords[2] + " of client " + commandProcess + " sucessful!");
                }

                return;
            }


            //WRITE COMMAND - (ONLY TO CLIENTS) 
            if (commandType.ToUpper() == "WRITE")
            {                
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for file writing
                    if (command.Contains('\"'))
                    {
                        textToWrite = command.Split('\"')[1];
                        clients[commandProcess].Item2.write(Convert.ToInt32(commandWords[2]), textToWrite);
                    } else
                        clients[commandProcess].Item2.write(Convert.ToInt32(commandWords[2]), Convert.ToInt32(commandWords[3]));

                    LogPrint("Writing of file register " + commandWords[2] + " of client " + commandProcess + " sucessful!");
                }

                return;
            }


            //WRITE COMMAND - (ONLY TO CLIENTS) 
            if (commandType.ToUpper() == "COPY")
            {
                //if its a client 
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start(@"..\..\..\Client\bin\Debug\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for file copying
                    string salt = "";
                    if (command.Contains('\"'))
                    {
                        salt = command.Split('\"')[1];
                    }
                    
                    clients[commandProcess].Item2.copy(Convert.ToInt32(commandWords[2]), commandWords[3], Convert.ToInt32(commandWords[4]), System.Text.Encoding.UTF8.GetBytes(salt));

                    LogPrint("Copy of file register " + commandWords[2] + " with string " + salt + " to file register " + commandWords[4] + " of client " + commandProcess + " sucessful!");
                }

                return;
            }



            //RECOVER COMMAND - METADATASERVERS AND DATASERVERS
            if (commandType.ToUpper() == "RECOVER")
            {
                //if its a metadata server 
                if (commandProcess[0] == 'm')
                {
                    //if that MetadataServer doesnt exist in puppet master "database" we have to create it
                    if (!metadataServers.ContainsKey(commandProcess))
                    {
                        //Check what MetadataServer is (the number of it)
                        string[] metadataInfo = commandProcess.Split('-');
                        int metadataServerPort = metadataServerStartPort + Convert.ToInt32(metadataInfo[1]);

                        //Creation of process MetadataServer
                        Process.Start("..\\..\\..\\MetadataServer\\bin\\Debug\\MetadataServer.exe", commandProcess + " " + metadataServerPort);

                        //Saving MetadataServer stuff in puppet master
                        string newMetadataServerURL = "tcp://localhost:" + metadataServerPort + "/" + commandProcess;
                        MetadataServerInterface metadataServerProxy = (MetadataServerInterface)Activator.GetObject(
                                                        typeof(MetadataServerInterface),
                                                        newMetadataServerURL);

                        metadataServers.Add(commandProcess, new Tuple<string, MetadataServerInterface>(newMetadataServerURL, metadataServerProxy)); 
                    }

                    //call to the MetadataServer for recover
                    metadataServers[commandProcess].Item2.recover();

                    LogPrint("Recover of " + commandProcess + " successful!");

                }
                else if (commandProcess[0] == 'd')
                        {


                        }

                return;
            }

            //UNFREEZE COMMAND - (ONLY TO DATASERVERS)
            if (commandType.ToUpper() == "UNFREEZE")
            {
                //if its a data server (just to check)
                if (commandProcess[0] == 'd')
                {
                    //if that DataServer doesnt exist in puppet master "database" we have to create it
                    if (!dataServers.ContainsKey(commandProcess))
                    {
                        //Check what DataServer is (the number of it)
                        string[] dataInfo = commandProcess.Split('-');
                        int dataServerPort = dataServerStartPort + Convert.ToInt32(dataInfo[1]);

                        //Creation of process DataServer
                        Process.Start("..\\..\\..\\DataServer\\bin\\Debug\\DataServer.exe", commandProcess + " " + dataServerPort);

                        //Saving DataServer stuff in puppet master
                        string newDataServerURL = "tcp://localhost:" + dataServerPort + "/" + commandProcess;
                        DataServerInterface dataServerProxy = (DataServerInterface)Activator.GetObject(
                                                        typeof(DataServerInterface),
                                                        newDataServerURL);

                        dataServers.Add(commandProcess, new Tuple<string, DataServerInterface>(newDataServerURL, dataServerProxy));
                    }

                    //call to the DataServer for unfreeze
                    dataServers[commandProcess].Item2.unfreeze();

                    LogPrint("Unfreeze of " + commandProcess + " successful!");
                }

                return;
            }

            //DUMP COMMAND 
            if (commandType.ToUpper() == "DUMP")
            {
                //if its a client
                if (commandProcess[0] == 'c')
                {
                    //if that client doesnt exist in puppet master "database" we have to create it
                    if (!clients.ContainsKey(commandProcess))
                    {
                        string[] clientInfo = commandProcess.Split('-');
                        int clientPort = clientStartPort + Convert.ToInt32(clientInfo[1]);

                        //Creation of process client
                        Process.Start("..\\..\\..\\Client\\bin\\Debug\\Client.exe", commandProcess + " " + clientPort);

                        //Saving client stuff in puppet master
                        string newCLientURL = "tcp://localhost:" + clientPort + "/" + commandProcess;
                        ClientInterface clientProxy = (ClientInterface)Activator.GetObject(
                                                        typeof(ClientInterface),
                                                        newCLientURL);

                        clients.Add(commandProcess, new Tuple<string, ClientInterface>(newCLientURL, clientProxy));
                    }

                    //call to the client for dumping
                    string dumpInfo = clients[commandProcess].Item2.dump();

                    DumpPrint("\tDump of Process " + commandProcess);
                    DumpPrint(dumpInfo);

                    LogPrint("Dump of " + commandProcess + " successful!");
                }


                //if its a data server
                if (commandProcess[0] == 'd')
                {
                    //if that DataServer doesnt exist in puppet master "database" we have to create it
                    if (!dataServers.ContainsKey(commandProcess))
                    {
                        //Check what DataServer is (the number of it)
                        string[] dataInfo = commandProcess.Split('-');
                        int dataServerPort = dataServerStartPort + Convert.ToInt32(dataInfo[1]);

                        //Creation of process DataServer
                        Process.Start("..\\..\\..\\DataServer\\bin\\Debug\\DataServer.exe", commandProcess + " " + dataServerPort);

                        //Saving DataServer stuff in puppet master
                        string newDataServerURL = "tcp://localhost:" + dataServerPort + "/" + commandProcess;
                        DataServerInterface dataServerProxy = (DataServerInterface)Activator.GetObject(
                                                        typeof(DataServerInterface),
                                                        newDataServerURL);

                        dataServers.Add(commandProcess, new Tuple<string, DataServerInterface>(newDataServerURL, dataServerProxy));
                    }

                    //call to the DataServer for dumping
                    string dumpInfo = dataServers[commandProcess].Item2.dump();

                    DumpPrint("\tDump of Process " + commandProcess);
                    DumpPrint(dumpInfo);

                    LogPrint("Dump of " + commandProcess + " successful!");
                }

                //if its a metadata server
                if (commandProcess[0] == 'm')
                {
                    //if that MetadataServer doesnt exist in puppet master "database" we have to create it
                    if (!metadataServers.ContainsKey(commandProcess))
                    {
                        //Check what MetadataServer is (the number of it)
                        string[] metadataInfo = commandProcess.Split('-');
                        int medataServerPort = metadataServerStartPort + Convert.ToInt32(metadataInfo[1]);

                        //Creation of process DataServer
                        Process.Start("..\\..\\..\\MetadataServer\\bin\\Debug\\MetadataServer.exe", commandProcess + " " + medataServerPort);

                        //Saving DataServer stuff in puppet master
                        string newMetadataServerURL = "tcp://localhost:" + medataServerPort + "/" + commandProcess;
                        MetadataServerInterface metadataServerProxy = (MetadataServerInterface)Activator.GetObject(
                                                        typeof(MetadataServerInterface),
                                                        newMetadataServerURL);

                        metadataServers.Add(commandProcess, new Tuple<string, MetadataServerInterface>(newMetadataServerURL, metadataServerProxy));
                    }

                    //call to the MetadataServer for dumping
                    string dumpInfo = metadataServers[commandProcess].Item2.dump();

                    DumpPrint("\tDump of Process " + commandProcess);
                    DumpPrint(dumpInfo);

                    LogPrint("Dump of " + commandProcess + " successful!");
                }

                return;
            }

            LogPrint("COMMAND NOT RECOGNISED - TRY ANOTHER COMMAND");

        }


        //Loads the local script and opens it for future readings
        private void load_script_Click(object sender, EventArgs e)
        {
            //Open File Dialog properties
            OpenFileDialog _openFileDialog_PADI_FS = new OpenFileDialog();
            _openFileDialog_PADI_FS.InitialDirectory = @".";
            _openFileDialog_PADI_FS.RestoreDirectory = true;
            _openFileDialog_PADI_FS.Title = "Choose Script";
            _openFileDialog_PADI_FS.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            _openFileDialog_PADI_FS.Multiselect = false;
            _openFileDialog_PADI_FS.CheckFileExists = true;
            _openFileDialog_PADI_FS.CheckPathExists = true;

            if (_openFileDialog_PADI_FS.ShowDialog() == DialogResult.OK)
            {
                //Opens script from local file system (ready to read)
                script = new StreamReader(_openFileDialog_PADI_FS.FileName.ToString());
            }
        }

        private void run_script_Click(object sender, EventArgs e)
        {
            String command = null;

            //Script wasnt loaded yet
            if (script == null)
            {
                LogPrint("Please, load the script first");
                return;
            }

            //Check if its not end of script
            while ((command = script.ReadLine()) != null)
            {
                //If its a comment, ignore
                if (command[0] == '#')
                    continue;

                //Run command
                LogPrint("Command to RUN:" + command);

                runCommand(command);

                LogPrint("----------------------------------------------");

                
            }

            //End of script (close script)
            script.Close();
            script = null;

            return;
        }

        //Executes next step in the script previously loaded (any comment line is ignored)
        private void next_step_script_Click(object sender, EventArgs e)
        {
            String command = null;

            //Script wasnt loaded yet
            if (script == null)
            {
                LogPrint("Please, load the script first");
                return;
            }
            
            //Check if its not end of script but its a comment, then keep ignoring
            while ((command = script.ReadLine()) != null && command[0] == '#') 
            {
                    continue;
            }

            //Check if its end of script
            if (command == null)
            {
                script.Close();
                script = null;
                return;
            }

            /*Debug only - APAGAR*/
            LogPrint("Command to RUN:" + command);

            runCommand(command);
           
            LogPrint("----------------------------------------------");
        }

        //Executes one command previously entered in the run_command_TextBox
        private void run_command_Click(object sender, EventArgs e)
        {
            LogPrint("Command to RUN:" + command_TextBox.Text);
            runCommand(command_TextBox.Text);
            LogPrint("----------------------------------------------");

            command_TextBox.Text = "";
        }

        


        

        
    }
}
