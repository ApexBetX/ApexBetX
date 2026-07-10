using ApexBetX.Controllers;
using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApexBetX.Tests.Controllers
{
    public class UsersControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var account1 = new Account
            {
                AccountId = 1,
                Email = "esihle@test.com",
                Password = "TestPassword",
                Role = "User",
                IsEmailVerified = true
            };

            var account2 = new Account
            {
                AccountId = 2,
                Email = "john@test.com",
                Password = "TestPassword",
                Role = "User",
                IsEmailVerified = true
            };

            context.Accounts.AddRange(
                account1,
                account2
            );

            var user1 = new User
            {
                UserId = 1,

                AccountId = 1,

                IDNumber = "0101015009087",

                FirstName = "Esihle",

                Surname = "Mlinjana",

                Email = "esihle@test.com",

                Phone = "0712345678",

                IsArchived = false,

                BettingAccounts = new List<BettingAccount>
                {
                    new BettingAccount
                    {
                        AccountId = 1,

                        AccountNumber = "ACC001",

                        Balance = 0,

                        IsClosed = false,

                        CreatedDate = DateTime.Today
                    }
                }
            };

            var user2 = new User
            {
                UserId = 2,

                AccountId = 2,

                IDNumber = "0202025009088",

                FirstName = "John",

                Surname = "Smith",

                Email = "john@test.com",

                Phone = "0723456789",

                IsArchived = false,

                BettingAccounts = new List<BettingAccount>
                {
                    new BettingAccount
                    {
                        AccountId = 2,

                        AccountNumber = "ACC002",

                        Balance = 0,

                        IsClosed = false,

                        CreatedDate = DateTime.Today
                    }
                }
            };

            context.Users.AddRange(
                user1,
                user2
            );

            context.SaveChanges();

            return context;
        }

        private EmailService GetEmailService()
        {
            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            {
                                "SmtpSettings:Host",
                                "smtp.test.com"
                            },

                            {
                                "SmtpSettings:Port",
                                "587"
                            },

                            {
                                "SmtpSettings:Username",
                                "test@test.com"
                            },

                            {
                                "SmtpSettings:Password",
                                "testpassword"
                            }
                        })
                    .Build();

            return new EmailService(configuration);
        }

        [Fact]
        public async Task
            Index_Returns_View_With_All_Users_When_Search_Is_Empty()
        {
            var context = GetDbContext();

            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index(null);

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Index_Searches_By_IDNumber()
        {
            var context = GetDbContext();

            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index(
                    "0101015009087");

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Single(model);

            Assert.Equal(
                "Esihle",
                model[0].FirstName);
        }

        [Fact]
        public async Task Index_Searches_By_Surname()
        {
            var context = GetDbContext();

            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index("Smith");

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Single(model);

            Assert.Equal(
                "John",
                model[0].FirstName);
        }

        [Fact]
        public async Task
            Index_Searches_By_AccountNumber()
        {
            var context = GetDbContext();

            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index("ACC001");

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Single(model);

            Assert.Equal(
                "Mlinjana",
                model[0].Surname);
        }

        [Fact]
        public async Task
            Index_Returns_Empty_List_When_No_Search_Match()
        {

            var context = GetDbContext();

            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index("NotFound");

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Empty(model);
        }


        [Fact]
        public async Task
            Index_Does_Not_Return_Unverified_Users()
        {
            // Arrange

            var context = GetDbContext();

            var account =
                await context.Accounts.FindAsync(1);

            account!.IsEmailVerified = false;

            await context.SaveChangesAsync();


            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index(null);

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Single(model);

            Assert.DoesNotContain(
                model,
                user => user.Email == "esihle@test.com");
        }


        [Fact]
        public async Task
            Index_Does_Not_Return_Archived_Users()
        {
            // Arrange

            var context = GetDbContext();

            var user =
                await context.Users.FindAsync(1);

            user!.IsArchived = true;

            await context.SaveChangesAsync();


            var userService =
                new UserService(context);

            var emailService =
                GetEmailService();

            var controller =
                new UsersController(
                    context,
                    userService,
                    emailService);

            var result =
                await controller.Index(null);

            var viewResult =
                Assert.IsType<ViewResult>(result);

            var model =
                Assert.IsAssignableFrom<List<User>>(
                    viewResult.Model);

            Assert.Single(model);

            Assert.DoesNotContain(
                model,
                user => user.Email == "esihle@test.com");
        }
    }
}