using System.Net.Http.Json;

var client = new HttpClient();
client.BaseAddress = new Uri("http://150.95.88.91:4100");
client.DefaultRequestHeaders.Add("x-system-id", "THS-LMS");

try {
    Console.WriteLine("Testing connection to Loyalty API...");
    var response = await client.GetAsync("/api/v1/rewards/active/THS-LMS");
    Console.WriteLine($"Status: {response.StatusCode}");
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Content: {content.Substring(0, Math.Min(content.Length, 100))}...");
} catch (Exception ex) {
    Console.WriteLine($"Connection failed: {ex.Message}");
    if (ex.InnerException != null) {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
}
