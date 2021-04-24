using Cipher.RSA;
using Newtonsoft.Json;
using Publisher.Basic;
using Publisher.Server.Network;
using System;
using System.IO;

namespace Publisher.Server.Info
{
    public class UserInfo : Publisher.Basic.BasicUserInfo
    {
        public string FileName { get; private set; }

        public RSACipher Cipher { get; set; }

        public ProjectInfo CurrentProject { get; set; }

        public PublisherNetworkClient CurrentNetwork { get; set; }

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
