using System.Collections.Generic;
using MessageSystem.Models;
using MessageSystem.Repositories;

namespace MessageSystem.Services
{
    public class UserService
    {
        private readonly UserRepository _repo;

        public UserService(UserRepository repo)
        {
            _repo = repo;
        }

        public MS_User RegisterUser(string userName, string password, string name)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            return _repo.CreateUser(userName, password, name);
        }

        public bool ValidateUser(MS_User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName)) return false;
            if (string.IsNullOrWhiteSpace(user.Password)) return false;
            if (string.IsNullOrWhiteSpace(user.Name)) return false;
            // Check for unique username
            var existing = _repo.GetUserByName(user.UserName);
            if (existing != null && existing.UserId != user.UserId) return false;
            return true;
        }

        public void ImportUsers(List<MS_User> users)
        {
            foreach (var user in users)
            {
                if (!ValidateUser(user))
                    throw new InvalidOperationException($"Invalid user: {user.UserName}");
            }
            _repo.ImportUsers(users);
        }
    }
}