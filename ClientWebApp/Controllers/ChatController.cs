using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;

namespace ClientWebApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        //private _helper = new Helper();

        public ChatController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChatWithAdmin()
        {
            var client = _httpClientFactory.CreateClient();

            // Retrieve access token from cookies
            string accessToken = Request.Cookies["AccessToken"];
            var username = Request.Cookies["Username"];
            var userId1 = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Auth");
            }


            if (string.IsNullOrEmpty(userId1))
            {
                return Unauthorized("User ID not found in token.");
            }

            

            // Admin ID (replace this with your actual admin user ID)
            string adminUserId = "67FFCAAA-5C64-466F-5D2B-08DD30805857";

            string apiUrl = $"http://localhost:5246/api/Chat/GetChatWithAdmin/{userId1}";

            List<Message> chatList = new List<Message>();
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
                    ModelState.AddModelError("", "Failed to fetch chat history. Please try again.");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error fetching chat history: {ex.Message}");
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
