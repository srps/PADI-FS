﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADI_FS_Library
{
    public interface ClientInterface
    {
        //Main Operations
        FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum);
        FileMetadata open(string filename);
        void close(string filename);
        void delete(string filename);

        string read(int fileRegister, string semantics, int stringRegister);
        void write(int fileRegister, string contentToWrite);
        void write(int fileRegister, int stringRegister);
        void copy(int fileRegister1, string semantics, int fileRegister2, string salt);

        string dump();

        //Other Operations

    }
}
