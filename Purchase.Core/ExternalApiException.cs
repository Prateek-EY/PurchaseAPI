using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purchase.Core
{
        public class ExternalApiException : Exception
        {
            public int StatusCode { get; }

            public ExternalApiException(string message, int statusCode)
                : base(message)
            {
                StatusCode = statusCode;
            }
        }
  
}
