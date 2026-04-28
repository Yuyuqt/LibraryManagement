using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://150.95.88.91:4100");
        client.DefaultRequestHeaders.Add("x-system-id", "THS-LMS");

        Console.WriteLine("Testing Loyalty API Connectivity...");
        
        try 
        {
            Console.WriteLine("\n1. Testing Active Rewards...");
            var rewardsResponse = await client.GetAsync("/api/v1/rewards/active/THS-LMS");
            Console.WriteLine($"Status: {rewardsResponse.StatusCode}");
            if (rewardsResponse.IsSuccessStatusCode)
            {
                var content = await rewardsResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Content: {content.Substring(0, Math.Min(100, content.Length))}...");
            }
            else
            {
                var error = await rewardsResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {error}");
            }

            Console.WriteLine("\n2. Testing Account Lookup (Test User)...");
            // Using a dummy ID to see if we get a 404 or a connection error
            var lookupResponse = await client.GetAsync("/api/v1/accounts/lookup/THS-LMS/test-user-id");
            Console.WriteLine($"Status: {lookupResponse.StatusCode}");
            if (lookupResponse.IsSuccessStatusCode)
            {
                var content = await lookupResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Content: {content}");
            }
            else
            {
                var error = await lookupResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nCRITICAL CONNECTION ERROR: {ex.Message}");
        }
    }
}
