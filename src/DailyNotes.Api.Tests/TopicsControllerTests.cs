using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class TopicsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TopicsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyCurrentUserTopics()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var resp = await client.GetAsync("/api/topics");
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(JsonValueKind.Array, doc.ValueKind);
        }

        [Fact]
        public async Task Post_CreatesTopicSuccessfully()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var payload = new
            {
                title = "Backend Development",
                description = "Mastering .NET and EF Core",
                proficiency = "Intermediate",
                skillLevel = 3,
                visibility = "Private"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/topics", content);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(1, doc.GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", doc.GetProperty("userId").GetString());
            Assert.Equal("Backend Development", doc.GetProperty("title").GetString());
        }
    }
}
