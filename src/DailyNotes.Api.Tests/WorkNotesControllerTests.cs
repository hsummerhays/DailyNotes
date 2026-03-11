using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class WorkNotesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public WorkNotesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task Post_CreatesWorkNoteSuccessfully()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            // 1. Create a project first
            var projectPayload = new { name = "Note Parent Project", visibility = "Private" };
            var projectContent = new StringContent(JsonSerializer.Serialize(projectPayload), Encoding.UTF8, "application/json");
            var projectResp = await client.PostAsync("/api/projects", projectContent);
            projectResp.EnsureSuccessStatusCode();
            var projectDoc = JsonDocument.Parse(await projectResp.Content.ReadAsStringAsync()).RootElement;
            int projectId = projectDoc.GetProperty("id").GetInt32();

            // 2. Create a task first
            var taskPayload = new { name = "Note Parent Task", visibility = "Private", projectId = projectId, status = "InProgress" };
            var taskContent = new StringContent(JsonSerializer.Serialize(taskPayload), Encoding.UTF8, "application/json");
            var taskResp = await client.PostAsync("/api/work-tasks", taskContent);
            taskResp.EnsureSuccessStatusCode();
            var taskDoc = JsonDocument.Parse(await taskResp.Content.ReadAsStringAsync()).RootElement;
            int taskId = taskDoc.GetProperty("id").GetInt32();

            // 3. Create the note linked to that task
            var payload = new
            {
                workTaskId = taskId,
                noteDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
                content = new { body = "This is a test work note." },
                timeMinutes = 30,
                visibility = "Private"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/work-notes", content);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(1, doc.GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", doc.GetProperty("userId").GetString());
            Assert.Equal(taskId, doc.GetProperty("workTaskId").GetInt32());
        }
    }
}
