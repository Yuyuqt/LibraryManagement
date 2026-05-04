using System;

namespace BlazorWebAssembly.Services
{
    public class NotificationStateService
    {
        public int UnreadCount { get; private set; }
        public event Action? OnChange;

        public void SetUnreadCount(int count)
        {
            UnreadCount = count;
            NotifyStateChanged();
        }

        public void DecrementCount()
        {
            if (UnreadCount > 0)
            {
                UnreadCount--;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
