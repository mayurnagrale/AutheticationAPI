// wwwroot/js/chat.js
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
const accessToken = getCookie("AccessToken");
let chatConnection;

// Only create the connection if there's an access token
if (accessToken) {
    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5246/chatHub", {  // Corrected URL format
            accessTokenFactory: () => accessToken
        })
        .withAutomaticReconnect()
        .build();

    chatConnection.on("receivemessage", (user, message) => {
        console.log("message from server to user", user, message);
        //addMessageToUI(user, message, 'received'); // Ensure these functions exist
        const selectedUser = document.getElementById("selected_user_id").value; //Ensure element with Id exist
        console.log(selectedUser);
        console.log(user);
        if (selectedUser === user) {
            addMessageToUINew(user, message, 'received'); // Ensure these functions exist
        }
        else if (user == "Admin") {
            addMessageToChatUINew(user, message, 'received');
        }
        else {
            console.log("Message received but chat is not open for this user.");
        }
    });

    // Start the connection *first*, *then* register the handler
    chatConnection.start()
        .then(() => {
            console.log("Chat connection started successfully!");

        })
        .catch(err => console.error("Error starting chat connection:", err));

} else {
    console.warn("User not authenticated. Chat functionality disabled.");
}

// --- Validation and Sending Functions ---

function validateAndSendMessageToUser() {
    const recipientSelect = document.getElementById("recipientSelect");
    const messageInput = document.getElementById("messageInput");

    if (!recipientSelect || !messageInput) {
        console.error("Required elements not found.");
        alert("Error: Required elements are missing.");
        return;
    }

    const recipient = recipientSelect.value;
    console.log(recipient);
    const message = messageInput.value;
    console.log(message);

    if (!recipient) {
        alert("Please select a recipient.");
        return;
    }

    if (!message.trim()) {
        alert("Message cannot be empty.");
        return;
    }

    sendMessageToUser(recipient, message); // Call the actual send function
}

function validateAndSendMessageToAdmin() {
    const chatInput = document.getElementById("chat_input");
    const message = chatInput.value.trim(); // Get and trim the message

    if (message) {
        console.log("Message to admin:", message);
        // Here you can add your logic to send the message to admin
        sendMessageToAdmin("Admin", message);

        chatInput.value = ""; // Clear input after sending
    } else {
        console.log("Empty message. Not sending.");
    }

    
}

function validateAndSendMessageToUserNew() {
    const recipientSelect = document.getElementById("selected_user_id").value;
    const messageInput = document.getElementById("message_input");

    if (!recipientSelect || !messageInput) {
        console.error("Required elements not found.");
        alert("Error: Required elements are missing.");
        return;
    }

    const recipient = recipientSelect;
    console.log(recipient);
    const message = messageInput.value;

    if (!recipient) {
        alert("Please select a recipient.");
        return;
    }

    if (!message.trim()) {
        alert("Message cannot be empty.");
        return;
    }

    sendMessageToUser(recipient, message);

    messageInput.value = "";
}
function validateAndSendMessageToAll() {
    const messageInput = document.getElementById("messageInput");
    if (!messageInput) {
        console.error("Message input field not found.");
        alert("Error: Message input field is missing.");
        return;
    }
    const message = messageInput.value;
    if (!message.trim()) {
        alert("Message cannot be empty.");
        return;
    }

    sendMessageToAll(message); // Call the actual send function
}


function sendMessageToUser(recipient, message) {
    const sender = getCookie('Username');
    if (!sender) {
        console.error("Sender username not found.");
        return;
    }
    console.log(chatConnection);
    if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
        chatConnection.invoke("SendMessageToUser", recipient, sender, message)
            .then(() => {
                console.log("message sent from here");
                addMessageToUINew(recipient, message, 'sent');
                // Check if the input element exists *before* trying to clear it
                const messageInput = document.getElementById("messageInput");
                if (messageInput) {
                    messageInput.value = "";
                } else {
                    console.warn("messageInput element not found in this context.");
                }
            })
            .catch(err => console.error("Error sending message:", err));
    } else {
        console.error("Chat connection is not established or not connected.");
    }
}

function sendMessageToAdmin(recipient, message) {
    const sender = getCookie('Username');
    if (!sender) {
        console.error("Sender username not found.");
        return;
    }
    console.log(chatConnection);
    if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
        chatConnection.invoke("SendMessageToUser", recipient, sender, message)
            .then(() => {
                console.log("message sent from here");
                addMessageToChatUINew(recipient, message, 'sent');
                // Check if the input element exists *before* trying to clear it
                const messageInput = document.getElementById("chat_input");
                if (messageInput) {
                    messageInput.value = "";
                } else {
                    console.warn("messageInput element not found in this context.");
                }
            })
            .catch(err => console.error("Error sending message:", err));
    } else {
        console.error("Chat connection is not established or not connected.");
    }
}

function sendMessageToAll(message) {
    const sender = getCookie('Username');
    if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
        chatConnection.invoke("SendMessageToAll", sender, message)
            .then(() => {
                addMessageToUI(sender, message, 'sent');
                addMessageToUINew(user, message, 'received');
                document.getElementById("messageInput").value = ""; //Clear input
            })
            .catch(err => console.error("Error sending message to all:", err));
    } else {
        console.error("Chat connection is not established or not connected.");
    }
}



function addMessageToUINew(sender, message, messageType) {
    const chatMessagesDiv = document.getElementById("chat-messages");
    if (!chatMessagesDiv) {
        console.warn("Chat window is not open. Skipping message update.");
        return; // Stop execution if the chat window is not open
    }

    console.log("Updating chat UI:", sender, message, messageType);
    const messageDiv = document.createElement("div");
    messageDiv.classList.add("message");

    if (messageType === "sent") {
        messageDiv.classList.add("user-message");
    } else {
        messageDiv.classList.add("partner-message");
    }

    const messageContent = document.createElement("div");
    messageContent.classList.add("message-content");
    messageContent.textContent = message;

    const messageTime = document.createElement("div");
    messageTime.classList.add("message-time");
    messageTime.textContent = new Date().toLocaleTimeString();

    messageDiv.appendChild(messageContent);
    messageDiv.appendChild(messageTime);
    chatMessagesDiv.appendChild(messageDiv);

    // Scroll to the bottom for new messages
    chatMessagesDiv.scrollTop = chatMessagesDiv.scrollHeight;
}


function addMessageToChatUINew(sender, message, messageType) {
    const chatMessagesDiv = document.getElementById("chat-messages");
    if (!chatMessagesDiv) {
        console.warn("Chat window is not open. Skipping message update.");
        return; // Stop execution if the chat window is not open
    }

    console.log("Updating chat UI:", sender, message, messageType);
    const messageDiv = document.createElement("div");
    messageDiv.classList.add("message");

    if (messageType === "sent") {
        messageDiv.classList.add("user-message");
    } else {
        messageDiv.classList.add("partner-message");
    }

    const messageContent = document.createElement("div");
    messageContent.classList.add("message-content");
    messageContent.textContent = message;

    const messageTime = document.createElement("div");
    messageTime.classList.add("message-time");
    messageTime.textContent = new Date().toLocaleTimeString();

    messageDiv.appendChild(messageContent);
    messageDiv.appendChild(messageTime);
    chatMessagesDiv.appendChild(messageDiv);

    // Scroll to the bottom for new messages
    chatMessagesDiv.scrollTop = chatMessagesDiv.scrollHeight;
}



// Function to scroll to the bottom of the messages list
function scrollToBottom() {
    var messagesList = document.getElementById("messages");
    messagesList.scrollTop = messagesList.scrollHeight;
}

// --- Attach event listeners (after DOM is loaded) ---
document.addEventListener('DOMContentLoaded', () => {
    const sendButton = document.getElementById("sendButton");
    const sendMessageButton = document.getElementById("send_message_button");
    console.log(sendMessageButton);
    const sendChatButton = document.getElementById("send_chat_button");
    const sendToAllButton = document.getElementById("sendToAllButton");
    if (sendButton) {
        sendButton.addEventListener("click", validateAndSendMessageToUser); // Attach to validation function
    }
    if (sendToAllButton) {
        sendToAllButton.addEventListener("click", validateAndSendMessageToAll); // Attach to validation function
    }

    if (sendChatButton) {
        sendChatButton.addEventListener("click", validateAndSendMessageToAdmin); //Attach to validation function
    }

    if (sendMessageButton) {
        sendMessageButton.addEventListener("click", validateAndSendMessageToUserNew);
    } else {
        console.error("send_message_button not found!");
    }
});