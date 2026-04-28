using Microsoft.JSInterop;

namespace BlazorWebAssembly.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private string _currentTheme = "light";

        public event Action<string>? OnThemeChanged;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public string CurrentTheme => _currentTheme;

        public async Task InitializeAsync()
        {
            var savedTheme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "app-theme");
            if (!string.IsNullOrEmpty(savedTheme))
            {
                await SetThemeAsync(savedTheme);
            }
            else
            {
                await SetThemeAsync("light");
            }
        }

        public async Task SetThemeAsync(string theme)
        {
            _currentTheme = theme;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "app-theme", theme);
            await _jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", theme);
            OnThemeChanged?.Invoke(theme);
        }
    }
}
