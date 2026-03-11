using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class AssignmentsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AssignmentsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task Post_CreatesAssignmentSuccessfully()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            // 1. Create a course first
            var coursePayload = new
            {
                name = "Test Course",
                semester = "Spring 2026",
                description = "Test Course Description",
                credits = 3,
                progressPercent = 0
            };
            var courseContent = new StringContent(JsonSerializer.Serialize(coursePayload), Encoding.UTF8, "application/json");
            var courseResp = await client.PostAsync("/api/courses", courseContent);
            courseResp.EnsureSuccessStatusCode();
            var courseDoc = JsonDocument.Parse(await courseResp.Content.ReadAsStringAsync()).RootElement;
            int courseId = courseDoc.GetProperty("id").GetInt32();

            // 2. Create the assignment linked to that course
            var payload = new
            {
                courseId = courseId,
                title = "Test Assignment",
                description = "This is a test assignment.",
                dueDate = System.DateTime.UtcNow.AddDays(7).ToString("o"),
                status = "Pending",
                weight = 10
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/assignments", content);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(1, doc.GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", doc.GetProperty("userId").GetString());
            Assert.Equal("Test Assignment", doc.GetProperty("title").GetString());
            Assert.Equal(courseId, doc.GetProperty("courseId").GetInt32());
        }
    }
}
