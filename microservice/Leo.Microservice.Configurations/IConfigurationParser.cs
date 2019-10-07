using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Leo.Microservice.Configurations
{
    public interface IConfigurationParser
    {
        IDictionary<string, string> Parse(Stream input, string initialContext);
    }
}
