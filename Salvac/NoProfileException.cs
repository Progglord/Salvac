using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac
{
    [Serializable]
    public class NoProfileException : Exception
    {
        public NoProfileException() :
            base("There is no loaded profile.")
        { }
        public NoProfileException(string message) : base(message) { }
        public NoProfileException(string message, Exception inner) : base(message, inner) { }
        protected NoProfileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
