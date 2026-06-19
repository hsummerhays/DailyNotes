using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using DailyNotes.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DailyNotes.Application.Services
{
    public class MemoryItemService : ApplicationServiceBase, IMemoryItemService
    {
        public MemoryItemService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock)
            : base(db, tc, clock)
        {
        }

        public async Task<IEnumerable<MemoryItem>> GetAllAsync()
            => await TenantScoped(_db.MemoryItems).OrderByDescending(m => m.CreatedAt).ToListAsync();

        public async Task<MemoryItem?> GetByIdAsync(int id)
            => await TenantScoped(_db.MemoryItems).FirstOrDefaultAsync(m => m.Id == id);

        public async Task<MemoryItem> CreateAsync(MemoryItemRequest request)
        {
            ValidateEmbeddingDimensions(request.Embedding);

            var now = _clock.GetUtcNow().UtcDateTime;
            var item = new MemoryItem
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                MemoryType = request.MemoryType,
                MemoryStatus = request.MemoryStatus,
                Summary = request.Summary,
                Embedding = request.Embedding,
                ImportanceScore = request.ImportanceScore,
                CreatedAt = now,
                LastAccessedAt = now,
                LastConfirmedAt = request.LastConfirmedAt,
                RelatedMemoryId = request.RelatedMemoryId,
                SourceEntityType = request.SourceEntityType,
                SourceEntityId = request.SourceEntityId
            };

            _db.MemoryItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateAsync(int id, MemoryItemRequest request)
        {
            ValidateEmbeddingDimensions(request.Embedding);

            var existing = await TenantScoped(_db.MemoryItems).FirstOrDefaultAsync(m => m.Id == id);
            if (existing == null) return false;

            existing.MemoryType = request.MemoryType;
            existing.MemoryStatus = request.MemoryStatus;
            existing.Summary = request.Summary;
            existing.Embedding = request.Embedding;
            existing.ImportanceScore = request.ImportanceScore;
            existing.LastAccessedAt = _clock.GetUtcNow().UtcDateTime;
            existing.LastConfirmedAt = request.LastConfirmedAt;
            existing.RelatedMemoryId = request.RelatedMemoryId;
            existing.SourceEntityType = request.SourceEntityType;
            existing.SourceEntityId = request.SourceEntityId;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await TenantScoped(_db.MemoryItems).FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return false;

            _db.MemoryItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MemoryItem>> SearchAsync(
            float[] queryEmbedding,
            double minImportanceScore = 0.0,
            string? memoryType = null,
            string? memoryStatus = "Active",
            int limit = 5)
        {
            if (queryEmbedding == null || queryEmbedding.Length == 0)
            {
                return Array.Empty<MemoryItem>();
            }

            ValidateEmbeddingDimensions(queryEmbedding);

            List<MemoryItem> items;

            // EF Core InMemory provider does not support FromSqlRaw. Fallback for testing.
            if (_db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                var query = TenantScoped(_db.MemoryItems);

                if (minImportanceScore > 0.0)
                {
                    query = query.Where(m => m.ImportanceScore >= minImportanceScore);
                }

                if (!string.IsNullOrEmpty(memoryType))
                {
                    query = query.Where(m => m.MemoryType == memoryType);
                }

                if (!string.IsNullOrEmpty(memoryStatus))
                {
                    query = query.Where(m => m.MemoryStatus == memoryStatus);
                }

                items = await query.ToListAsync();
            }
            else
            {
                // In production Npgsql/PostgreSQL:
                // Fetch candidates using pgvector similarity search, then apply composite score in C#.
                var tenantId = _tc.TenantId;
                var userId = _tc.UserId;
                var limitCandidates = limit * 5;

                string sql = "SELECT * FROM memory_items WHERE tenant_id = {0} AND user_id = {1}";
                var parameters = new List<object> { tenantId, userId };
                int paramIndex = 2;

                if (minImportanceScore > 0.0)
                {
                    sql += $" AND importance_score >= {{{paramIndex++}}}";
                    parameters.Add(minImportanceScore);
                }

                if (!string.IsNullOrEmpty(memoryType))
                {
                    sql += $" AND memory_type = {{{paramIndex++}}}";
                    parameters.Add(memoryType);
                }

                if (!string.IsNullOrEmpty(memoryStatus))
                {
                    sql += $" AND memory_status = {{{paramIndex++}}}";
                    parameters.Add(memoryStatus);
                }

                sql += $" ORDER BY embedding <=> {{{paramIndex++}}} LIMIT {{{paramIndex}}}";
                parameters.Add(new Vector(queryEmbedding));
                parameters.Add(limitCandidates);

                items = await _db.MemoryItems.FromSqlRaw(sql, parameters.ToArray()).ToListAsync();
            }

            var now = _clock.GetUtcNow().UtcDateTime;

            // Re-rank items using the composite score formula:
            // score = semantic_similarity * 0.6 + importance_score * 0.2 + recency_score * 0.2
            // Boost importance by frequency of access: recalled memories rank higher.
            var results = items
                .Select(item =>
                {
                    double similarity = CosineSimilarity(item.Embedding, queryEmbedding);
                    double daysSinceAccess = (now - item.LastAccessedAt).TotalDays;
                    if (daysSinceAccess < 0) daysSinceAccess = 0; // clock skew safety
                    double recencyScore = 1.0 / (1.0 + daysSinceAccess);

                    // access count boost (up to +100% boost to importance_score for 100+ recalls)
                    double accessBoost = 1.0 + (Math.Min(item.AccessCount, 100) / 100.0);
                    double boostedImportance = Math.Min(item.ImportanceScore * accessBoost, 1.0);

                    double compositeScore = (similarity * 0.6) + (boostedImportance * 0.2) + (recencyScore * 0.2);

                    return new
                    {
                        Item = item,
                        CompositeScore = compositeScore
                    };
                })
                .OrderByDescending(r => r.CompositeScore)
                .Take(limit)
                .Select(r =>
                {
                    r.Item.LastAccessedAt = now;
                    r.Item.AccessCount++;
                    return r.Item;
                })
                .ToList();

            // Save the updated access times and counts
            if (results.Any())
            {
                await _db.SaveChangesAsync();
            }

            return results;
        }

        private static void ValidateEmbeddingDimensions(float[] embedding)
        {
            if (embedding.Length != MemoryItem.EmbeddingDimensions)
            {
                throw new DomainException(
                    $"Embedding must contain exactly {MemoryItem.EmbeddingDimensions} dimensions, but received {embedding.Length}.");
            }
        }

        private static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA == null || vectorB == null || vectorA.Length == 0 || vectorB.Length == 0)
            {
                return 0.0;
            }

            int length = Math.Min(vectorA.Length, vectorB.Length);
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }

            if (normA == 0.0 || normB == 0.0)
            {
                return 0.0;
            }

            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
