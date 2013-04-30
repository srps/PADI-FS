using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PADI_FS_Library;



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
        Dictionary<string, File> _filesData;

        public DataServerRemoting(string dataServerName, int dataServerPort)
        {
            // Inicializations
            _dataServerName = dataServerName;
            _dataServerPort = dataServerPort;

            _isInFreezeMode = false;
            _isInFailMode = false;

            _filesData = new Dictionary<string, File>();
        }

        //Auxiliar Functions

        private void LogPrint(string toPrint)
        {
            Console.WriteLine(toPrint);
        }


        // Main Operations

        // Read Operation
        public File read(string filename, string semantics) //semantics ... QUAL O USO DISTO????
        {

            LogPrint("Read Operation");
            LogPrint("\t Filename: " + filename + " Semantics: " + semantics);

            if (_filesData.ContainsKey(filename))
            {
                LogPrint("Successful!");

                return _filesData[filename];
            }
            else
            {
                throw new Exception();
            }

        }

        //Write Operation
        public void write(string filename, int versionNumber, string contentToWrite)
        {

            LogPrint("Write Operation");
            LogPrint("\t Filename: " + filename + " Version Number: " + versionNumber + " Contents: " + contentToWrite);

            if (_filesData.ContainsKey(filename))
            {
                _filesData[filename].VersionNumber = versionNumber;
                _filesData[filename].FileContents = contentToWrite;
            }
            else
            {
                throw new Exception();
            }

            LogPrint("Successful!");

            return;
        }

        //Unfreeze Operation
        public void unfreeze()
        {
            _isInFreezeMode = false;
        }

        //Dump Operation
        //This operation doesnt only prints the files saved in this process, but also sends it, in a text form way, to the
        //process caller (Puppet Master) for local printing
        public string dump()
        {
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

            return dumpInfo;
        }





        //Other Operations
        public void createFilename(string filename)
        {
            _filesData.Add(filename, new File(filename, "", 0));
        }

        public void deleteFilename(string filename)
        {
            if(_filesData.ContainsKey(filename))
                _filesData.Remove(filename);
        }
    }
}
