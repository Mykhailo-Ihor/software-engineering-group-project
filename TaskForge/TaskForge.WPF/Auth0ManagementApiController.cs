using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskForge.WPF
{
    public class Auth0ManagementApiController
    {
        private const string Domain = "dev-ki8p3p3wo311vi24.us.auth0.com";
        private const string ClientId = "y4EKFUYATWHldohGZB0OWFUSEDwXkEdn";
        private const string ClientSecret = "S6oMJwAYfmIsdBA3ltAslGd-nMtGc9-4dsZJMWwloNraGBnZGwo4XLEMMeTBCpxy";

        public async Task<string> GetManagementApiTokenAsync()
        {
            using var client = new HttpClient();
            var body = new
            {
                client_id = ClientId,
                client_secret = ClientSecret,
                audience = $"https://{Domain}/api/v2/",
                grant_type = "client_credentials"
            };
            var response = await client.PostAsync(
                $"https://{Domain}/oauth/token",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task UpdateAuth0UserProfileAsync(string userId, string firstName, string lastName, string email)
        {
            var token = await GetManagementApiTokenAsync();
            var managementClient = new ManagementApiClient(token, new Uri($"https://{Domain}/api/v2/"));
            var updateUserRequest = new UserUpdateRequest
            {
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Email = email
            };
            await managementClient.Users.UpdateAsync(userId, updateUserRequest);
        }
    }
}
