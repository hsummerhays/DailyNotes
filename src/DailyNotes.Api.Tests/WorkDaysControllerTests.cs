using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class WorkDaysControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public WorkDaysControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            // Ensure a fresh, seeded DB for each test (fixture is shared across tests)
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyCurrentUserWorkDays()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var resp = await client.GetAsync("/api/work-days");
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(JsonValueKind.Array, doc.ValueKind);
            var arr = doc.EnumerateArray().ToArray();
            Assert.Single(arr);
            Assert.Equal(1, arr[0].GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", arr[0].GetProperty("userId").GetString());
        }

        [Fact]
        public async Task GetById_OtherTenant_ReturnsNotFound()
        {
            // first get the id for tenant2's workday
            var clientT2 = _factory.CreateClient();
            clientT2.DefaultRequestHeaders.Add("X-User-Id", "user-b");
            clientT2.DefaultRequestHeaders.Add("X-Tenant-Id", "2");

            var respT2 = await clientT2.GetAsync("/api/work-days");
            respT2.EnsureSuccessStatusCode();
            var arr = JsonDocument.Parse(await respT2.Content.ReadAsStringAsync()).RootElement.EnumerateArray().ToArray();
            var otherId = arr[0].GetProperty("id").GetInt32();

            // now request as user-a / tenant 1
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var resp = await client.GetAsync($"/api/work-days/{otherId}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Post_IgnoresTenantAndUserInBody_UsesClaims()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var payload = new
            {
                tenantId = 999,
                userId = "other",
                workDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
                timeIn1 = "09:00",
                timeOut1 = "17:00"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/work-days", content);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(1, doc.GetProperty("tenantId").GetInt32());
            Assert.Equal("user-a", doc.GetProperty("userId").GetString());
        }
    }
}
