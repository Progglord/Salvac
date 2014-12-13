using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac.Data
{
    [Serializable]
    public class InvalidDatabaseException : Exception
    {
        public InvalidDatabaseException() { }
        public InvalidDatabaseException(string message) : base(message) { }
        public InvalidDatabaseException(string message, Exception inner) : base(message, inner) { }
        protected InvalidDatabaseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
