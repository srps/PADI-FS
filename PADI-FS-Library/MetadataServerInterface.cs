using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADI_FS_Library
{
    public interface MetadataServerInterface
    {   
        //Main Operations
        FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum);
        FileMetadata open(string filename);
        void close(string filename);
        void delete(string filename);

        /*void fail();*/
        void recover();

        string dump();

        //Other Operations
        void registerDataServer(string dataServerName, string dataServerURL);
        void iWasBorn(string metadataServerName, int metadataServerPort);
        void sendUpdateInformation(MetadataServerInformation infoToUpdate);
        void takeControl();
        void ping();
        bool isMaster();
    }
}
