using ClientWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;

namespace ClientWebApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var client = _httpClientFactory.CreateClient();

            var loginRequest = new
            {
                username = username,
                passwordHash = password
            };

            var response = await client.PostAsJsonAsync("http://localhost:5246/Auth/Login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
                // Store tokens securely (e.g., in cookies or session storage)
                //HttpContext.Session.SetString("AccessToken", tokens.AccessToken);
                //HttpContext.Session.SetString("RefreshToken", tokens.RefreshToken);

                Response.Cookies.Append("AccessToken", tokens.AccessToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15) // Match token expiry
                });

                Response.Cookies.Append("RefreshToken", tokens.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7) // Longer expiry for refresh token
                });

                Response.Cookies.Append("UserId", tokens.UserId.ToString(), new CookieOptions
                {
                    HttpOnly = false, // Optional: Allow client-side access if needed
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                Response.Cookies.Append("Username", username, new CookieOptions
                {
                    HttpOnly = false, // Allow access in JavaScript if needed
                    Secure = true, // Set true if using HTTPS
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });

                var accessToken = tokens.AccessToken;
                var signalRClient = new SignalRClientService(client);
                await signalRClient.ConnectToSignalR(accessToken);

                //signalRClient.OnNotificationReceived += HandleNotification;

                if (username != null && username == "Admin")
                {
                    TempData["ToastrMessage"] = "Welcome back, Admin!";
                    TempData["ToastrType"] = "success";
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string phoneNumber, string username, string password)
        {
            var client = _httpClientFactory.CreateClient();

            var registerRequest = new
            {
                firstName = firstName,
                lastName = lastName,
                phoneNumber = phoneNumber,
                username = username,
                passwordHash = password
            };

            var response = await client.PostAsJsonAsync("http://localhost:5246/Auth/Register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to login page after successful registration
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "There was an error registering the user.");
            return View();
        }


        public async Task<IActionResult> Logout()
        {

            if (Request.Cookies.TryGetValue("UserId", out string userId) && Guid.TryParse(userId, out Guid parsedUserId))
            {
                var client = _httpClientFactory.CreateClient();

                // Call the server logout endpoint
                var response =await client.PostAsync($"http://localhost:5246/Auth/Logout/{parsedUserId}", null);

                if (response.IsSuccessStatusCode)
                {
                    // Clear cookies after successful logout
                    Response.Cookies.Delete("AccessToken");
                    Response.Cookies.Delete("RefreshToken");
                    Response.Cookies.Delete("UserId");
                    Response.Cookies.Delete("Username");

                }
                else
                {
                    TempData["ToastrMessage"] = "Error during logout. Please try again.";
                    TempData["ToastrType"] = "error";
                }
            }
            else
            {
                TempData["ToastrMessage"] = "User ID not found in cookies.";
                TempData["ToastrType"] = "warning";
            }

            return RedirectToAction("Login", "Account");
        }

        //private void HandleNotification(string message)
        //{
        //}

    }



    // TokenResponse class for deserialization
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid UserId { get; set; }
    }
}
