﻿using Newtonsoft.Json;
using NSL.Cipher.RSA;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Shared;
using System;
using System.IO;

namespace ServerPublisher.Server.Info
{
    public class UserInfo : BasicUserInfo
    {
        public string FileName { get; private set; }

        public RSACipher Cipher { get; set; }

        public ServerProjectInfo CurrentProject { get; set; }

        public PublisherNetworkClient CurrentNetwork { get; set; }

        public UserInfo(string fileName)
        {
            FileName = fileName;
            Reload(JsonConvert.DeserializeObject<BasicUserInfo>(File.ReadAllText(fileName)));
        }

        public UserInfo(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("user creating");

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
            RSAPrivateKey = userInfo.RSAPrivateKey;
        }
    }
}