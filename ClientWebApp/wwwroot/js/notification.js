"use strict";

// --- Helper function to get a cookie by name ---
function getCookie(name) {
    const cookieName = `${name}=`;
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(cookieName) === 0) {
            return c.substring(cookieName.length, c.length);
        }
    }
    return null;
}

// --- Start SignalR connection for chat ---
const AccessToken = getCookie("AccessToken");
console.log('AccessToken:', AccessToken);

let connection;

if (AccessToken) {
    startSignalRConnection();
} else {
    console.error("AccessToken is missing or null. SignalR connection not started.");
}

function startSignalRConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5246/notificationHub", {
            accessTokenFactory: () => AccessToken
        })
        .withAutomaticReconnect()
        .build();

    // Receive real-time user status updates
    connection.on("ReceiveUserStatusUpdate", (username, isOnline) => {
        console.log(`User ${username} is now ${isOnline ? "online" : "offline"}`);
        toastr.info(`${username} is now ${isOnline ? "online" : "offline"}`);
        updateUserStatus(username, isOnline);
    });

    // Receive list of online users
    connection.on("ReceiveOnlineUsers", (users) => {
        console.log("Online Users:", users);
        displayOnlineUsers(users);
    });

    // Event handler for login notifications (for admins)
    connection.on("ReceiveLoginNotification", (message) => {
        console.log("Login Notification:", message);
        toastr.info(message + " has logged in.");
    });

    // Event handler for login notifications (for admins)
    connection.on("ReceiveLogoutNotification", (message) => {
        console.log("Login Notification:", message);
        toastr.info(message);
    });

    connection.start()
        .then(() => {
            console.log("SignalR connection started successfully.");
            /*connection.invoke("GetOnlineUsers").catch(err => console.error("Error getting online users:", err));*/
        })
        .catch(err => console.error("Error starting SignalR connection:", err));
}

// Function to update user status in the UI
function updateUserStatus(username, isOnline) {
    let row = document.querySelector(`#userListBody tr[data-username='${username}']`);
    if (row) {
        row.querySelector(".status").textContent = isOnline ? "🟢 Online" : "🔴 Offline";
    } else {
        const tableBody = document.getElementById("userListBody");
        if (tableBody) {
            const newRow = document.createElement("tr");
            newRow.setAttribute("data-username", username);
            newRow.innerHTML = `<td>${username}</td><td class="status">${isOnline ? "🟢 Online" : "🔴 Offline"}</td>`;
            tableBody.appendChild(newRow);
        }
    }
}

// Function to display the list of online users
function displayOnlineUsers(users) {
    const userListElement = document.getElementById("userListBody");
    if (!userListElement) {
        console.error("userList element not found.");
        return;
    }

    userListElement.innerHTML = "";
    users.forEach(user => {
        const row = document.createElement("tr");
        row.innerHTML = `<td>${user}</td><td class="status">🟢 Online</td>`;
        userListElement.appendChild(row);
    });
}
