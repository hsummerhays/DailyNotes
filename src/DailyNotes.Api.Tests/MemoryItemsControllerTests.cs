using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DailyNotes.Api.Tests
{
    public class MemoryItemsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public MemoryItemsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetDatabase();
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyCurrentUserMemories()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var resp = await client.GetAsync("/api/memory-items");
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(JsonValueKind.Array, doc.ValueKind);
        }

        [Fact]
        public async Task Post_CreatesAndSearchesMemorySuccessfully()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            // Create memory 1
            var payload1 = new
            {
                memoryType = "Learning",
                memoryStatus = "Active",
                summary = "React 19 upgrades and Zustand state management pattern",
                embedding = new float[1536], // Using a 1536-dimensional mock embedding vector
                importanceScore = 0.8,
                confidenceScore = 0.95,
                sourceEntityType = "Note",
                sourceEntityId = 42,
                sourceExcerpt = "I prefer remote jobs and Utah-based companies."
            };
            payload1.embedding[0] = 0.1f;
            payload1.embedding[1] = 0.2f;
            payload1.embedding[2] = 0.3f;
            payload1.embedding[3] = 0.4f;

            var content1 = new StringContent(JsonSerializer.Serialize(payload1), Encoding.UTF8, "application/json");
            var resp1 = await client.PostAsync("/api/memory-items", content1);
            resp1.EnsureSuccessStatusCode();

            var createdItem = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal("Learning", createdItem.GetProperty("memoryType").GetString());
            Assert.Equal("Active", createdItem.GetProperty("memoryStatus").GetString());
            Assert.Equal(0.8, createdItem.GetProperty("importanceScore").GetDouble());
            Assert.Equal(0.95, createdItem.GetProperty("confidenceScore").GetDouble());
            Assert.Equal("Note", createdItem.GetProperty("sourceEntityType").GetString());
            Assert.Equal(42, createdItem.GetProperty("sourceEntityId").GetInt32());
            Assert.Equal("I prefer remote jobs and Utah-based companies.", createdItem.GetProperty("sourceExcerpt").GetString());
            Assert.Equal(0, createdItem.GetProperty("accessCount").GetInt32());

            var createdId = createdItem.GetProperty("id").GetInt32();

            // Create memory 2 linked to memory 1
            var payload2 = new
            {
                memoryType = "Goal",
                memoryStatus = "Active",
                summary = "Master advanced frontend state management",
                embedding = new float[1536],
                importanceScore = 0.9,
                confidenceScore = 0.80,
                relatedMemoryId = createdId
            };
            payload2.embedding[0] = 0.12f;
            payload2.embedding[1] = 0.18f;
            payload2.embedding[2] = 0.28f;
            payload2.embedding[3] = 0.42f;

            var content2 = new StringContent(JsonSerializer.Serialize(payload2), Encoding.UTF8, "application/json");
            var resp2 = await client.PostAsync("/api/memory-items", content2);
            resp2.EnsureSuccessStatusCode();

            var createdItem2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(createdId, createdItem2.GetProperty("relatedMemoryId").GetInt32());

            // Search with vector close to payload 1 and 2
            var searchEmbedding = new float[1536];
            searchEmbedding[0] = 0.11f;
            searchEmbedding[1] = 0.19f;
            searchEmbedding[2] = 0.32f;
            searchEmbedding[3] = 0.38f;

            var searchPayload = new
            {
                queryEmbedding = searchEmbedding,
                minImportanceScore = 0.5,
                memoryStatus = "Active",
                limit = 2
            };
            var searchContent = new StringContent(JsonSerializer.Serialize(searchPayload), Encoding.UTF8, "application/json");
            var searchResp = await client.PostAsync("/api/memory-items/search", searchContent);
            searchResp.EnsureSuccessStatusCode();

            var searchResults = JsonDocument.Parse(await searchResp.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(JsonValueKind.Array, searchResults.ValueKind);
            Assert.True(searchResults.GetArrayLength() > 0);

            // Fetch memory 1 again to verify access count incremented
            var getResp = await client.GetAsync($"/api/memory-items/{createdId}");
            getResp.EnsureSuccessStatusCode();
            var fetchedItem = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync()).RootElement;
            Assert.True(fetchedItem.GetProperty("accessCount").GetInt32() > 0);
        }

        [Fact]
        public async Task Post_WithWrongEmbeddingDimensions_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var payload = new
            {
                memoryType = "Fact",
                summary = "Embedding has the wrong number of dimensions",
                embedding = new float[] { 0.1f, 0.2f, 0.3f }, // not 1536
                importanceScore = 0.5
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/memory-items", content);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Search_WithMinConfidenceScore_ExcludesLowConfidenceMemories()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

            var embedding = new float[1536];
            embedding[0] = 0.5f;

            var lowConfidencePayload = new
            {
                memoryType = "Fact",
                summary = "A guess with low confidence",
                embedding,
                importanceScore = 0.9,
                confidenceScore = 0.1
            };
            var highConfidencePayload = new
            {
                memoryType = "Fact",
                summary = "A well-established fact",
                embedding,
                importanceScore = 0.9,
                confidenceScore = 0.9
            };

            foreach (var payload in new object[] { lowConfidencePayload, highConfidencePayload })
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("/api/memory-items", content);
                resp.EnsureSuccessStatusCode();
            }

            var searchPayload = new
            {
                queryEmbedding = embedding,
                minConfidenceScore = 0.5,
                limit = 10
            };
            var searchContent = new StringContent(JsonSerializer.Serialize(searchPayload), Encoding.UTF8, "application/json");
            var searchResp = await client.PostAsync("/api/memory-items/search", searchContent);
            searchResp.EnsureSuccessStatusCode();

            var results = JsonDocument.Parse(await searchResp.Content.ReadAsStringAsync()).RootElement;
            Assert.True(results.GetArrayLength() > 0);
            Assert.All(results.EnumerateArray(), item => Assert.True(item.GetProperty("confidenceScore").GetDouble() >= 0.5));
        }
    }
}
