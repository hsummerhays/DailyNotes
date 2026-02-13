-- Link all data to user: a17a384d-1bf6-4d7a-843e-15974c670c5c

-- Tables WITH UserId + TenantId
UPDATE projects SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE work_tasks SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE work_days SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE work_notes SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE topics SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE topic_notes SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE courses SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE quiz_attempts SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c';
UPDATE attachments SET "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);

-- Tables with only TenantId (no UserId)
UPDATE pay_periods SET "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);
UPDATE assignments SET "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c' LIMIT 1);

-- Verify
SELECT 'projects' as tbl, count(*) FROM projects WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c'
UNION ALL SELECT 'work_tasks', count(*) FROM work_tasks WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c'
UNION ALL SELECT 'work_days', count(*) FROM work_days WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c'
UNION ALL SELECT 'work_notes', count(*) FROM work_notes WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c'
UNION ALL SELECT 'topics', count(*) FROM topics WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c'
UNION ALL SELECT 'courses', count(*) FROM courses WHERE "UserId" = 'a17a384d-1bf6-4d7a-843e-15974c670c5c';
