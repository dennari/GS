

using CommonDomain;
using System;
using System.Threading.Tasks;
namespace Growthstories.Core
{

    public interface IAuthToken
    {
        string AccessToken { get; set; }
        int ExpiresIn { get; set; }
        string RefreshToken { get; set; }
    }

    public interface IAuthUser : IAuthToken, IMemento
    {
        string Username { get; }
        string Password { get; }
        string Email { get; }
        Guid GardenId { get; }
        bool IsCollaborator { get; }
        bool IsRegistered { get; }
    }


    public interface IIAPService
    {
        bool HasPaidBasicProduct();
        Task<bool> ShopForBasicProduct();
    }

    public class NullIIAP : IIAPService
    {

        public bool HasPaidBasicProduct()
        {
            return true;
        }

        public Task<bool> ShopForBasicProduct()
        {
            return Task.FromResult(true);
        }
    }

}
