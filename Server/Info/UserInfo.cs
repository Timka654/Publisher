using Cipher.RSA;
using Newtonsoft.Json;
using Publisher.Server.Configuration;
using Publisher.Server.Network;
using Publisher.Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Info
{
    public class UserInfo : Publisher.Basic.BasicUserInfo
    {
        public string FileName { get; private set; }

        public RSACipher Cipher { get; set; }

        public ProjectInfo CurrentProject { get; set; }

        public NetworkClient CurrentNetwork { get; set; }

        public UserInfo(string fileName)
        {
            FileName = fileName;
            Reload(JsonConvert.DeserializeObject<Publisher.Basic.BasicUserInfo>(File.ReadAllText(fileName)));
        }

        public UserInfo(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("user creating");

            Id = Guid.NewGuid().ToString();
            Name = args["name"];

            Cipher = new RSACipher();

            RSAPublicKey = Cipher.GetPublicKey();
            PSAPrivateKey = Cipher.GetPrivateKey();
        }
        internal void Reload(Publisher.Basic.BasicUserInfo userInfo)
        {
            Id = userInfo.Id;
            Name = userInfo.Name;
            RSAPublicKey = userInfo.RSAPublicKey;
            PSAPrivateKey = userInfo.PSAPrivateKey;
        }
    }
}
