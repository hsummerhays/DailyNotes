using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class TopicNotesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TopicNotesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task Post_CreatesTopicNoteSuccessfully()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            // 1. Create a topic first
            var topicPayload = new { title = "Note Parent Topic", visibility = "Private", proficiency = "Beginner", skillLevel = 1 };
            var topicContent = new StringContent(JsonSerializer.Serialize(topicPayload), Encoding.UTF8, "application/json");
            var topicResp = await client.PostAsync("/api/topics", topicContent);
            topicResp.EnsureSuccessStatusCode();
            var topicDoc = JsonDocument.Parse(await topicResp.Content.ReadAsStringAsync()).RootElement;
            int topicId = topicDoc.GetProperty("id").GetInt32();

            // 2. Create the note linked to that topic
            var payload = new
            {
                topicId = topicId,
                title = "Test Topic Note",
                content = new { body = "This is a test note." },
                timeMinutes = 15,
                visibility = "Private"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/topic-notes", content);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(1, doc.GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", doc.GetProperty("userId").GetString());
            Assert.Equal("Test Topic Note", doc.GetProperty("title").GetString());
            Assert.Equal(topicId, doc.GetProperty("topicId").GetInt32());
        }
    }
}
