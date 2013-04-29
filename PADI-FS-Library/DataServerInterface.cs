using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADI_FS_Library
{
    public interface DataServerInterface
    {
        //Main Operations
        File read(string filename, string semantics);
        void write(string filename, int versionNumber, string contentToWrite);

        /*void freeze();*/
        void unfreeze();
        /*void fail();
        void recover();*/

        string dump();
        //Other Operations
        void createFilename(string filename);
        void deleteFilename(string filename);
    }
}
