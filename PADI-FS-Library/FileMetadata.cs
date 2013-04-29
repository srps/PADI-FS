using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;

namespace PADI_FS_Library
{
    [Serializable]
    public class FileMetadata
    {
        string _filename;
        int _numberOfStoringFileDataServers;
        int _readQuorum;
        int _writeQuorum;
        List<Tuple<string, string>> _fileDataServersLocations;


        public FileMetadata(string filename, int numberOfStoringFileDataServers, int readQuorum, int writeQuorum, List<Tuple<string, string>> fileDataServersLocations)
        { 
            _filename = filename;
            _numberOfStoringFileDataServers = numberOfStoringFileDataServers;
            _readQuorum = readQuorum;
            _writeQuorum = writeQuorum;
            _fileDataServersLocations = fileDataServersLocations;

        }

        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public int NumberOfStoringFileDataServers
        {
            get { return _numberOfStoringFileDataServers; }
            set { _numberOfStoringFileDataServers = value; }
        }

        public int ReadQuorum
        {
            get { return _readQuorum; }
            set { _readQuorum = value; }
        }

        public int WriteQuorum
        {
            get { return _writeQuorum; }
            set { _writeQuorum = value; }
        }

        public List<Tuple<string, string>> FileDataServersLocations
        {
            get { return _fileDataServersLocations; }
            set { _fileDataServersLocations = value; }
        }

       
        public override string ToString()
        {
            string toReturn = _filename + " || " + _numberOfStoringFileDataServers + " || " + _readQuorum + " || " + _writeQuorum + "\r\n";

            foreach (Tuple<string, string> fileLocations in _fileDataServersLocations)
            {
                toReturn += "(" + fileLocations.Item1 + "," + fileLocations.Item2 + ") ";
            }

            return toReturn;
        }

    }
}
