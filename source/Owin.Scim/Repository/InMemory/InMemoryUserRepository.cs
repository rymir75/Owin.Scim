﻿namespace Owin.Scim.Repository.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Extensions;

    using Model.Users;

    using NContext.Security.Cryptography;
    using Owin.Scim.Configuration;
    using Querying;

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly IGroupRepository _GroupRepository;

        private readonly ConcurrentDictionary<string, ScimUser> _Users;

        private readonly ScimServerConfiguration _scimServerConfiguration;

        public InMemoryUserRepository(ScimServerConfiguration scimServerConfiguration, IGroupRepository groupRepository)
        {
            _GroupRepository = groupRepository;
            _Users = new ConcurrentDictionary<string, ScimUser>();
            _scimServerConfiguration = scimServerConfiguration;
        }

        public Task<ScimUser> CreateUser(ScimUser user)
        {
            user.Id = Guid.NewGuid().ToString("N");

            var createdDate = DateTime.UtcNow;
            user.Meta.Created = createdDate;
            user.Meta.LastModified = createdDate;

            _Users.TryAdd(user.Id, user);

            return Task.FromResult(user);
        }

        public async Task<ScimUser> GetUser(string userId)
        {
            // return a deep-clone of the user object
            // since this is in-memory, we don't want any HTTP PATCH or other code to actually modify the
            // simulated database record stored in the list, unless done through create,update,delete
            var user = !_Users.ContainsKey(userId) 
                ? null 
                : _Users[userId].Copy();

            if (user != null)
                user.Groups = await _GroupRepository.GetGroupsUserBelongsTo(userId);

            return user;
        }

        public Task<ScimUser> UpdateUser(ScimUser user)
        {
            if (_Users.ContainsKey(user.Id))
            {
                user.Meta.LastModified = DateTime.UtcNow;
                _Users[user.Id] = user;
            }

            return Task.FromResult(user);
        }

        public Task DeleteUser(string userId)
        {
            if (_Users.ContainsKey(userId))
            {
                ScimUser userRecord;
                _Users.TryRemove(userId, out userRecord);
            }

            return Task.FromResult(true);
        }

        public Task<IEnumerable<ScimUser>> QueryUsers(ScimQueryOptions options)
        {
            var users = _Users.Values.AsEnumerable();
            if (options.Filter != null)
                users = users.Where(options.Filter.ToPredicate<ScimUser>(_scimServerConfiguration)).ToList();
            
            // TODO: (DG) sorting
            if (options.SortBy != null)
            {
            }

            if (options.StartIndex > 1)
                users = users.Skip(options.StartIndex);

            if (options.Count > 0)
                users = users.Take(options.Count);

            return Task.FromResult(users);
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

        public Task<bool> UserExists(string userId)
        {
            return Task.FromResult(_Users.ContainsKey(userId));
        }
    }
}