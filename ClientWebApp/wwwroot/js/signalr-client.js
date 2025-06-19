const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5246/notificationHub", {
        accessTokenFactory: () => localStorage.getItem("AccessToken") // ✅ Pass JWT Token
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveLoginNotification", (username) => {
    console.log(`🚀 User ${username} just logged in.`);
    alert(`User ${username} just logged in.`);
});

connection.start()
    .then(() => console.log("✅ Connected to SignalR!"))
    .catch(err => console.error("❌ SignalR Connection Error:", err));