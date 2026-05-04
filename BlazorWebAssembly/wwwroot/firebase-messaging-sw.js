// Scripts for firebase and messaging
// Service worker တွေမှာ import အစား importScripts ကို သုံးပါတယ်
importScripts('https://www.gstatic.com/firebasejs/11.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/11.6.1/firebase-messaging-compat.js');

// --- Firebase Config ---
// Step 2 ကနေ ရခဲ့တဲ့ ကျွန်တော်တို့ရဲ့ Firebase Project Configuration ကို ဒီနေရာမှာ ထည့်ပေးပါ
const firebaseConfig = {
  apiKey: "AIzaSyBodr7GJuCFdJfBYV8XBY77P9A4Wrxz_CA",
  authDomain: "library-ths.firebaseapp.com",
  projectId: "library-ths",
  storageBucket: "library-ths.firebasestorage.app",
  messagingSenderId: "222408896403",
  appId: "1:222408896403:web:8533e26175c043e40a0266",
  measurementId: "G-LVVGH9T1K3"
};

// Service worker မှာ Firebase app ကို initialize လုပ်ခြင်း
firebase.initializeApp(firebaseConfig);

// Background မှာ message တွေ လက်ခံနိုင်ဖို့ Firebase Messaging instance ကို ရယူခြင်း
const messaging = firebase.messaging();

// Background မှာရောက်လာတဲ့ message တွေကို ကိုင်တွယ်ဖြေရှင်းမယ့် အပိုင်း
messaging.onBackgroundMessage(function(payload) {
    console.log('[firebase-messaging-sw.js] Received background message ', payload);

    const title = payload.notification.title;
    const body = payload.notification.body;

    const notificationOptions = {
        body: body,
        icon: '/favicon.png' 
    };

    self.registration.showNotification(title, notificationOptions);
});
