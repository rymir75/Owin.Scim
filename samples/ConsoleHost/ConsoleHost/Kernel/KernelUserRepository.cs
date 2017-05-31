using Nito.AsyncEx;

namespace ConsoleHost.Kernel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using NContext.Security.Cryptography;

    public class KernelUserRepository
    {
        private readonly ConcurrentDictionary<string, KernelUser> _Users;

        public KernelUserRepository()
        {
            _Users = new ConcurrentDictionary<string, KernelUser>();
        }

        public Task<KernelUser> CreateUser(KernelUser user)
        {
            user.Id = Guid.NewGuid().ToString("N");

            _Users.TryAdd(user.Id, user);

            return Task.FromResult(user);
        }

        public Task<KernelUser> GetUser(string userId)
        {
            // this should really return a deep-clone of the db record so it accurate represents in-memory
            // and domain code can't mutate the in-memory db record 
            return Task.FromResult(!_Users.ContainsKey(userId)
                ? null
                : _Users[userId]);
        }

        public Task<KernelUser> UpdateUser(KernelUser user)
        {
            if (!_Users.ContainsKey(user.Id))
                return Task.FromResult<KernelUser>(null);

            _Users[user.Id] = user;

            return Task.FromResult(user);
        }

        public Task DeleteUser(string userId)
        {
            if (_Users.ContainsKey(userId))
            {
                KernelUser userRecord;
                _Users.TryRemove(userId, out userRecord);
            }

            return TaskConstants.Completed;
        }

        public Task<bool> IsUserNameAvailable(string userName)
        {
            /* Before comparing or evaluating the uniqueness of a "userName" or 
               "password" attribute, service providers MUST use the preparation, 
               enforcement, and comparison of internationalized strings (PRECIS) 
               preparation and comparison rules described in Sections 3 and 4, 
               respectively, of [RFC7613], which is based on the PRECIS framework
               specification [RFC7564]. */

            var userNameBytes = Encoding.UTF8.GetBytes(userName);

            return Task.FromResult(
                _Users
                .Values
                .All(u => !CryptographyUtility.CompareBytes(Encoding.UTF8.GetBytes(u.UserName), userNameBytes)));
        }

        public Task<IEnumerable<KernelUser>> GetUsers()
        {
            return Task.FromResult<IEnumerable<KernelUser>>(_Users.Values);
        }
    }
}