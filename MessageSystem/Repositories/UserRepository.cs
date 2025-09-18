using Microsoft.EntityFrameworkCore;
using MessageSystem.Models;

namespace MessageSystem.Repositories
{
    public interface IUserRepository
    {
        MS_User? GetUserById(int userId);
        List<MS_User> GetUsersById(List<int> userIds);
        MS_User? GetUserByName(string userName);
        MS_User CreateUser(string userName, string password, string name);
        void ImportUsers(List<MS_User> users);
    }

    public class UserRepository : IUserRepository
    {
        private readonly MessageSystemContext _context;

        public UserRepository(MessageSystemContext context)
        {
            _context = context;
        }

        #region retrieval methods
        public MS_User? GetUserById(int userId)
            => _context.Users.Find(userId);
        
        public List<MS_User> GetUsersById(List<int> userIds)
            => _context.Users.Where(u=> userIds.Contains(u.UserId)).Select(u=>u).ToList();


        public MS_User? GetUserByName(string userName)
            => _context.Users.FirstOrDefault(u => u.UserName.ToLower() == userName.ToLower());
        #endregion



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