using Azure.Identity;
using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Services.Abstractions;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public UserService(IConfiguration configuration)
        {
            var clientId = configuration["AzureAd:ClientId"];
            var tenantId = configuration["AzureAd:TenantId"];
            var clientSecret = configuration["AzureAd:ClientSecret"];
            var scopes = new[] { configuration["AzureAd:Scopes"] };
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(
                tenantId,
                clientId,
                clientSecret,
                options);

            _graphServiceClient =
                new GraphServiceClient(clientSecretCredential, scopes);
        }

        public async Task<UserDto> AddUserAsync(RegisterUserDto user)
        {
            var adUser = new User
            {
                DisplayName = $"{user.GivenName} {user.Surname}",
                GivenName = user.GivenName,
                Surname = user.Surname,
                Mail = user.Mail,
                Identities = new List<ObjectIdentity>()
                {
                    new ObjectIdentity
                    {
                        SignInType = "userName",
                        Issuer = "solomaorg.onmicrosoft.com",
                        IssuerAssignedId = $"{user.GivenName}_{user.Surname}"
                    },
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = "solomaorg.onmicrosoft.com",
                        IssuerAssignedId = user.Mail
                    }
                },
                PasswordProfile = new PasswordProfile
                {
                    Password = user.Password,
                    ForceChangePasswordNextSignIn = true
                },
                PasswordPolicies = "DisablePasswordExpiration"
            };
            var result = await _graphServiceClient.Users.Request().AddAsync(adUser);
            return new UserDto()
            {
                Id = result.Id,
                DisplayName = result.DisplayName,
                GivenName = result.GivenName,
                Surname = result.Surname,
            };
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _graphServiceClient.Users[userId].Request().DeleteAsync();
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = await _graphServiceClient.Users.Request().GetAsync();
            var test = await _graphServiceClient.ServicePrincipals.Request()
                .Filter("appId eq '3229905c-3379-4d7d-8b84-146f8e80e7fd'")
                .GetAsync();
            var appRoles = test[0].AppRoles;

            foreach (var role in appRoles)
            {
                if (role.Value == "User1")
                {
                    var appRoleAssignment = new AppRoleAssignment
                    {
                        PrincipalId = Guid.Parse("db38ae42-d20f-464a-9f19-9252b8e85006"),
                        ResourceId = Guid.Parse("a0668e5d-f5ca-425e-9bcf-e95f89edb6a8"),
                        AppRoleId = role.Id
                    };
                    var testUser = await _graphServiceClient.Users["db38ae42-d20f-464a-9f19-9252b8e85006"]
                        .AppRoleAssignments.Request()
                        .AddAsync(appRoleAssignment);
                }
            }
            return users.Select(u => new UserDto()
            {
                Id = u.Id, Mail = u.Mail, DisplayName = u.DisplayName, GivenName = u.GivenName, Surname = u.Surname
            });
        }

        public Task<UserDto> UpdateUserAsync(UpdateUserDto user)
        {
            throw new NotImplementedException();
        }
    }
}
