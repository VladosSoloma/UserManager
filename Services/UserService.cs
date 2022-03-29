using Azure.Identity;
using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Services.Abstractions;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly string _clientId;
        private readonly string _principalId;
        public UserService(IConfiguration configuration)
        {
            _principalId = configuration["AzureAd:ServicePrincipalId"];
            var clientId = configuration["AzureAd:ClientId"];
            _clientId = clientId;
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
                    new()
                    {
                        SignInType = "userName",
                        Issuer = "solomaorg.onmicrosoft.com",
                        IssuerAssignedId = $"{user.GivenName}_{user.Surname}"
                    },
                    new()
                    {
                        SignInType = "emailAddress",
                        Issuer = "solomaorg.onmicrosoft.com",
                        IssuerAssignedId = user.Mail
                    }
                },
                PasswordProfile = new PasswordProfile
                {
                    Password = user.Password,
                    ForceChangePasswordNextSignIn = false
                },
                PasswordPolicies = "DisablePasswordExpiration"
            };
            var dbUser = await _graphServiceClient.Users.Request().AddAsync(adUser);

            var servicePrincipals = await _graphServiceClient.ServicePrincipals.Request()
                .Filter($"appId eq '{_clientId}'")
                .GetAsync();
            var appRoles = servicePrincipals.Single().AppRoles;
            var appRoleAssignment = new AppRoleAssignment
            {
                PrincipalId = Guid.Parse(dbUser.Id),
                ResourceId = Guid.Parse(_principalId),
                AppRoleId = appRoles.SingleOrDefault(r => r.Value == "User")?.Id
            };
            await _graphServiceClient.Users[dbUser.Id]
                .AppRoleAssignments.Request()
                .AddAsync(appRoleAssignment);
            return new UserDto()
            {
                Id = dbUser.Id,
                DisplayName = dbUser.DisplayName,
                GivenName = dbUser.GivenName,
                Surname = dbUser.Surname,
            };
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _graphServiceClient.Users[userId].Request().DeleteAsync();
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = await _graphServiceClient.Users.Request().GetAsync();
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
