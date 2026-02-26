using System.Text;
using System.Text.Json;

namespace PollVoter
{
	public static class Program
	{
		private static async Task Main()
		{
			Console.WriteLine("Fetching the current poll data...");

			using var client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync("https://api.falcon1k.dev/poll/current");
			response.EnsureSuccessStatusCode();
			var currentPoll = JsonSerializer.Deserialize<PollResponse>(await response.Content.ReadAsStringAsync(),
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (currentPoll == null || string.IsNullOrEmpty(currentPoll.PollName))
			{
				Console.WriteLine("Failed to fetch poll data, please try again later.");

				return;
			}

			Console.WriteLine("--------------------------------------------------------------------------");
			Console.WriteLine($"Current poll: {currentPoll.PollName}");
			Console.WriteLine("--------------------------------------------------------------------------");
			Console.WriteLine("Options:");

			for (var i = 0; i < currentPoll.Options.Length; i++)
			{
				int optionIndex = i + 1;
				Console.WriteLine($"[{optionIndex}] {currentPoll.Options[i]}");
			}

			Console.WriteLine("--------------------------------------------------------------------------");
			Console.WriteLine($"Enter your choice (1-{currentPoll.Options.Length}):");
			string? userInput = Console.ReadLine();

			if (userInput != null && int.TryParse(userInput, out int userChoice) && userChoice >= 1 &&
				userChoice <= currentPoll.Options.Length)
			{
				Console.WriteLine($"You chose option {userChoice}. Uploading your vote...");

				var payload = new { votedFor = userChoice };
				string jsonPayload = JsonSerializer.Serialize(payload);
				HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
				await client.PostAsync("https://api.falcon1k.dev/poll/upload", content);

				Console.WriteLine("Vote uploaded successfully!");
				Console.WriteLine("Visit https://api.falcon1k.dev/poll/fetch to see the results.");
				Console.WriteLine("--------------------------------------------------------------------------");
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
			}
			else
			{
				Console.WriteLine("Invalid input. Please enter a valid option number.");
				Console.WriteLine("--------------------------------------------------------------------------");
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
			}
		}

		[Serializable]
		private class PollResponse
		{
			public string PollName { get; set; } = null!;
			public string[] Options { get; set; } = null!;
		}
	}
}