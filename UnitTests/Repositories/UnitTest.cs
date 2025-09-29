using MessageSystem.Models;
using MessageSystem.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace UnitTests;

public class RepositoryTests
{
    private MessageSystemContext _context = null!;
    private IUserRepository _userRepo = null!;
    private IMessageRepository _msgRepo = null!;
    private IThreadRepository _threadRepo = null!;

    [SetUp]
    public void Setup()
    {
        _context = new MessageSystemContext();
        _userRepo = new UserRepository(_context);
        _msgRepo = new MessageRepository(_context);
        _threadRepo = new ThreadRepository(_context);
    }

    [Test]
    public void UserRepository_CanCreateAndRetrieveUser()
    {
        MS_User newUsr = _userRepo.CreateUser("user1", "pass1", "User One");

        //Test retrieval by ID
        MS_User? retrievedUsr = _userRepo.GetUserById(newUsr.UserId);
        Assert.That(retrievedUsr, Is.Not.Null);
        Assert.That(retrievedUsr?.UserName, Is.EqualTo("user1"));
        Assert.That(retrievedUsr?.Password, Is.EqualTo("pass1"));
        Assert.That(retrievedUsr?.Name,Is.EqualTo("User One"));

        //test case mismatchewd retrieval by UserName
        MS_User? retrievedByName = _userRepo.GetUserByName("UsEr1");
        Assert.That(retrievedByName?.UserName, Is.EqualTo("user1"));
        Assert.That(retrievedByName?.UserId, Is.EqualTo(newUsr.UserId));
        Assert.That(retrievedByName?.Password, Is.EqualTo("pass1"));
        Assert.That(retrievedByName?.Name, Is.EqualTo("User One"));

        //test multiple user import and retrieval
        List<MS_User> users = new List<MS_User> {
            new MS_User() { UserName = "user2", Password = "pass2", Name = "User Two" },
            new MS_User() { UserName = "user3", Password = "pass3", Name = "User Three" }
        };

        _userRepo.ImportUsers(users);
        List<MS_User> existingUsers = _userRepo.GetUsersById(users.Select(u => u.UserId).ToList());

        Assert.That(existingUsers.Count, Is.EqualTo(2));
            Assert.That(existingUsers.Any(u => u.UserName == "user2"));
            Assert.That(existingUsers.Any(u => u.UserName == "user3"));

        //test retrieval of mix of existing and non-existing users
        List<int> mixedIds = new List<int> { users[0].UserId, users[1].UserId, 9999 };
        List<MS_User> mixedExisting = _userRepo.GetUsersById(mixedIds);
        Assert.That(mixedExisting.Count, Is.EqualTo(2));
        Assert.That(mixedExisting.Any(u => u.UserName == "user2"));
        Assert.That(mixedExisting.Any(u => u.UserName == "user3"));

        //test retrieval of non-existing user
        MS_User? nonExist = _userRepo.GetUserById(9999);
        Assert.That(nonExist, Is.Null);

        //test creation of user with duplicate username
        Assert.Throws<InvalidOperationException>(() => _userRepo.CreateUser("USER1", "passX", "User X"));
    

        Assert.Pass();
    }
    
    [TearDown]
    public void Teardown()
    {
        _context.Dispose();
    }
}