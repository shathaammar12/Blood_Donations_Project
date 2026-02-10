using Blood_Donations_Project.Models;
using Blood_Donations_Project.ViewModels;

namespace Blood_Donations_Project.Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginViewModel model);
    }
}
