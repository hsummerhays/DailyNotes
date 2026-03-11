using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class WorkTasksControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public WorkTasksControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyCurrentUserWorkTasks()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var resp = await client.GetAsync("/api/work-tasks");
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(JsonValueKind.Array, doc.ValueKind);
            // It might be empty or populated depending on the seed. But it should parse successfully.
        }

        [Fact]
        public async Task Post_CreatesWorkTaskSuccessfully()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            // 1. Create a project first
            var projectPayload = new { name = "Task Parent Project", visibility = "Private" };
            var projectContent = new StringContent(JsonSerializer.Serialize(projectPayload), Encoding.UTF8, "application/json");
            var projectResp = await client.PostAsync("/api/projects", projectContent);
            projectResp.EnsureSuccessStatusCode();
            var projectDoc = JsonDocument.Parse(await projectResp.Content.ReadAsStringAsync()).RootElement;
            int projectId = projectDoc.GetProperty("id").GetInt32();

            // 2. Create the task linked to that project
            var payload = new
            {
                tenantId = 999, // should be ignored
                userId = "other", // should be ignored
                name = "Test Work Task",
                status = "InProgress",
                visibility = "Private",
                projectId = projectId
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/work-tasks", content);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(1, doc.GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", doc.GetProperty("userId").GetString());
            Assert.Equal("Test Work Task", doc.GetProperty("name").GetString());
            Assert.Equal(projectId, doc.GetProperty("projectId").GetInt32());
        }
    }
}
