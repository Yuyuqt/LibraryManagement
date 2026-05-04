import { initializeApp } from "https://www.gstatic.com/firebasejs/11.6.1/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/11.6.1/firebase-messaging.js";

const firebaseConfig = {
  apiKey: "AIzaSyBodr7GJuCFdJfBYV8XBY77P9A4Wrxz_CA",
  authDomain: "library-ths.firebaseapp.com",
  projectId: "library-ths",
  storageBucket: "library-ths.firebasestorage.app",
  messagingSenderId: "222408896403",
  appId: "1:222408896403:web:8533e26175c043e40a0266",
  measurementId: "G-LVVGH9T1K3"
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

window.getFcmToken = async () => {
    try {
        const permission = await Notification.requestPermission();
        if (permission === 'granted') {
            // IMPORTANT: Replace 'YOUR_VAPID_KEY' with your actual key from 
            // Firebase Console -> Project Settings -> Cloud Messaging -> Web configuration -> Web Push certificates
            console.log('Registering service worker...');
            const registration = await navigator.serviceWorker.register('/firebase-messaging-sw.js');
            console.log('Service worker registered successfully:', registration.scope);

            const token = await getToken(messaging, { 
                vapidKey: 'BNgDM2Eq7TPwap0FhF-xv0CK6cS5EfshHOiC8z4916XnewFWYKdAfhwHctsgte5q8jQicQO8PEDVeymlXToasVQ',
                serviceWorkerRegistration: registration
            });
            return token;
        } else {
            throw new Error('Permission not granted for notifications. Current state: ' + permission);
        }
    } catch (error) {
        console.error('Error getting FCM token:', error);
        throw new Error(error.message || error.toString());
    }
};

onMessage(messaging, (payload) => {
    console.log('Foreground Message received: ', payload);
    
    const title = payload.notification.title;
    const body = payload.notification.body;

    if (window.showToast) {
        window.showToast(title + ": " + body, 'success');
    }
});
