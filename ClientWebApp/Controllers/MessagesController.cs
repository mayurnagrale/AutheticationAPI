using ClientWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ClientWebApp.Services;
using System.IdentityModel.Tokens.Jwt;

namespace ClientWebApp.Controllers
{
    public class MessagesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        //private _helper = new Helper();

        public MessagesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5246/api/Chat/GetChatList"; // Replace with actual API URL

            List<UserChatListDTO> chatList = new List<UserChatListDTO>();

            // Retrieve access token from cookies
            string accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                // Redirect to login if no access token is found
                return RedirectToAction("Login", "Auth");
            }

            // Set authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    chatList = JsonConvert.DeserializeObject<List<UserChatListDTO>>(jsonResponse);
                    // Retrieve logged-in user's ID from token
                    Guid loggedInUserId = GetUserIdFromToken(accessToken);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token might have expired, redirect to login
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to fetch chat list. Please try again.");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error fetching chat list: {ex.Message}");
            }

            return View(chatList);
        }

        public class UserChatListDTO
        {
            public Guid Id { get; set; }  
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public bool IsOnline { get; set; }
            public DateTime? LastSeen { get; set; }
        }

        private Guid GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                // Read the JWT token
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                if (jsonToken == null) return Guid.Empty;

                // Get the "sub" claim (User ID)
                string userIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

                return Guid.TryParse(userIdClaim, out Guid userId) ? userId : Guid.Empty;
            }
            catch
            {
                return Guid.Empty; // Return empty if any issue occurs
            }
        }

        public class ChatRequest
        {
            public string UserId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> GetChatHistory([FromBody] ChatRequest request)
        {
            string userid = request.UserId; // Now correctly mapped
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5246/api/Chat/GetChatHistory/{userid}";

            List<Message> chatList = new List<Message>();

            // Retrieve access token from cookies
            string accessToken = Request.Cookies["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Auth");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    chatList = JsonConvert.DeserializeObject<List<Message>>(jsonResponse);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to fetch chat list. Please try again.");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error fetching chat list: {ex.Message}");
            }

            return Json(chatList);
        }

        public class Message
        {
            public int Id { get; set; }
            public Guid SenderId { get; set; }
            public Guid RecipientId { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsRead { get; set; }
        }
    }
}
