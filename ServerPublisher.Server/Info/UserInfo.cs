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

        /// <summary>
        /// Only for json deserialize
        /// </summary>
        public UserInfo()
        {

        }

        public static UserInfo CreateUser(CommandLineArgs args)
        {
            return CreateUser(args["name"]);
        }

        public static UserInfo CreateUser(string name)
        {
            var u = new UserInfo();

            PublisherServer.ServerLogger.AppendInfo($"user {name} creating");

            u.Id = Guid.NewGuid().ToString();
            u.Name = name;

            u.Cipher = new RSACipher();

            u.RSAPublicKey = u.Cipher.GetPublicKey();
            u.RSAPrivateKey = u.Cipher.GetPrivateKey();

            return u;
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
