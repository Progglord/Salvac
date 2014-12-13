using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac
{
    [Serializable]
    public class NoWorldException : Exception
    {
        public NoWorldException() :
            base("There is no world model database connected.")
        { }
        public NoWorldException(string message) : base(message) { }
        public NoWorldException(string message, Exception inner) : base(message, inner) { }
        protected NoWorldException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
