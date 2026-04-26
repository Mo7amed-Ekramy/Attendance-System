// Real-time notification listener using SignalR
// This file handles receiving real-time notifications from the server

let notificationConnection = null;
let currentStudentId = null;

// Connect to the notification hub
async function connectToNotificationHub(studentId, sectionIds = []) {
    try {
        if (notificationConnection) {
            await disconnectFromNotificationHub();
        }

        // Build connection URL
        const hubUrl = '/notificationHub';

        // Create connection
        notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Set up connection state handlers
        notificationConnection.onclose(async () => {
            console.log('Notification hub connection closed. Attempting to reconnect...');
            await startConnection();
        });

        notificationConnection.onreconnecting(error => {
            console.log('Notification hub reconnecting...', error);
        });

        notificationConnection.onreconnected(connectionId => {
            console.log('Notification hub reconnected with connectionId:', connectionId);
        });

        // Set up notification receiver
        notificationConnection.on('ReceiveNotification', (notification) => {
            console.log('Notification received:', notification);
            handleIncomingNotification(notification);
        });

        // Set up connection started handler to join groups
        notificationConnection.onreconnected(() => {
            console.log('Rejoining groups after reconnection...');
            joinGroups(studentId, sectionIds);
        });

        // Start the connection
        await startConnection();

        // Join student and section groups
        await joinGroups(studentId, sectionIds);

        currentStudentId = studentId;
        console.log('Connected to notification hub');
    } catch (error) {
        console.error('Error connecting to notification hub:', error);
    }
}

// Start the connection
async function startConnection() {
    try {
        await notificationConnection.start();
        console.log('Notification hub started');
    } catch (error) {
        console.error('Error starting notification hub:', error);
        // Retry after delay
        setTimeout(() => startConnection(), 5000);
    }
}

// Join user to appropriate groups
async function joinGroups(studentId, sectionIds) {
    try {
        // Join student group to receive personal notifications
        if (studentId) {
            await notificationConnection.invoke('JoinStudentGroup', studentId);
            console.log('Joined student group:', studentId);
        }

        // Join section groups to receive section-specific notifications
        if (sectionIds && sectionIds.length > 0) {
            for (const sectionId of sectionIds) {
                await notificationConnection.invoke('JoinSectionGroup', sectionId);
                console.log('Joined section group:', sectionId);
            }
        }
    } catch (error) {
        console.error('Error joining groups:', error);
    }
}

// Leave groups when no longer needed
async function leaveGroups(studentId, sectionIds = []) {
    try {
        if (studentId && notificationConnection) {
            await notificationConnection.invoke('LeaveStudentGroup', studentId);
        }

        if (sectionIds && sectionIds.length > 0) {
            for (const sectionId of sectionIds) {
                await notificationConnection.invoke('LeaveSectionGroup', sectionId);
            }
        }
    } catch (error) {
        console.error('Error leaving groups:', error);
    }
}

// Disconnect from the hub
async function disconnectFromNotificationHub() {
    try {
        if (notificationConnection) {
            if (currentStudentId) {
                await leaveGroups(currentStudentId);
            }
            await notificationConnection.stop();
            notificationConnection = null;
            console.log('Disconnected from notification hub');
        }
    } catch (error) {
        console.error('Error disconnecting from notification hub:', error);
    }
}

// Handle incoming notification
function handleIncomingNotification(notification) {
    // Create a notification object
    const notificationData = {
        id: null, // Will be set when loaded from database
        title: notification.title,
        type: notification.type,
        isRead: false,
        createdAt: new Date(notification.createdAt)
    };

    // Dispatch a custom event that can be listened to by other components
    const event = new CustomEvent('notificationReceived', {
        detail: notificationData
    });
    document.dispatchEvent(event);

    // Also try to show a browser notification if permitted
    showBrowserNotification(notificationData);

    // Store in localStorage for offline viewing
    storeNotification(notificationData);
}

// Show browser notification
function showBrowserNotification(notification) {
    if ('Notification' in window) {
        // Request permission if not granted
        if (Notification.permission === 'default') {
            Notification.requestPermission().then(permission => {
                if (permission === 'granted') {
                    createBrowserNotification(notification);
                }
            });
        } else if (Notification.permission === 'granted') {
            createBrowserNotification(notification);
        }
    }
}

// Create browser notification
function createBrowserNotification(notification) {
    const browserNotification = new Notification('New Notification', {
        body: notification.title,
        icon: '/favicon.ico'
    });

    // Auto-close after 5 seconds
    setTimeout(() => {
        browserNotification.close();
    }, 5000);

    // Click to focus the window
    browserNotification.onclick = () => {
        window.focus();
        browserNotification.close();
    };
}

// Store notification in localStorage
function storeNotification(notification) {
    try {
        const storedNotifications = JSON.parse(localStorage.getItem('offlineNotifications') || '[]');
        storedNotifications.push(notification);
        localStorage.setItem('offlineNotifications', JSON.stringify(storedNotifications));
    } catch (error) {
        console.error('Error storing notification in localStorage:', error);
    }
}

// Get offline notifications from localStorage
function getOfflineNotifications() {
    try {
        return JSON.parse(localStorage.getItem('offlineNotifications') || '[]');
    } catch (error) {
        console.error('Error getting offline notifications:', error);
        return [];
    }
}

// Clear offline notifications
function clearOfflineNotifications() {
    try {
        localStorage.removeItem('offlineNotifications');
    } catch (error) {
        console.error('Error clearing offline notifications:', error);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('Notification JS loaded');

    // Listen for custom events from other components
    window.addEventListener('joinNotificationHub', (event) => {
        const { studentId, sectionIds } = event.detail;
        connectToNotificationHub(studentId, sectionIds);
    });

    window.addEventListener('leaveNotificationHub', async () => {
        await disconnectFromNotificationHub();
    });

    // Check for stored offline notifications
    const offlineNotifications = getOfflineNotifications();
    if (offlineNotifications.length > 0) {
        console.log('Found offline notifications:', offlineNotifications.length);
        // Dispatch an event with offline notifications
        const event = new CustomEvent('offlineNotificationsLoaded', {
            detail: offlineNotifications
        });
        document.dispatchEvent(event);
    }
});

// Export functions for use by other scripts
window.NotificationManager = {
    connect: connectToNotificationHub,
    disconnect: disconnectFromNotificationHub,
    joinGroups: joinGroups,
    leaveGroups: leaveGroups,
    getOfflineNotifications: getOfflineNotifications,
    clearOfflineNotifications: clearOfflineNotifications
};
