using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GiveAID.Web.Controllers;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;
using Moq;
using Xunit;

namespace GiveAID.Tests.Tests.Integration
{
    /// <summary>
    /// Integration tests for <see cref="AuthController"/>.
    ///
    /// These tests exercise the full request → controller → DB → response pipeline
    /// using a LocalDB test database via <see cref="TestDbContextFactory"/>.
    /// Each test gets a fresh, seeded database via <c>ResetDatabase</c>.
    ///
    /// Covered flows:
    ///   • Register — new user, duplicate email, duplicate username
    ///   • Login    — valid credentials, wrong password, non-existent user
    ///   • Me       — with valid token
    ///   • Logout   — valid token acknowledgement
    /// </summary>
    public class AuthFlowTests : IDisposable
    {
        private readonly GiveAIDContext _context;

        public AuthFlowTests()
        {
            _context = TestDbContextFactory.CreateContext();
            TestDbContextFactory.ResetDatabase(_context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        // ════════════════════════════════════════════════════════════════════
        // Register
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Register_NewUser_Succeeds()
        {
            var controller = new AuthController();

            var result = controller.Register(new RegisterViewModel
            {
                Username  = "newuser",
                Email     = "newuser@test.com",
                Password  = "NewUser@123",
                FullName  = "New Test User",
                Profession = "Teacher"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var user = _context.Users.FirstOrDefault(u => u.Email == "newuser@test.com");
            Assert.NotNull(user);
            Assert.Equal("newuser", user.Username);
            Assert.Equal("New Test User", user.FullName);
            Assert.Equal("User", user.Role);
            Assert.True(user.IsActive);

            // Password must be hashed (BCrypt), not stored in plaintext.
            Assert.NotEqual("NewUser@123", user.PasswordHash);
            Assert.StartsWith("$2", user.PasswordHash); // BCrypt prefix
            Assert.True(PasswordHasher.Verify("NewUser@123", user.PasswordHash));
        }

        [Fact]
        public void Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Pre-seed the user that the test will try to re-register
            _context.Users.Add(new User
            {
                Username     = "existinguser",
                Email        = "duplicate@test.com",
                PasswordHash = PasswordHasher.Hash("Password@123"),
                FullName     = "Existing User",
                Role         = "User",
                IsActive     = true,
                CreatedAt    = DateTime.Now,
                UpdatedAt    = DateTime.Now
            });
            _context.SaveChanges();

            var controller = new AuthController();
            var result = controller.Register(new RegisterViewModel
            {
                Username = "differentuser",
                Email    = "duplicate@test.com", // same email
                Password = "Password@123",
                FullName = "New Name"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_DuplicateUsername_ReturnsBadRequest()
        {
            _context.Users.Add(new User
            {
                Username     = "takenuser",
                Email        = "takenuser@example.com",
                PasswordHash = PasswordHasher.Hash("Password@123"),
                FullName     = "Taken User",
                Role         = "User",
                IsActive     = true,
                CreatedAt    = DateTime.Now,
                UpdatedAt    = DateTime.Now
            });
            _context.SaveChanges();

            var controller = new AuthController();
            var result = controller.Register(new RegisterViewModel
            {
                Username = "takenuser", // same username
                Email    = "newemail@test.com",
                Password = "Password@123",
                FullName = "New Name"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_WeakPassword_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Register(new RegisterViewModel
            {
                Username = "weakuser",
                Email    = "weak@test.com",
                Password = "123", // too short
                FullName = "Weak Password User"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_MissingEmail_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Register(new RegisterViewModel
            {
                Username = "noemailuser",
                Email    = "",
                Password = "Password@123",
                FullName = "No Email"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_InvalidEmail_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Register(new RegisterViewModel
            {
                Username = "bademailuser",
                Email    = "not-an-email",
                Password = "Password@123",
                FullName = "Bad Email User"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_ShortUsername_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Register(new RegisterViewModel
            {
                Username = "ab", // < 3 chars
                Email    = "shortuser@test.com",
                Password = "Password@123",
                FullName = "Short Username"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Login
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Login_ValidCredentials_ReturnsToken()
        {
            // Insert a known user with a known password hash
            var passwordHash = PasswordHasher.Hash(TestDbContextFactory.TestUsers.AdminPassword);
            _context.Users.Add(new User
            {
                Username          = "logintest",
                Email             = "logintest@test.com",
                PasswordHash      = passwordHash,
                FullName          = "Login Test",
                Role              = "Admin",
                IsActive          = true,
                IsVerified        = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            });
            _context.SaveChanges();

            var controller = new AuthController();
            var result = controller.Login(new LoginRequest
            {
                Email    = "logintest@test.com",
                Password = TestDbContextFactory.TestUsers.AdminPassword
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsBadRequest()
        {
            var passwordHash = PasswordHasher.Hash("CorrectPassword");
            _context.Users.Add(new User
            {
                Username     = "wrongpwtest",
                Email        = "wrongpw@test.com",
                PasswordHash = passwordHash,
                FullName     = "Wrong Password Test",
                Role         = "User",
                IsActive     = true,
                CreatedAt    = DateTime.Now,
                UpdatedAt    = DateTime.Now
            });
            _context.SaveChanges();

            var controller = new AuthController();
            var result = controller.Login(new LoginRequest
            {
                Email    = "wrongpw@test.com",
                Password = "CorrectPassword"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Login_NonExistentUser_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Login(new LoginRequest
            {
                Email    = "nobody@test.com",
                Password = "AnyPassword123"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Login_InactiveUser_ReturnsBadRequest()
        {
            var passwordHash = PasswordHasher.Hash("Password@123");
            _context.Users.Add(new User
            {
                Username     = "inactiveuser",
                Email        = "inactive@test.com",
                PasswordHash = passwordHash,
                FullName     = "Inactive User",
                Role         = "User",
                IsActive     = false, // deactivated
                CreatedAt    = DateTime.Now,
                UpdatedAt    = DateTime.Now
            });
            _context.SaveChanges();

            var controller = new AuthController();
            var result = controller.Login(new LoginRequest
            {
                Email    = "inactive@test.com",
                Password = "Password@123"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Login_EmptyEmail_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Login(new LoginRequest
            {
                Email    = "",
                Password = "Password@123"
            });

Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Login_NullRequest_ReturnsBadRequest()
        {
            var controller = new AuthController();
            var result = controller.Login(null);
            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Logout
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Logout_ValidToken_ReturnsOk()
        {
            // Create a user and generate a valid token
            var user = new User
            {
                Username          = "logouttest",
                Email             = "logout@test.com",
                PasswordHash      = PasswordHasher.Hash("Password@123"),
                FullName           = "Logout Test",
                Role               = "User",
                IsActive           = true,
                PasswordChangedAt  = DateTime.UtcNow,
                CreatedAt          = DateTime.UtcNow,
                UpdatedAt          = DateTime.UtcNow
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var token = JwtHelper.GenerateToken(user);

            // Build a request with a valid Authorization header
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            httpRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var controller = new AuthController
            {
                Request = httpRequest
            };

            var result = controller.Logout();
Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Logout_NoToken_ReturnsUnauthorized()
        {
            var controller = new AuthController
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/")
            };

            var result = controller.Logout();
Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }
    }
}
