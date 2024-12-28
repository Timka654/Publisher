using Newtonsoft.Json;
using NSL.Cipher.RSA;
using NSL.Logger;
using NSL.Utils;
using NSL.Utils.CommandLine;
using ServerPublisher.Server.Dev.Test.Utils;
using ServerPublisher.Server.Utils;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;

namespace ServerPublisher.Server.Info
{
    public enum UserInfoTypeEnum
    {
        PUBKEY,
        PRIVKEY
    }

    public class UserInfo : BasicUserInfo, IDisposable
    {
        public UserInfoTypeEnum Type { get; set; }

        public string FileName { get; private set; }

        public RSACipher Cipher { get; set; }

        public DateTime UpdateTime { get; set; }

        public event Action OnRemoved = () => { };

        public event Action OnUpdate = () => { };

        public UserInfo(string fileName, UserInfoTypeEnum type = UserInfoTypeEnum.PRIVKEY)
        {
            Type = type;
            FileName = fileName;
            Reload(JsonConvert.DeserializeObject<BasicUserInfo>(File.ReadAllText(fileName)));
        }

        /// <summary>
        /// Only for json deserialize
        /// </summary>
        public UserInfo()
        {

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

        public void ProducePublicKey(string dir)
        {
            DirectoryUtils.CreateNoExistsDirectory(dir);

            File.WriteAllText(Path.Combine(dir, $"{Name}_{Id}.pubuk").GetNormalizedPath(), JsonConvert.SerializeObject(new
            {
                Id,
                Name,
                RSAPublicKey
            }, JsonUtils.JsonSettings));
        }

        public void ProducePrivateKey(string dir)
        {
            DirectoryUtils.CreateNoExistsDirectory(dir);

            File.WriteAllText(Path.Combine(dir, $"{Name}_{Id}.priuk").GetNormalizedPath(), JsonConvert.SerializeObject(new
            {
                Id,
                Name,
                RSAPublicKey,
                RSAPrivateKey
            }, JsonUtils.JsonSettings));
        }


        internal void Reload(BasicUserInfo userInfo)
        {
            Id = userInfo.Id;
            Name = userInfo.Name;
            RSAPublicKey = userInfo.RSAPublicKey;

            if (RSAPrivateKey != userInfo.RSAPrivateKey)
            {
                RSAPrivateKey = userInfo.RSAPrivateKey;

                Cipher?.Dispose();

                Cipher = new RSACipher();

                Cipher.LoadXml(RSAPrivateKey);
            }

            UpdateTime = DateTime.UtcNow;

            OnUpdate();
        }

        bool disposed = false;

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            OnRemoved();
        }
    }
}
