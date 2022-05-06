﻿using ServerPublisher.Server.Info;
using System.Collections.Concurrent;

namespace ServerPublisher.Server.Managers.Storages
{
    public class SessionsStorage
    {
        protected ConcurrentDictionary<string, UserInfo> userStorage;

        protected SessionsStorage()
        {
            userStorage = new ConcurrentDictionary<string, UserInfo>();
        }

        public bool AddUser(UserInfo user)
        {
            return userStorage.TryAdd(user.Id, user);
        }

        public bool RemoveUser(UserInfo user)
        {
            return RemoveUser(user.Id);
        }

        public bool RemoveUser(string userId)
        {
            return userStorage.TryRemove(userId, out var dummy);
        }

        public UserInfo GetUser(string userId)
        {
            userStorage.TryGetValue(userId, out var user);

            return user;
        }
    }
}