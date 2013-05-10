using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PADI_FS_Library;

using System.Threading;
using System.Collections;




namespace DataServer
{
    public class DataServerRemoting : MarshalByRefObject, DataServerInterface
    {
        // DataServer Properties
        string _dataServerName;
        int _dataServerPort;

        // For PuppetMaster ops
        bool _isInFreezeMode;
        bool _isInFailMode;

        //Data about files being created
        Dictionary<string, PADI_FS_Library.File> _filesData;

        public DataServerRemoting(string dataServerName, int dataServerPort)
        {
            // Inicializations
            _dataServerName = dataServerName;
            _dataServerPort = dataServerPort;

            _isInFreezeMode = false;
            _isInFailMode = false;

            _filesData = new Dictionary<string, PADI_FS_Library.File>();
        }

        //Auxiliar Functions

        private void LogPrint(string toPrint)
        {
            Console.WriteLine(toPrint);
        }


        // Main Operations

        // Read Operation
        public PADI_FS_Library.File read(string filename, string semantics) //semantics ... QUAL O USO DISTO????
        {
                   

            if (_isInFreezeMode)
            {

                //Freeze recover mode
                lock (this)
                {
                    Monitor.Wait(this);
                }

                LogPrint("Read Operation");
                LogPrint("\t Filename: " + filename + " Semantics: " + semantics);

                if (_filesData.ContainsKey(filename))
                {
                    LogPrint("Successful!");

                    lock (this) { Monitor.Pulse(this); }

                    return _filesData[filename];
                }
                else
                {
                    lock (this) { Monitor.Pulse(this); }

                    throw new FileDoesntExistException(filename);
                }

            }
            else
            {
                lock (this)
                {
                    LogPrint("Read Operation");
                    LogPrint("\t Filename: " + filename + " Semantics: " + semantics);

                    //Normal operating
                    if (_filesData.ContainsKey(filename))
                    {
                        LogPrint("Successful!");

                        return _filesData[filename];
                    }
                    else
                    {
                        //throw new Exception();
                        throw new FileDoesntExistException(filename);
                    }
                }
                
            }

            

        }

        //Write Operation
        public bool write(string filename, int versionNumber, string contentToWrite)
        {

            if (_isInFreezeMode)
            {
                //Freeze recover mode
                lock (this)
                {
                    Monitor.Wait(this);
                }

                LogPrint("Write Operation");
                LogPrint("\t Filename: " + filename + " Version Number: " + versionNumber + " Contents: " + contentToWrite);

                if (_filesData.ContainsKey(filename))
                {
                    if (_filesData[filename].VersionNumber < versionNumber)
                    {
                        _filesData[filename].VersionNumber = versionNumber;
                        _filesData[filename].FileContents = contentToWrite;
                    }
                    
                    lock (this) { Monitor.Pulse(this); }
                }
                else
                {
                    lock (this) { Monitor.Pulse(this); }
                    return false;
                }
            }
            else
            {
                lock (this)
                {
                    LogPrint("Write Operation");
                    LogPrint("\t Filename: " + filename + " Version Number: " + versionNumber + " Contents: " + contentToWrite);

                    //Normal operating
                    if (_filesData.ContainsKey(filename))
                    {
                        if (_filesData[filename].VersionNumber < versionNumber)
                        {
                            _filesData[filename].VersionNumber = versionNumber;
                            _filesData[filename].FileContents = contentToWrite;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            LogPrint("Successful!");

            return true;
        }

        //Freeze Operation
        public void freeze()
        {
            _isInFreezeMode = true;
            LogPrint("--------------------------FREEZE MODE BEGINS-----------------------------");
        }

        //Unfreeze Operation
        public void unfreeze()
        {
            _isInFreezeMode = false;
            LogPrint("---------------------------FREEZE MODE ENDS------------------------------");
            lock (this) { Monitor.Pulse(this); }
        }

        //Fail Operation
        public void fail()
        {
            lock (this)
            {
                _isInFailMode = true;
            }
            LogPrint("---------------------------FAIL MODE BEGINS------------------------------");
        }

        //Recover Operation
        public void recover()
        {
            lock (this)
            {
                _isInFailMode = false;
            }            
            LogPrint("---------------------------FAIL MODE ENDS------------------------------");
        }

        //Dump Operation
        //This operation doesnt only prints the files saved in this process, but also sends it, in a text form way, to the
        //process caller (Puppet Master) for local printing
        public string dump()
        {
            LogPrint("Dump Operation");

            string dumpInfo = "";

            int fileNumber = 1;
            string toDump = null;
            foreach (string filename in _filesData.Keys)
            {

                dumpInfo += "File stored number " + fileNumber++ + " information:" + "\r\n";
                toDump = _filesData[filename].ToString();
                LogPrint(toDump);
                dumpInfo += toDump + "\r\n";

            }

            LogPrint("Sucessfull!");

            return dumpInfo;
        }





        //Other Operations
        public void createFilename(string filename)
        {
            lock (this)
            {
                LogPrint("Create File Operation");
                LogPrint("\t Filename: " + filename);

                if (!_filesData.ContainsKey(filename))
                    _filesData.Add(filename, new PADI_FS_Library.File(filename, "", 0));

                LogPrint("Sucessfull!");
            }            

            /*if (_isInFreezeMode)
            {
                //Freeze recover mode
                lock (this)
                {
                    Monitor.Wait(this);
                }

                if(!_filesData.ContainsKey(filename))
                    _filesData.Add(filename, new File(filename, "", 0));

                Monitor.Pulse(this);
            }
            else
            {
                if(!_filesData.ContainsKey(filename))
                    _filesData.Add(filename, new File(filename, "", 0));
            }*/
        }

        public void deleteFilename(string filename)
        {
            lock (this)
            {
                LogPrint("Delete File Operation");
                LogPrint("\t Filename: " + filename);

                if (_filesData.ContainsKey(filename))
                    _filesData.Remove(filename);

                LogPrint("Sucessfull!");
            }
            

            /*if (_isInFreezeMode)
            {
                //Freeze recover mode
                lock (this)
                {
                    Monitor.Wait(this);
                }

                if (_filesData.ContainsKey(filename))
                    _filesData.Remove(filename);

                Monitor.Pulse(this);
            }
            else
            {
                if (_filesData.ContainsKey(filename))
                    _filesData.Remove(filename);
            }*/
        }

        public void register()
        {
            //Registo do DataServer nos MetadataServers (basta enviar a um, ele expande o registo)

            Tuple<string, MetadataServerInterface> aliveMetadataServerMaster = isAnyoneAlive();

            while (aliveMetadataServerMaster == null)
                aliveMetadataServerMaster = isAnyoneAlive();

            //Someone (MetadataServer) is alive (no matter who - it wont be null)
            if (aliveMetadataServerMaster != null) //just to check xD
            {
                MetadataServerInterface aliveMetadataServerMasterInterface = aliveMetadataServerMaster.Item2;


                aliveMetadataServerMasterInterface.registerDataServer(_dataServerName, "tcp://localhost:" + _dataServerPort + "/" + _dataServerName);
            }          

        }

        //isAnyoneAlive Operation - gets a random Metadata Server that is alive (master or not)
        private Tuple<string, MetadataServerInterface> isAnyoneAlive()
        {
            TextReader metadataServersPorts; 
            string metadataServersPortsLine;
            string[] metadataServersPortsLineWords;

            string metadataServerName = null;
            string metadataServerPort = null;
            string metadataServerURL = null;

            LinkedList<Tuple<string, MetadataServerInterface>> metadataServersList = new LinkedList<Tuple<string,MetadataServerInterface>>();

            while (true)
            {
                metadataServersPorts = new StreamReader(@"..\..\..\DataServer\bin\Debug\MetadataServersPorts.txt");

                metadataServersList = new LinkedList<Tuple<string, MetadataServerInterface>>();

                while ((metadataServersPortsLine = metadataServersPorts.ReadLine()) != null)
                {
                    metadataServersPortsLineWords = metadataServersPortsLine.Split(' ');
                    metadataServerName = metadataServersPortsLineWords[0];
                    metadataServerPort = metadataServersPortsLineWords[1];
                    metadataServerURL = "tcp://localhost:" + metadataServerPort + "/" + metadataServerName;

                    MetadataServerInterface metadataServerProxy = (MetadataServerInterface)Activator.GetObject(
                                                              typeof(MetadataServerInterface),
                                                              metadataServerURL);
                    Random random = new Random();

                    if(random.Next(2) < 1)
                        metadataServersList.AddFirst(new Tuple<string, MetadataServerInterface>(metadataServerName, metadataServerProxy));
                    else metadataServersList.AddLast(new Tuple<string, MetadataServerInterface>(metadataServerName, metadataServerProxy));
                }
                metadataServersPorts.Close();

                foreach (Tuple<string, MetadataServerInterface> metadata in metadataServersList)
                {
                    try
                    {
                        
                        
                        metadata.Item2.ping();
                        

                        return new Tuple<string, MetadataServerInterface>(metadata.Item1, metadata.Item2);
                    }
                    catch (Exception e)
                    {
                        LogPrint("NOT ONLINE METADATA WITH NAME " + metadata.Item1 + " " + e.Message);
                    }
                }
                
            }

        }

        //Never allow lease to expire
        public override object InitializeLifetimeService()
        {

            return null;

        }
    }
}
