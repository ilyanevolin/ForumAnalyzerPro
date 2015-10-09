using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace ForumAnalyzerPro.Common
{
    [Serializable]
    // [ProtoBuf.ProtoContract(SkipConstructor = true)]
    public class Proxy
    {
        protected Proxy()
        { }
        // [ProtoBuf.ProtoMember(1)]
        private string ip;
        // [ProtoBuf.ProtoMember(2)]
        private int port;
        // [ProtoBuf.ProtoMember(3)]
        private string user;
        // [ProtoBuf.ProtoMember(4)]
        private string pass;

        public Proxy(string Ip, int Port, string User = "", string Pass = "")
        {
            this.Ip = Ip;
            this.Port = Port;
            this.User = User;
            this.Pass = Pass;
        }

        // [ProtoBuf.ProtoIgnore]
        public WebProxy WebProxy
        {
            get
            {
                try
                {
                    WebProxy proxy = new WebProxy(Ip, Port);
                    if (User != "" && Pass != "")
                    {
                        NetworkCredential c = new NetworkCredential(User, Pass);
                        proxy.Credentials = c;
                    }
                    return proxy;
                }
                catch (Exception ex)
                {
                    string msg = "P29" + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logging.Log("user", "account action: ", msg);
                    
                    return null;
                }

            }
        }


        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        public string User
        {
            get { return user; }
            set { user = value; }
        }
        public string Pass
        {
            get { return pass; }
            set { pass = value; }
        }


    }
}
