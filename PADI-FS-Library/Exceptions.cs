using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADI_FS_Library
{
    [Serializable]
    public class FileDoesntExistException : ApplicationException
    {
        private string _filename;

        public FileDoesntExistException(string filename)
        {
            _filename = filename;
        }

        public string Filename
        {
            get { return _filename; }
        }

        public FileDoesntExistException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            _filename = info.GetString("_filename");            
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_filename", _filename);
        }
    }
}
