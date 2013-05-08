using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADI_FS_Library
{
    [Serializable]
    public class MetadataServerInformation
    {
        //Alive Servers
        private Dictionary<string, bool> _aliveServers;

        // Other Metadata Server URLs
        private Dictionary<string, MetadataServerInterface> _metadataProxies;

        //DataServers URLS
        private Dictionary<string, string> _dataServersURLS;
        //DataServers Proxies
        private Dictionary<string, DataServerInterface> _dataServersProxies;

        //Metadata about files being created
        private Dictionary<string, FileMetadata> _filesMetadata;

        //Number of files existing in each DataServer
        private Dictionary<string, int> _dataServersNumberOfFiles;

        //Name associated to files created in Data Servers
        private Dictionary<string, string> _dataServersAssociatedFilenames;

        //LocalFilenames and associated DataServers
        private Dictionary<string, List<string>> _localFilesDataServers;

        //List of tasks which will be used to associate missing DataServers to files
        private Dictionary<string, int> _filesWithMissingDataServers;

        //Association between existing files and number of clients that are using them
        private Dictionary<string, int> _numberOfClientsUsingExistingFile; /*Nao esta a ser usado*/


        private int _fileUniqueID;


        public MetadataServerInformation(Dictionary<string, bool> aliveServers,
            Dictionary<string, MetadataServerInterface> metadataProxies,
            Dictionary<string, string> dataServersURLS,
            Dictionary<string, DataServerInterface> dataServersProxies,
            Dictionary<string, FileMetadata> filesMetadata,
            Dictionary<string, int> dataServersNumberOfFiles,
            Dictionary<string, string> dataServersAssociatedFilenames,
            Dictionary<string, List<string>> localFilesDataServers,
            Dictionary<string, int> filesWithMissingDataServers,
            Dictionary<string, int> numberOfClientsUsingExistingFile,
            int fileUniqueID)
        {
            _aliveServers = aliveServers;
            _metadataProxies = metadataProxies;
            _dataServersURLS = dataServersURLS;
            _filesMetadata = filesMetadata;
            _dataServersProxies = dataServersProxies;
            _dataServersNumberOfFiles = dataServersNumberOfFiles;
            _dataServersAssociatedFilenames = dataServersAssociatedFilenames;
            _localFilesDataServers = localFilesDataServers;
            _filesWithMissingDataServers = filesWithMissingDataServers;
            _numberOfClientsUsingExistingFile = numberOfClientsUsingExistingFile;
            _fileUniqueID = fileUniqueID;
        }

        public Dictionary<string, bool> AliveServers
        {
            get
            {
                return _aliveServers;
            }
            set
            {
                _aliveServers = value;
            }
        }

        public Dictionary<string, MetadataServerInterface> MetadataProxies
        {
            get
            {
                return _metadataProxies;
            }
            set
            {
                _metadataProxies = value;
            }
        }

        public Dictionary<string, string> DataServersURLS
        {
            get
            {
                return _dataServersURLS;
            }
            set
            {
                _dataServersURLS = value;
            }
        }

        public Dictionary<string, DataServerInterface> DataServersProxies
        {
            get
            {
                return _dataServersProxies;
            }
            set
            {
                _dataServersProxies = value;
            }
        }

        public Dictionary<string, FileMetadata> FilesMetadata
        {
            get
            {
                return _filesMetadata;
            }
            set
            {
                _filesMetadata = value;
            }
        }

        public Dictionary<string, int> DataServersNumberOfFiles
        {
            get
            {
                return _dataServersNumberOfFiles;
            }
            set
            {
                _dataServersNumberOfFiles = value;
            }
        }

        public Dictionary<string, string> DataServersAssociatedFilenames
        {
            get
            {
                return _dataServersAssociatedFilenames;
            }
            set
            {
                _dataServersAssociatedFilenames = value;
            }
        }

        public Dictionary<string, List<string>> LocalFilesDataServers
        {
            get
            {
                return _localFilesDataServers;
            }
            set
            {
                _localFilesDataServers = value;
            }
        }

        public Dictionary<string, int> FilesWithMissingDataServers
        {
            get
            {
                return _filesWithMissingDataServers;
            }
            set
            {
                _filesWithMissingDataServers = value;
            }
        }

        public Dictionary<string, int> NumberOfClientsUsingExistingFile
        {
            get
            {
                return _numberOfClientsUsingExistingFile;
            }
            set
            {
                _numberOfClientsUsingExistingFile = value;
            }
        }

        public int FileUniqueID
        {
            get
            {
                return _fileUniqueID;
            }
            set
            {
                _fileUniqueID = value;
            }
        }

    }



}
