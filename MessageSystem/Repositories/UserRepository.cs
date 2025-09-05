using Microsoft.EntityFrameworkCore;
using MessageSystem.Models;

namespace MessageSystem.Repositories
{
    public class UserRepository
    {
        private readonly MessageSystemContext _context;

        public UserRepository(MessageSystemContext context)
        {
            _context = context;
        }

        #region retrieval methods
        public MS_User? GetUserById(int userId)
            => _context.Users.Find(userId);

        public MS_User? GetUserByName(string userName)
            => _context.Users.FirstOrDefault(u => u.UserName.ToLower() == userName.ToLower());
        #endregion

        public bool AreUserIDsValid(List<int> userIds, bool requireActive = true)
        {
            List<int> retval = GetValiduserIds(userIds, requireActive);
            return retval.Count == userIds.Count;
        }

        public List<int> GetValiduserIds(List<int> userIds, bool requireActive = true)
        {
            var users = _context.Users.Where(u => userIds.Contains(u.UserId));
            if (requireActive)
                users = users.Where(u => u.IsActive && !u.IsDisabled && !u.IsDeleted);
            return users.Select(u => u.UserId).ToList();
        }

        public bool IsUserIdValid(int userId, bool requireActive = true)
            => AreUserIDsValid(new List<int> { userId }, requireActive);

        public MS_User CreateUser(string userName, string password, string name)
        {
            if (_context.Users.Any(u => u.UserName.ToLower() == userName.ToLower()))
                throw new InvalidOperationException("UserName must be unique (case-insensitive).");

            var user = new MS_User
            {
                UserName = userName,
                Password = password,
                Name = name
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public void ImportUsers(List<MS_User> users)
        {
            _context.Users.AddRange(users);
            _context.SaveChanges();
        }
    }
}