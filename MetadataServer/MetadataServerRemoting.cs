using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PADI_FS_Library;

using System.IO;

namespace MetadataServer
{

    //For replica syncing
    public delegate void RemoteAsyncSyncReplicasDelegate(MetadataServerInformation infoToUpdate);

    
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

        //Metadata Actual Master Proxy
        MetadataServerInterface _metadataMasterActualProxy;

        //MetadataServers Proxys
        Dictionary<string, MetadataServerInterface> _metadataServersProxys;

        //DataServers URLS
        Dictionary<string, string> _dataServersURLS;
        //DataServers Proxys
        Dictionary<string, DataServerInterface> _dataServersProxys;

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

        //For file creation
        string _fileUniqueName;
        int _fileUniqueID;

        public MetadataServerRemoting(string metadataServerName, int metadataServerPort)
        {
            //Inicializations
            _metadataServerName = metadataServerName;
            _metadataServerPort = metadataServerPort;

            _isInFailMode = false;

            _metadataMasterActualProxy = null;


            _metadataServersProxys = new Dictionary<string, MetadataServerInterface>();

            _dataServersURLS = new Dictionary<string, string>();
            _dataServersProxys = new Dictionary<string, DataServerInterface>();

            _filesMetadata = new Dictionary<string, FileMetadata>();

            _dataServersNumberOfFiles = new Dictionary<string, int>();
            _dataServersAssociatedFilenames = new Dictionary<string, string>();
            _localFilesDataServers = new Dictionary<string, List<string>>();

            _filesWithMissingDataServers = new Dictionary<string, int>();


            _fileUniqueName = "LOCAL-DATASERVER-FILENAME-";
            _fileUniqueID = 1;           
        }

        //Auxiliar Functions

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


        //Main Operations

        //Create Operation
        public FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            LogPrint("Create Operation");
            LogPrint("\t Filename: " + filename + " Number of Data Servers: " + numberOfDataServers +
                " ReadQuorum: " + readQuorum + " Write Quorum: " + writeQuorum);

            if (!_isMaster)
            {
                FileMetadata fileToReturn = _metadataMasterActualProxy.create(filename, numberOfDataServers, readQuorum, writeQuorum);
                LogPrint("Sucessfull!");
                return fileToReturn;
            }

            lock (this)
            {
                //Verification if the file with this filename already exists, if it does, returns that file
                if (_filesMetadata.ContainsKey(filename))
                {
                    return _filesMetadata[filename];
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
                    _dataServersProxys[dataServerName].createFilename(associatedFilename);
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


                //Update replicas
                if (_isMaster)
                    updateMetadataServerReplicas();

                LogPrint("Sucessfull!");

                return newFileMetadata;
            }
            
        }

        // Open Operation
        public FileMetadata open(string filename)
        {
            LogPrint("Open Operation");
            LogPrint("\t Filename: " + filename);

            if (!_isMaster)
            {
                FileMetadata openToReturn = _metadataMasterActualProxy.open(filename);
                LogPrint("Sucessfull!");
                return openToReturn;
            }

            lock (this)
            {
                LogPrint("Sucessfull!");
                if (_filesMetadata.ContainsKey(filename))
                {
                    return _filesMetadata[filename];

                }
                else throw new FileDoesntExistException(filename);
            }
        }

        // Close Operation
        public void close(string filename)
        {

            LogPrint("Open Operation");
            LogPrint("\t Filename: " + filename);

            if (!_isMaster)
            {
                _metadataMasterActualProxy.close(filename);
                LogPrint("Sucessfull!");
                return;
            }

            LogPrint("Sucessfull!");
            return;
        }

        // Delete Operation
        public void delete(string filename)
        {
            LogPrint("Delete Operation");
            LogPrint("\t Filename: " + filename);

            if (!_isMaster)
            {
                _metadataMasterActualProxy.delete(filename);
                LogPrint("Sucessfull!");
                return;
            }

            lock (this)
            {
                // If filename exists, delete filemetada stored from filename
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
                        _dataServersProxys[dataServerNameWithFile].deleteFilename(local_dataserver_filename_associated);
                        _dataServersNumberOfFiles[dataServerNameWithFile] = _dataServersNumberOfFiles[dataServerNameWithFile] - 1;
                    }

                    _localFilesDataServers.Remove(local_dataserver_filename_associated);
                }

                //If exists an association saying that there are still DataServers to add to this file, delete it
                foreach (string fileWithMissingDataServersName in _filesWithMissingDataServers.Keys)
                    if (fileWithMissingDataServersName == filename)
                    {
                        _filesWithMissingDataServers.Remove(fileWithMissingDataServersName);
                        break;
                    }

                //Update replicas
                if (_isMaster)
                    updateMetadataServerReplicas();

                LogPrint("Sucessfull!");
            }          

        }


        //Fail Operation
        public void fail()
        {
            LogPrint("Fail Operation");

            lock (this)
            {
                LogPrint("Sucessfull!");
                _isInFailMode = true;
            }
        }

        //Recover Operation
        public void recover()
        {
            LogPrint("Recover Operation");

            lock (this)
            {
                LogPrint("Sucessfull!");
                _isInFailMode = false;
            }
        }

        //Dump Operation
        //This operation doesnt only prints if this metadata is the master and the files metadata saved in this process, but also sends it, 
        //in a text form way, to the process caller (Puppet Master) for local printing
        public string dump()
        {
            LogPrint("Dump Operation");

            string dumpInfo = "IS MASTER? " + _isMaster + "\r\n";

            LogPrint(dumpInfo);

            int fileNumber = 1;
            string toDump = null;
            foreach (string filename in _filesMetadata.Keys)
            {
                dumpInfo += "File stored number " + fileNumber++ + " information:" + "\r\n";
                toDump = _filesMetadata[filename].ToString();
                LogPrint(toDump);
                dumpInfo += toDump + "\r\n";
            }

            LogPrint("Sucessfull!");

            return dumpInfo;
        }

        //Other Operations

        //RegisterDataServer Operation
        public void registerDataServer(string dataServerName, string dataServerURL)
        {
            LogPrint("Register Data Server Operation");

            if (!_isMaster)
            {
                _metadataMasterActualProxy.registerDataServer(dataServerName, dataServerURL);
                LogPrint("Sucessfull!");
                return;
            }

            lock (this)
            {
                DataServerInterface dataServerProxy = (DataServerInterface)Activator.GetObject(
                                            typeof(DataServerInterface),
                                            dataServerURL);

                //Saving DataServer stuff in MetaDataServer context
                _dataServersURLS.Add(dataServerName, dataServerURL);
                _dataServersProxys.Add(dataServerName, dataServerProxy);
                _dataServersNumberOfFiles.Add(dataServerName, 0);

                //LogPrint("COUNT: " + _filesWithMissingDataServers.Count);
                //for each file with missing data servers, associate this data server
                foreach (string fileWithMissingDataServerName in _filesWithMissingDataServers.Keys)
                {
                    _dataServersProxys[dataServerName].createFilename(_dataServersAssociatedFilenames[fileWithMissingDataServerName]);
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

                //Update replicas
                if (_isMaster)
                    updateMetadataServerReplicas();

                LogPrint("Sucessfull!");
            }            

        }

        public void configureMaster()
        {
            lock (this)
            {
                Tuple<string, MetadataServerInterface> aliveMetadataServerMaster = isAnyMasterAlive();

                //Someone is alive (the master)
                if (aliveMetadataServerMaster != null)
                {
                    _isMaster = false;

                    _metadataMasterActualProxy = aliveMetadataServerMaster.Item2;

                    _metadataMasterActualProxy.iWasBorn(_metadataServerName, _metadataServerPort);

                }
                else
                {
                    _isMaster = true;

                    _metadataMasterActualProxy = (MetadataServerInterface)Activator.GetObject(typeof(MetadataServerInterface),
                                                                                              "tcp://localhost:" + _metadataServerPort + "/" + _metadataServerName);

                    _metadataServersProxys.Add(_metadataServerName, _metadataMasterActualProxy);
                }
            }
            
        }


        private Tuple<string, MetadataServerInterface> isAnyMasterAlive()
        {
            TextReader metadataServersPorts = new StreamReader(@"..\..\..\MetadataServer\bin\Debug\MetadataServersPorts.txt");
            string metadataServersPortsLine;
            string[] metadataServersPortsLineWords;

            string metadataServerName = null;
            string metadataServerPort = null;
            string metadataServerURL = null;
            while ((metadataServersPortsLine = metadataServersPorts.ReadLine()) != null)
            {
                metadataServersPortsLineWords = metadataServersPortsLine.Split(' ');
                metadataServerName = metadataServersPortsLineWords[0];
                metadataServerPort = metadataServersPortsLineWords[1];
                metadataServerURL = "tcp://localhost:" + metadataServerPort + "/" + metadataServerName;

                if (metadataServerName != _metadataServerName)
                {
                    try
                    {
                        MetadataServerInterface metadataServerProxy = (MetadataServerInterface)Activator.GetObject(
                                                              typeof(MetadataServerInterface),
                                                              metadataServerURL);
                        if (metadataServerProxy.isMaster())
                        {
                            metadataServersPorts.Close();
                            return new Tuple<string, MetadataServerInterface>(metadataServerName, metadataServerProxy);
                        }
                        
                    }
                    catch (Exception e)
                    {
                        LogPrint("NOT ONLINE METADATA WITH NAME " + metadataServerName + " " + e.Message);
                    }
                }
                        
            }
            metadataServersPorts.Close();

            return null;
            
         }



        //iWasBorn Operation
        public void iWasBorn(string metadataServerName, int metadataServerPort)
        {
            //Save metadata info
            string metadataServerURL = "tcp://localhost:" + metadataServerPort + "/" + metadataServerName;

            MetadataServerInterface metadataServerProxy = (MetadataServerInterface)Activator.GetObject(
                                                              typeof(MetadataServerInterface),
                                                              metadataServerURL);

            _metadataServersProxys.Add(metadataServerName, metadataServerProxy);

            
            foreach (string metadataServer in _metadataServersProxys.Keys)
            {
                if (metadataServer != _metadataServerName)
                    _metadataServersProxys[metadataServer].sendUpdateInformation(getMetadataServerInformation());
            }

        }

        private void updateMetadataServerReplicas()
        {
            MetadataServerInformation toSend = getMetadataServerInformation();

            foreach (string metadataServerName in _metadataServersProxys.Keys)
            {
                if(metadataServerName != _metadataServerName)
                    _metadataServersProxys[metadataServerName].sendUpdateInformation(getMetadataServerInformation());
            }

        }

        //SendUpdateInformation Operation
        public void sendUpdateInformation(MetadataServerInformation infoToUpdate)
        {
            this.updateMetadataServerInformation(infoToUpdate);
        }

        private MetadataServerInformation getMetadataServerInformation()
        {
            return new MetadataServerInformation(null, _metadataServersProxys, _dataServersURLS, _dataServersProxys, _filesMetadata,
                                                 _dataServersNumberOfFiles, _dataServersAssociatedFilenames, _localFilesDataServers,
                                                 _filesWithMissingDataServers, _numberOfClientsUsingExistingFile, _fileUniqueID);
        }

        private void updateMetadataServerInformation(MetadataServerInformation infoToUpdate)
        {
            /*Falta o primeiro argumento*/
            _metadataServersProxys = infoToUpdate.MetadataProxies;            
            _dataServersURLS = infoToUpdate.DataServersURLS;
            _dataServersProxys = infoToUpdate.DataServersProxies;
            _filesMetadata = infoToUpdate.FilesMetadata;
            _dataServersNumberOfFiles = infoToUpdate.DataServersNumberOfFiles;
            _dataServersAssociatedFilenames = infoToUpdate.DataServersAssociatedFilenames;
            _localFilesDataServers = infoToUpdate.LocalFilesDataServers;           
            _filesWithMissingDataServers = infoToUpdate.FilesWithMissingDataServers;
            _numberOfClientsUsingExistingFile = infoToUpdate.NumberOfClientsUsingExistingFile;
            _fileUniqueID = infoToUpdate.FileUniqueID;
            
            
        }

        //takeControl Operation
        public void takeControl()
        {
            _isMaster = true;
        }

        //ping Operation - to check if a Metadata Server is alive
        public void ping()
        {
        }

        public bool isMaster()
        {
            return _isMaster;
        }

        //Never allow lease to expire
        public override object InitializeLifetimeService()
        {

            return null;

        }

    }
}
