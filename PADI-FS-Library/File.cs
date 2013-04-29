using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADI_FS_Library
{
    [Serializable]
    public class File
    {
        string _filename;
        int _versionNumber;
        string _fileContents;

        public File(string filename, string fileContents, int versionNumber)
        {
            _filename = filename;
            _fileContents = fileContents;
            _versionNumber = versionNumber;            
        }
        
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }


        public string FileContents
        {
            get { return _fileContents; }
            set { _fileContents = value; }
        }

        public int VersionNumber
        {
            get { return _versionNumber; }
            set { _versionNumber = value; }
        }

        public override string ToString()
        {
            if (_fileContents == "")
            {
                return "Filename: " + _filename + " || " + "Version Number: " + _versionNumber + " || " + "FileContents: empty file";
            }
            else
            {
                return "Filename: " + _filename + " || " + "Version Number: " + _versionNumber + " || " + "FileContents: " + _fileContents;
            }
            
        }
    }
}
