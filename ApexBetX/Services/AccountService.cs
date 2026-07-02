using ApexBetX.Models;

namespace ApexBetX.Services
{
    public class AccountService
    {
        public bool CanCloseAccount(BettingAccount account)
        {
            return account.Balance == 0;
        }

        public bool CanAddTransaction(BettingAccount account)
        {
            return account.IsClosed == false;
        }
    }
}