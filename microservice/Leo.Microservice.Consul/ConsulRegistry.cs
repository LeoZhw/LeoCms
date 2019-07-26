using System;
using System.Collections.Generic;
using System.Net;
using Leo.Microservice.Platform;

namespace Leo.Microservice.Consul
{
    public class ConsulRegistry : IRegistry
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Thread thread;
        private bool terminal = false;
        private string id;
        private Dictionary<string, Dictionary<string, string>> provider = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> registered = new Dictionary<string, Dictionary<string, string>>();
        private IZookClient client;
        
        public ConsulRegistry(int protocolPort)
        {
            var configInfo = new ConfigInfo(null);

            id = Guid.NewGuid().ToString();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var host in hostEntry.AddressList)
            {
                if (host.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                if (IPAddress.IsLoopback(host) == false)
                {
                    string url = string.Format("gtcp://{0}:{1}", host.ToString(), protocolPort);
                    if (provider.ContainsKey(url) == false)
                    {
                        provider[url] = new Dictionary<string, string>();
                    }
                }
            }

        }

        public void Reigstered(string serviceInterface, string service)
        {
            foreach (var key in provider.Keys)
            {
                var dir = provider[key];
                dir[serviceInterface] = service;
            }
        }

        public Dictionary<string, Dictionary<string, string>> GetRegistered()
        {
            return registered;
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            var data = client.GetData("/hong2", false);
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, data);
            //byte[] buffer = stream.ToArray();
            //listener.EndReceive(ar, ref remoteEP);

            QueueMessage msg = (QueueMessage)formatter.Deserialize(stream);
            //logger.InfoFormat("receive multicast message '{0}' from {1}", msg.Action, remoteEP.Address.ToString());

            if (msg.Action == "Register")
            {
                string url = (string)msg[0];
                Dictionary<string, string> dir = (Dictionary<string, string>)msg[1];

                if (msg.ObjectCount > 2)
                {
                    string registryId = msg[2] as string;
                    if (registryId == id)
                    {
                        //goto label1;
                    }
                }

                lock (registered)
                {
                    logger.InfoFormat("receive service register message: Time={0} url={1} ServcieCount={2}", msg.Time, url, dir.Count);

                    if (registered.ContainsKey(url) == true)
                    {
                        registered.Remove(url);
                    }

                    registered[url] = new Dictionary<string, string>();

                    foreach (var item in dir)
                    {
                        registered[url][item.Key] = item.Value;
                    }
                }
            }
            else if (msg.Action == "Unregister")
            {
                string url = (string)msg[0];
                Dictionary<string, string> dir = (Dictionary<string, string>)msg[1];

                if (msg.ObjectCount > 2)
                {
                    string registryId = msg[2] as string;
                    if (registryId == id)
                    {
                        //goto label1;
                    }
                }

                lock (registered)
                {
                    logger.InfoFormat("receive service unregister message: Time={0} ServcieCount={1}", msg.Time, dir.Count);

                    if (registered.ContainsKey(url) == true)
                    {
                        registered.Remove(url);
                    }
                }
            }
            else if (msg.Action == "Request")
            {
                //logger.InfoFormat("receive service request message from {0}", remoteEP.Address.ToString());

                foreach (var url in provider.Keys)
                {
                    if (provider[url].Count > 0)
                    {
                        QueueMessage message = new QueueMessage();

                        message.Action = "Register";
                        message.Time = DateTime.Now;
                        message.Add(url);
                        message.Add(provider[url]);
                        message.Add(id);

                        Send(message);
                    }
                }
            }
            else
            {
                //logger.WarnFormat("unknow arrival message from {0}", remoteEP.Address.ToString());
            }

            //label1:
            //listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        public void Register()
        {
            foreach (var key in provider.Keys)
            {
                if (provider[key].Count > 0)
                {
                    Register(key, provider[key]);
                }
            }
        }

        public void Register(string url, Dictionary<string, string> dir)
        {
            QueueMessage msg = new QueueMessage();

            msg.Action = "Register";
            msg.Time = DateTime.Now;
            msg.Add(url);
            msg.Add(dir);
            msg.Add(id);

            Send(msg);
        }

        public void Unregister()
        {
            foreach (var url in provider.Keys)
            {
                QueueMessage msg = new QueueMessage();

                msg.Action = "Unregister";
                msg.Time = DateTime.Now;
                msg.Add(url);
                msg.Add(provider[url]);
                msg.Add(id);

                Send(msg);
            }
        }

        public void Request()
        {
            QueueMessage msg = new QueueMessage();

            msg.Time = DateTime.Now;
            msg.Action = "Request";

            Send(msg);
        }

        public void Send(QueueMessage msg)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, msg);
            byte[] buffer = stream.ToArray();
            NodeCreator.TryCreate(client, new NodeInfo("/hong2", buffer, IDs.OPEN_ACL_UNSAFE, CreateModes.Ephemeral));
        }

        public string Lookup(string serviceInterface)
        {
            string url = null;

            lock (registered)
            {
                foreach (var key in registered.Keys)
                {
                    if (registered[key].ContainsKey(serviceInterface))
                    {
                        url = key + "/ServiceFactory";
                        break;
                    }
                }
            }

            return url;
        }

        public void Start()
        {
            NodeCreator.TryCreate(client, new NodeInfo("/hong2", null, IDs.OPEN_ACL_UNSAFE, CreateModes.Persistent));
            //
            terminal = false;
            thread = new Thread(new ThreadStart(Run));
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Priority = ThreadPriority.Normal;

            thread.Start();
        }

        public void Stop()
        {
            if (thread != null)
            {
                terminal = true;
                thread.Join();
                thread = null;
            }
        }

        public void Run()
        {
            Register();
            Request();

            while (terminal == false)
            {
                Thread.Sleep(1000);
            }

            Unregister();
        }
    }
}
