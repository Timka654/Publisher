using NSL.Cipher.RSA;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Managers.Storages;
using System.Text;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(1)]
    public class UserManager : UserStorage
    {
        public static UserManager Instance { get; private set; }

        public UserManager() : base()
        {
            Instance = this;
        }

        public UserInfo TrySignUser(string user_id, byte[] encoded)
        {
            var user = base.GetUser(user_id);

            if (user != null)
            {
                var cipher = new RSACipher();

                cipher.LoadXml(user.RSAPrivateKey);

                byte[] data = user.Cipher.Decode(encoded, 0, encoded.Length);

                if (Encoding.ASCII.GetString(data) == user_id)
                    return user;
            }

            return null;
        }

        public bool ValidateUser(string user_id, byte[] encoded, out UserInfo user)
        {
            user = base.GetUser(user_id);

            if (user != null)
            {
                var cipher = new RSACipher();

                cipher.LoadXml(user.RSAPrivateKey);

                byte[] data = user.Cipher.Decode(encoded, 0, encoded.Length);

                if (Encoding.ASCII.GetString(data) == user.Name)
                    return true;
            }

            return false;
        }
    }
}
