using System;
using System.Collections.Generic;

namespace Leo.Microservice.Platform
{
    public interface IRegistry
    {
        void Reigstered(string serviceInterface, string service);
        void Register();
        void Unregister();
        Dictionary<string, Dictionary<string, string>> GetRegistered();
        string Lookup(string serviceInterface);
        void Start();
        void Stop();
    }
}
