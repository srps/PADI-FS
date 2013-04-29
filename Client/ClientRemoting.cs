using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.IO;
using PADI_FS_Library;

using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

namespace Client
{

    //Delegate for Read Operation
    public delegate PADI_FS_Library.File RemoteAsyncGetFileDelegate(string filename, string typeOfRead);
    //Delegate for Write Operation
    public delegate void RemoteAsyncWriteFileDelegate(string filename, int versionNumber, string contentToWrite);

    /// <summary>
    /// Client Remoting Class
    /// </summary>


    public class ClientRemoting : MarshalByRefObject, ClientInterface
    {
        //Client Properties
        string _clientName;
        int _clientPort;

        //Constant Variables -> Only initialized here
        const int numberOfRegisters = 10;

        //Variables -> Only initialized here
        int _registerPosition = 0;
        int _readResponses = 0;
        List<PADI_FS_Library.File> _readFilesResponses = new List<PADI_FS_Library.File>();
        int _writeResponses = 0;


        //MetadataServers Proxys
        Dictionary<string, MetadataServerInterface> _metadataServersProxys;
        //Arrays with 10 positions for storing file Metadata being used
        FileMetadata[] _filesMetadata;
        //Arrays with 10 positions for storing files (contents) being used...
        PADI_FS_Library.File[] _filesContents;
        //...and respective file metadata position in _filesMetadata array -> ???
        int[] _posOfFilesContentInFilesMetadata;
        
        public ClientRemoting(string clientName, int clientPort)
        {
            //Inicialize variables
            _clientName = clientName;
            _clientPort = clientPort;

            _metadataServersProxys = new Dictionary<string, MetadataServerInterface>();
            _filesMetadata = new FileMetadata[numberOfRegisters];
            _filesContents = new PADI_FS_Library.File[numberOfRegisters];
            _posOfFilesContentInFilesMetadata = new int[numberOfRegisters];
            for (int i = 0; i < numberOfRegisters; i++)
            {
                _filesMetadata[i] = null;
                _filesContents[i] = null;
            }

            //Populate Metadata Servers Proxys
            TextReader metadataServersPorts = new StreamReader("..\\..\\..\\Client\\bin\\Debug\\MetadataServersPorts.txt");
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

                _metadataServersProxys.Add(metadataServerName, metadataServerToAdd);
            }

        }

        //Auxiliar Functions

        private void LogPrint(string toPrint)
        {
            Console.WriteLine(toPrint);
        }

        private void saveFileMetadata(FileMetadata fileMetadata)
        {
            _filesMetadata[_registerPosition] = fileMetadata;

            if (++_registerPosition == numberOfRegisters)
                _registerPosition = 0;
        }

        // This is the call that the Read AsyncCallBack delegate will reference.
        public void GetFileRemoteAsyncCallBack(IAsyncResult ar)
        {

            // Alternative 2: Use the callback to get the return value
            RemoteAsyncGetFileDelegate del = (RemoteAsyncGetFileDelegate)((AsyncResult)ar).AsyncDelegate;
            try
            {
                PADI_FS_Library.File file = del.EndInvoke(ar);
                _readFilesResponses.Add(file);
                _readResponses++;
            }
            catch (Exception)
            {
                LogPrint("Client - Read Operation - Something wrong happened! - maybe file doesnt exist");
            }

            return;
        }

        // This is the call that the Write AsyncCallBack delegate will reference.
        public void WriteFileRemoteAsyncCallBack(IAsyncResult ar)
        {

            // Alternative 2: Use the callback to get the return value
            //RemoteAsyncGetFileDelegate del = (RemoteAsyncGetFileDelegate)((AsyncResult)ar).AsyncDelegate;
            try
            {
                _writeResponses++;
            }
            catch (Exception)
            {
                LogPrint("Client - Write Operation - Something wrong happened! - maybe file doesnt exist");
            }

            return;
        }

        //Main Operations

        //Create Operation
        public FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            LogPrint("Create Operation");
            LogPrint("\tFile: " + filename + " Number of Data Servers: " + numberOfDataServers + " ReadQuorum: " + readQuorum + " WriteQuorum: " + writeQuorum); 
            

            FileMetadata fileMetadataToCreate = null;

            //If client calls each MetadataServer in order, then if someone awnsers it will be the master/primary
            foreach(string metadataServerName in _metadataServersProxys.Keys)
            {
                try
                {
                    fileMetadataToCreate = _metadataServersProxys[metadataServerName].create(filename, numberOfDataServers, readQuorum, writeQuorum);
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Create Operation - Something wrong happened");
                }
            }

            //Save file metadata created in client side
            saveFileMetadata(fileMetadataToCreate);
            
            LogPrint("Sucessful!");

            return fileMetadataToCreate;
        }

        //Open Operation
        public FileMetadata open(string filename)
        {
            LogPrint("Open Operation");
            LogPrint("\tFile: " + filename);

            FileMetadata fileMetadataToOpen = null;

            foreach (string metadataServerName in _metadataServersProxys.Keys)
            {
                try
                {
                    fileMetadataToOpen = _metadataServersProxys[metadataServerName].open(filename);
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Open Operation - Something wrong happened");
                }
            }

            //Save file metadata created in client side (maybe there will be some changes -> more DataServers associated)
            saveFileMetadata(fileMetadataToOpen);

            LogPrint("Sucessful!");

            return fileMetadataToOpen;
        }

        //Close Operation
        public void close(string filename)
        {
            LogPrint("Close Operation");
            LogPrint("\tFile: " + filename);

            foreach (string metadataServerName in _metadataServersProxys.Keys)
            {
                try
                {
                    _metadataServersProxys[metadataServerName].close(filename);

                    for (int i = 0; i < numberOfRegisters; i++)
                    {
                        if (_filesMetadata[i].Filename == filename)
                        {
                            _filesMetadata[i] = null;
                            break;
                        }
                    }

                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Close Operation - Something wrong happened");
                }
            }

            LogPrint("Sucessful!");

            return;
        }
        
        //Delete Operation
        public void delete(string filename)
        {
            LogPrint("Delete Operation");
            LogPrint("\tFile: " + filename);

            foreach (string metadataServerName in _metadataServersProxys.Keys)
            {
                try
                {
                    _metadataServersProxys[metadataServerName].delete(filename);

                    for (int i = 0; i < numberOfRegisters; i++)
                    {
                        if (_filesMetadata[i].Filename == filename)
                        {
                            _filesMetadata[i] = null;
                        }
                    }

                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Delete Operation - Something wrong happened");
                }
            }

            LogPrint("Sucessful!");

            return;
        }

        //Read Operation
        public string read(int fileRegister, string semantics, int stringRegister)
        {
            //Read variables restore
            _readResponses = 0;
            _readFilesResponses = new List<PADI_FS_Library.File>();


            LogPrint("Read Operation");
            LogPrint("\tFile Register: " + fileRegister + " Semantics: " + semantics + " String Register: " + stringRegister);

            FileMetadata fileMetadataToRead = _filesMetadata[fileRegister];

            //if the file register to read is null (that is, if that file does not exists in the client) sends an exception
            if(fileMetadataToRead == null)
            {
                //TODO
            }

            //Call every Data Server that contains the file
            foreach(Tuple<string, string> fileDataServerLocation in fileMetadataToRead.FileDataServersLocations)
            {
                DataServerInterface dataServerProxy = (DataServerInterface)Activator.GetObject(
                                                        typeof(DataServerInterface),
                                                        fileDataServerLocation.Item1);

                // Alternative 2: asynchronous call with callback
                // Create delegate to remote method
                RemoteAsyncGetFileDelegate RemoteDel = new RemoteAsyncGetFileDelegate(dataServerProxy.read);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(this.GetFileRemoteAsyncCallBack);
                // Call remote method
                IAsyncResult RemAr = RemoteDel.BeginInvoke(fileDataServerLocation.Item2, semantics, RemoteCallback, null);
            }

            //Waits for a quorum of Read Responses
            while (_readResponses < fileMetadataToRead.ReadQuorum)
            {
                continue;
            }

            //After getting a quorum decides what to save (the most recent version of the files obtained)
            int counter = 0;
            int version = -1;
            PADI_FS_Library.File fileToSave = null;
            foreach (PADI_FS_Library.File fileRead in _readFilesResponses)
            {
                if (counter == fileMetadataToRead.ReadQuorum)
                    break;

                if (fileRead.VersionNumber >= version) //MAIOR APENAS OU MAIOR OU IGUAL ???
                {
                    version = fileRead.VersionNumber;
                    fileToSave = fileRead;
                }

                counter++;
            }

            //fileToSave = null problem (maybe because someone deleted the file in the exact moment this client was going to read it, etc)
            if (fileToSave == null)
            {
                if (_filesContents[stringRegister] == null)
                {
                    return "File Doesnt Exist"; // MUDAR PARA EXCEPCAO???
                }
            }
            else
            {
                //Array registers update
                if (_filesContents[stringRegister] == null || _filesContents[stringRegister].VersionNumber <= fileToSave.VersionNumber)
                    _filesContents[stringRegister] = fileToSave;

            }

            //Association between position of string register and file metadata register update -> ???
            _posOfFilesContentInFilesMetadata[stringRegister] = fileRegister;

            LogPrint("File Read Contents: " + _filesContents[stringRegister].FileContents);
            LogPrint("Sucessful!");

            return _filesContents[stringRegister].FileContents;
        }

        //Write Operation
        public void write(int fileRegister, string contentToWrite)
        {
            //Write variables restore
            _writeResponses = 0;

            LogPrint("Write Operation");
            LogPrint("\tFile Register: " + fileRegister + " Contents: " + contentToWrite);

            PADI_FS_Library.File fileToWrite = _filesContents[fileRegister];

            //if the file register to write is null (that is, if that file does not exists in the client) then
            //the version number used to write will be 0, otherwise it will be the version number of the existing file plus one
            int versionNumberToWrite;
            FileMetadata fileMetadataToWrite;
            if (fileToWrite == null)
            {
                versionNumberToWrite = 0;
                fileMetadataToWrite = _filesMetadata[fileRegister];
            }
            else
            {
                versionNumberToWrite = fileToWrite.VersionNumber + 1;
                fileMetadataToWrite = _filesMetadata[_posOfFilesContentInFilesMetadata[fileRegister]];
            }
            
            //LogPrint("---------VERSION TO WRITE: " + versionNumberToWrite);
            //LogPrint("---------POSITION IN FILES CONTENTS FILES META: " + _posOfFilesContentInFilesMetadata[fileRegister]);

            
            //Call every Data Server that contains the file
            foreach (Tuple<string, string> fileDataServerLocation in fileMetadataToWrite.FileDataServersLocations)
            {
                DataServerInterface dataServerProxy = (DataServerInterface)Activator.GetObject(
                                                        typeof(DataServerInterface),
                                                        fileDataServerLocation.Item1);

                // Alternative 2: asynchronous call with callback
                // Create delegate to remote method
                RemoteAsyncWriteFileDelegate RemoteDel = new RemoteAsyncWriteFileDelegate(dataServerProxy.write);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(this.WriteFileRemoteAsyncCallBack);
                // Call remote method
                IAsyncResult RemAr = RemoteDel.BeginInvoke(fileDataServerLocation.Item2, versionNumberToWrite, contentToWrite, RemoteCallback, null);
            }

            //LogPrint("WRITE RESPONSES: " + _writeResponses);
            //LogPrint("WRITE QUORUM: " + fileMetadataToWrite.WriteQuorum);
            
            //Waits for a quorum of Write Responses
            while (_writeResponses < fileMetadataToWrite.WriteQuorum) //ESTA A HAVER PROBLEMAS AQUI ???
            {
                continue;
            }
            

            //Updates local registers (not really necessary because before any write the client has to make a read)
            //Info: They arent updated if file didnt exist before
            if (fileToWrite != null)
            {
                _filesContents[fileRegister].VersionNumber = versionNumberToWrite;
                _filesContents[fileRegister].FileContents = contentToWrite;
            }

            LogPrint("Sucessful!");

            return;
        }

        //Dump Operation
        public string dump()
        {
            LogPrint("Dump Operation");

            string toReturn = "";

            LogPrint("Array File Metadata Registers");
            toReturn += "Array File Metadata Registers" + "\r\n";
            foreach (FileMetadata fileMetadata in _filesMetadata)
            {
                if (fileMetadata != null)
                {
                    string fileMetadataToString = fileMetadata.ToString();
                    LogPrint(fileMetadataToString);
                    toReturn += fileMetadataToString + "\r\n";
                }                
            }

            LogPrint("Array File Contents Registers");
            toReturn += "Array File Contents Registers" + "\r\n";
            foreach (PADI_FS_Library.File file in _filesContents)
            {
                if (file != null)
                {
                    string fileContentsToString = file.ToString();
                    LogPrint(fileContentsToString);
                    toReturn += fileContentsToString + "\r\n";
                }
            }

            LogPrint("Sucessful!");

            return toReturn;
        }
    }
}
