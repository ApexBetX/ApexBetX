using ApexBetX.Controllers;
using ApexBetX.Data;
using ApexBetX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApexBetX.Tests.Controllers
{
    public class UsersControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            context.Users.AddRange(
                new User
                {
                    UserId = 1,
                    IDNumber = "0101015009087",
                    FirstName = "Esihle",
                    Surname = "Mlinjana",
                    Email = "esihle@test.com",
                    Phone = "0712345678",
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
                },
                new User
                {
                    UserId = 2,
                    IDNumber = "0202025009088",
                    FirstName = "John",
                    Surname = "Smith",
                    Email = "john@test.com",
                    Phone = "0723456789",
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
                }
            );

            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task Index_Returns_View_With_All_Users_When_Search_Is_Empty()
        {
            var context = GetDbContext();
            var controller = new UsersController(context);

            var result = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);

            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Index_Searches_By_IDNumber()
        {
            var context = GetDbContext();
            var controller = new UsersController(context);

            var result = await controller.Index("0101015009087");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("Esihle", model[0].FirstName);
        }

        [Fact]
        public async Task Index_Searches_By_Surname()
        {
            var context = GetDbContext();
            var controller = new UsersController(context);

            var result = await controller.Index("Smith");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("John", model[0].FirstName);
        }

        [Fact]
        public async Task Index_Searches_By_AccountNumber()
        {
            var context = GetDbContext();
            var controller = new UsersController(context);

            var result = await controller.Index("ACC001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("Mlinjana", model[0].Surname);
        }

        [Fact]
        public async Task Index_Returns_Empty_List_When_No_Search_Match()
        {
            var context = GetDbContext();
            var controller = new UsersController(context);

            var result = await controller.Index("NotFound");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);

            Assert.Empty(model);
        }
    }
}