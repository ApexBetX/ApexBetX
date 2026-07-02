using ApexBetX.Models;

namespace ApexBetX.Services
{
    public class UserService
    {
        public bool CanDeleteUser(User user)
        {
            return user.BettingAccounts == null ||
                   !user.BettingAccounts.Any() ||
                   user.BettingAccounts.All(a => a.IsClosed);
        }
    }
}