using Microsoft.AspNetCore.SignalR.Client;

namespace ClientWebApp.Services
{
    public class SignalRClientService
    {
        private readonly HttpClient _httpClient;
        private HubConnection _hubConnection;

        public event Action<string> OnNotificationReceived;

        public SignalRClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task ConnectToSignalR(string _accessToken)
        {
            // Retrieve the JWT token from cookies
            var accessToken = _accessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("No access token found. Unable to connect to SignalR.");
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5246/NotificationHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .Build();

            _hubConnection.On<string>("ReceiveLoginNotification", (message) =>
            {
                Console.WriteLine($"New Notification: {message}");
                //OnNotificationReceived?.Invoke(message);
                _hubConnection.InvokeAsync("NotifyAdmin", message);


            });

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("Connected to SignalR Hub");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to SignalR: {ex.Message}");
            }
        }

    }
}
