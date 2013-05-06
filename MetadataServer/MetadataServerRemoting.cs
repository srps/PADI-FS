﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using PADI_FS_Library;

namespace MetadataServer
{

    /// <summary>
    /// MetadataServer Remoting Class
    /// </summary>

    public class MetadataServerRemoting : MarshalByRefObject, MetadataServerInterface
    {
        //MetadataServer Properties
        string _metadataServerName;
        int _metadataServerPort;

        bool _isMaster;
        bool _isInFailMode;

        Dictionary<string, bool> _aliveServers;

        // Other Metadata Server URLs
        Dictionary<string, MetadataServerInterface> _metadataProxies;

        //DataServers URLS
        Dictionary<string, string> _dataServersURLS;
        //DataServers Proxies
        Dictionary<string, DataServerInterface> _dataServersProxies;

        //Metadata about files being created
        Dictionary<string, FileMetadata> _filesMetadata;

        //Number of files existing in each DataServer
        Dictionary<string, int> _dataServersNumberOfFiles;

        //Name associated to files created in Data Servers
        Dictionary<string, string> _dataServersAssociatedFilenames;

        //LocalFilenames and associated DataServers
        Dictionary<string, List<string>> _localFilesDataServers;

        //List of tasks which will be used to associate missing DataServers to files
        Dictionary<string, int> _filesWithMissingDataServers;

        //Association between existing files and number of clients that are using them
        Dictionary<string, int> _numberOfClientsUsingExistingFile; /*Nao esta a ser usado*/


        
        string _fileUniqueName;
        int _fileUniqueID;



        public MetadataServerRemoting(string metadataServerName, int metadataServerPort)
        {

            //Inicializations
            _metadataServerName = metadataServerName;
            _metadataServerPort = metadataServerPort;

            _isInFailMode = false;
            _isMaster = false;
            bool _seenMaster = false;

            _dataServersURLS = new Dictionary<string, string>();
            _dataServersProxies = new Dictionary<string, DataServerInterface>();

            _filesMetadata = new Dictionary<string, FileMetadata>();

            _dataServersNumberOfFiles = new Dictionary<string, int>();
            _dataServersAssociatedFilenames = new Dictionary<string, string>();
            _localFilesDataServers = new Dictionary<string, List<string>>();

            _filesWithMissingDataServers = new Dictionary<string, int>();


            _fileUniqueName = "LOCAL-DATASERVER-FILENAME-";
            _fileUniqueID = 1;

            TextReader metadataServersPorts = new StreamReader(@"..\..\..\Metadata\bin\Debug\MetadataServersPorts.txt");
            string metadataServersPortsLine;
            string[] metadataServersPortsLineWords;
            while ((metadataServersPortsLine = metadataServersPorts.ReadLine()) != null)
            {
                metadataServersPortsLineWords = metadataServersPortsLine.Split(' ');
                string _lineMDServerName = metadataServersPortsLineWords[0];
                string _lineMDServerPort = metadataServersPortsLineWords[1];
                string _lineMDServerURL = "tcp://localhost:" + _lineMDServerPort + "/" + _lineMDServerName;

                    MetadataServerInterface metadataServerToAdd = (MetadataServerInterface)Activator.GetObject(
                                                                  typeof(MetadataServerInterface),
                                                                  _lineMDServerURL);

                    _metadataProxies.Add(_lineMDServerName, metadataServerToAdd);
                    try
                    {
                        if (_metadataServerName != _lineMDServerName)
                        {
                            _aliveServers.Add(_lineMDServerName, metadataServerToAdd.IsAlive());
                            if(_aliveServers[_lineMDServerName]) {
                                if(Convert.ToInt32(_lineMDServerName.Split('-')[1]) > Convert.ToInt32(_metadataServerName.Split('-')[1]))
                                {
                                    if(metadataServerToAdd.IsMaster()){
                                        metadataServerToAdd.Ping(_metadataServerName);
                                        _seenMaster = true;
                                    }
                                }
                                else {
                                      if(!_seenMaster) {
                                          _isMaster = true;
                                          metadataServerToAdd.TakeControl(_metadataServerName);
                                      }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _aliveServers.Add(_lineMDServerName, false);
                    }
            }
        }

        //Auxiliar Functions


        public void Ping(string _MDServerName) { }

        public MetadataServerInformation TakeControl(string _MDServerName) {
            _isMaster = false;
            return new MetadataServerInformation(_aliveServers,
                _metadataProxies,
                _dataServersURLS,
                _dataServersProxies,
                _filesMetadata,
                _dataServersNumberOfFiles,
                _dataServersAssociatedFilenames,
                _localFilesDataServers,
                _filesWithMissingDataServers,
                _numberOfClientsUsingExistingFile,
                _fileUniqueID);
        }

        public bool IsMaster()
        {
            return _isMaster;
        }

        public bool IsAlive()
        {
            return !_isInFailMode;
        }

        private void LogPrint(string toPrint)
        {
            Console.WriteLine(toPrint);
        }

        private string generateFilenameForDataServers()
        {
            string filenameToReturn = _fileUniqueName + _fileUniqueID;
            _fileUniqueID++;
            return filenameToReturn;
        }

        /*
        public bool RemoveFromFilesWithMissingDataServersPredicate(Tuple<string, int> element)
        {
            if (element.Item2 == 0)
                return true;
            else return false;
        }*/


        //Main Operations

        //Create Operation
        public FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            //Verification if the file with this filename already exists, if it does, returns an error
            if (_filesMetadata.ContainsKey(filename))
            {
                //TODO
            }



            //Associates this filename with a unique filename for local DataServer storage
            string associatedFilename = generateFilenameForDataServers();
            _dataServersAssociatedFilenames.Add(filename, associatedFilename);
            //Creates an association between filename and a list with data servers to fill in
            _localFilesDataServers.Add(associatedFilename, new List<string>());


            //Association between dataServers and local filenames for the filename asked
            List<Tuple<string, string>> dataServersLocalFilenames = new List<Tuple<string, string>>();

            //Computation about who will have filename asked to create and store it in the dataServersLocalFilenames list
            Dictionary<string, int> sortedDataServersNumberOfFiles = _dataServersNumberOfFiles.OrderBy(x => x.Value).ToDictionary(p => p.Key, q => q.Value);

            int counter = 0;
            foreach (string dataServerName in sortedDataServersNumberOfFiles.Keys)
            {
                _dataServersProxies[dataServerName].createFilename(associatedFilename);
                _dataServersNumberOfFiles[dataServerName] = _dataServersNumberOfFiles[dataServerName] + 1;
                _localFilesDataServers[associatedFilename].Add(dataServerName);

                dataServersLocalFilenames.Add(new Tuple<string, string>(_dataServersURLS[dataServerName], associatedFilename));


                if (++counter == numberOfDataServers)
                    break;
            }

            // If there are missing DataServers to associate to this file, it creates a task for it (which include the number of DataServers missing to associate to this file)
            // and it will be executed when appropriate
            if (counter < numberOfDataServers)
            {
                _filesWithMissingDataServers.Add(filename, numberOfDataServers - counter);
            }

            FileMetadata newFileMetadata = new FileMetadata(filename, numberOfDataServers, readQuorum, writeQuorum, dataServersLocalFilenames);

            _filesMetadata.Add(filename, newFileMetadata);

            return newFileMetadata;
        }

        // Open Operation
        public FileMetadata open(string filename)
        {
            return _filesMetadata[filename];
        }

        // Close Operation
        public void close(string filename)
        {
            return;
        }

        // Delete Operation
        public void delete(string filename)
        {
            // If filename exists, delete filemetadata stored from filename
            // Else returns because it doesnt exist
            if (_filesMetadata.ContainsKey(filename))
                _filesMetadata.Remove(filename);
            else return;
            
            // Delete association between filename and local filename in data servers, but saves local filename for next operations
            string local_dataserver_filename_associated = null;
            if (_dataServersAssociatedFilenames.ContainsKey(filename))
            {
                local_dataserver_filename_associated = _dataServersAssociatedFilenames[filename];
                _dataServersAssociatedFilenames.Remove(filename);
            }
            
            // For each data server associated with that local filename, tell them to delete the file, decrement the number if files
            // existing in that data server and in the end delete the association between this local file and those data servers
            /*FALTA A PARTE DE OS DATA SERVERS ESTAREM EM BAIXO*/
            if (_localFilesDataServers.ContainsKey(local_dataserver_filename_associated))
            {
                List<string> dataServersWithFile = _localFilesDataServers[local_dataserver_filename_associated];

                foreach (string dataServerNameWithFile in dataServersWithFile)
                {
                    _dataServersProxies[dataServerNameWithFile].deleteFilename(local_dataserver_filename_associated);
                    _dataServersNumberOfFiles[dataServerNameWithFile] = _dataServersNumberOfFiles[dataServerNameWithFile] - 1;
                }

                _localFilesDataServers.Remove(local_dataserver_filename_associated);
            }
            
            //If exists an association saying that there are still DataServers to add to this file, delete it
            foreach (string fileWithMissingDataServersName in _filesWithMissingDataServers.Keys)
                if(fileWithMissingDataServersName == filename)
                {
                    _filesWithMissingDataServers.Remove(fileWithMissingDataServersName);
                    break;
                }
            

        }


        //Fail Operation
        public void fail()
        {
            _isInFailMode = true;
        }

        //Recover Operation
        public void recover()
        {
            _isInFailMode = false;
        }

        //Dump Operation
        //This operation doesnt only prints the files metadata saved in this process, but also sends it, in a text form way, to the
        //process caller (Puppet Master) for local printing
        public string dump()
        {
            string dumpInfo = "";

            int fileNumber = 1;
            string toDump = null;
            foreach (string filename in _filesMetadata.Keys)
            {
                dumpInfo += "File stored number " + fileNumber++ + " information:" + "\r\n";
                toDump = _filesMetadata[filename].ToString();
                LogPrint(toDump);
                dumpInfo += toDump + "\r\n";
            }

            return dumpInfo;
        }

        //Other Operations

        //RegisterDataServer Operation
        public void registerDataServer(string dataServerName, string dataServerURL)
        {
            DataServerInterface dataServerProxy = (DataServerInterface)Activator.GetObject(
                                            typeof(DataServerInterface),
                                            dataServerURL);

            //Saving DataServer stuff in MetaDataServer context
            _dataServersURLS.Add(dataServerName, dataServerURL);
            _dataServersProxies.Add(dataServerName, dataServerProxy);
            _dataServersNumberOfFiles.Add(dataServerName, 0);

            //LogPrint("COUNT: " + _filesWithMissingDataServers.Count);
            //for each file with missing data servers, associate this data server
            foreach (string fileWithMissingDataServerName in _filesWithMissingDataServers.Keys)
            {
                _dataServersProxies[dataServerName].createFilename(_dataServersAssociatedFilenames[fileWithMissingDataServerName]);
                _dataServersNumberOfFiles[dataServerName] = _dataServersNumberOfFiles[dataServerName] + 1;
                _localFilesDataServers[_dataServersAssociatedFilenames[fileWithMissingDataServerName]].Add(dataServerName);
                _filesMetadata[fileWithMissingDataServerName].FileDataServersLocations.Add(new Tuple<string, string>(dataServerURL, _dataServersAssociatedFilenames[fileWithMissingDataServerName]));
            }

            List<string> filesWithMissingDataServersToList = _filesWithMissingDataServers.Keys.ToList();
            foreach (string fileWithMissingDataServerName in filesWithMissingDataServersToList)
                _filesWithMissingDataServers[fileWithMissingDataServerName] = _filesWithMissingDataServers[fileWithMissingDataServerName] - 1;

            //if there is any file where there are not anymore data servers to associate, then remove it from appropriate list
            foreach (KeyValuePair<string, int> s in _filesWithMissingDataServers.Where(p => p.Value == 0).ToList())
            {
                _filesWithMissingDataServers.Remove(s.Key);
            }



        }
    }
}
