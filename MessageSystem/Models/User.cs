using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MessageSystem.Models
{
    [PrimaryKey("UserId")]
    public class MS_User
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDisabled { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public string Name { get; set; } = string.Empty;
        public List<MS_Message>? Messages { get; set; }
        public List<MS_Thread>? Threads { get; set; }

        public static MS_User? GetUserById(MessageSystemContext context, int userId)
        {
            return context.Users.Find(userId);
        }

        public static MS_User? GetUserByName(MessageSystemContext context, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }
            return context.Set<MS_User>().FirstOrDefault(u => u.UserName.ToLower() == userName.ToLower());
        }

        public static MS_User CreateUser(string userName, string password, string name, DbContext context)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            }
            if (context.Set<MS_User>().Any(u => u.UserName.ToLower() == userName.ToLower()))
            {
                throw new InvalidOperationException("UserName must be unique (case-insensitive).");
            }

            var user = new MS_User
            {
                UserName = userName,
                Password = password,
                Name = name
            };
            context.Set<MS_User>().Add(user);
            context.SaveChanges();
            return user;
        }

        public static void ImportUsers(List<MS_User> users, MessageSystemContext context)
        {
            if (users == null || users.Count == 0)
                throw new ArgumentException("User list cannot be null or empty.", nameof(users));

            int pos = 0;
            foreach (var user in users)
            {
                if (!user.IsValid(context))
                    throw new InvalidOperationException($"Invalid user at position {pos}.");
                pos++;
            }

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        public bool IsValid(MessageSystemContext context)
        {
            if (string.IsNullOrWhiteSpace(UserName))
                return false;
            if (string.IsNullOrWhiteSpace(Password))
                return false;
            if (string.IsNullOrWhiteSpace(Name))
                return false;
            if (context.Users.Any(u => u.UserName.ToLower() == UserName.ToLower() && u.UserId != UserId))
                return false;
            return true;
        }

    }
}