using AppsTracker.Data.Models;

namespace AppsTracker.Domain.Users
{
    public sealed class UserModel
    {
        internal UserModel(Uzer user)
        {
            UserID = user.ID;
            Name = user.Name;
        }

        public int UserID { get; }

        public string Name { get; }
    }
}