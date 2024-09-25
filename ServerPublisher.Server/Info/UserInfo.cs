using Newtonsoft.Json;
using NSL.Cipher.RSA;
using NSL.Logger;
using NSL.Utils;
using ServerPublisher.Shared.Info;
using System;
using System.IO;

namespace ServerPublisher.Server.Info
{
    public class UserInfo : BasicUserInfo
    {
        public string FileName { get; private set; }

        public RSACipher Cipher { get; set; }

        //public ServerProjectInfo CurrentProject { get; set; }

        //public PublisherNetworkClient CurrentNetwork { get; set; }

        public UserInfo(string fileName)
        {
            FileName = fileName;
            Reload(JsonConvert.DeserializeObject<BasicUserInfo>(File.ReadAllText(fileName)));
        }

        public UserInfo(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("user creating");

            Id = Guid.NewGuid().ToString();
            Name = args["name"];

            Cipher = new RSACipher();

            RSAPublicKey = Cipher.GetPublicKey();
            RSAPrivateKey = Cipher.GetPrivateKey();
        }

        internal void Reload(BasicUserInfo userInfo)
        {
            Id = userInfo.Id;
            Name = userInfo.Name;
            RSAPublicKey = userInfo.RSAPublicKey;

            if (RSAPrivateKey != userInfo.RSAPrivateKey)
            {
                RSAPrivateKey = userInfo.RSAPrivateKey;

                Cipher = new RSACipher();

                Cipher.LoadXml(RSAPrivateKey);
            }
        }
    }
}
