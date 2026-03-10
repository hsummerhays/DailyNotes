-- Link all data to a specific user.
--
-- HOW TO FIND YOUR USER UUID:
--   Run the following query first to look up your user's ID:
--       SELECT "Id", "Email" FROM asp_net_users;
--   Then replace <YOUR_USER_UUID> below with your actual UUID before running this script.
--
-- Example: Replace '<YOUR_USER_UUID>' with 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'

-- Tables WITH UserId + TenantId
UPDATE projects SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE work_tasks SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE work_days SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE work_notes SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE topics SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE topic_notes SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE courses SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE quiz_attempts SET "UserId" = '<YOUR_USER_UUID>';
UPDATE attachments SET "UserId" = '<YOUR_USER_UUID>', "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);

-- Tables with only TenantId (no UserId)
UPDATE pay_periods SET "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);
UPDATE assignments SET "TenantId" = (SELECT "TenantId" FROM tenant_users WHERE "UserId" = '<YOUR_USER_UUID>' LIMIT 1);

-- Verify
SELECT 'projects' as tbl, count(*) FROM projects WHERE "UserId" = '<YOUR_USER_UUID>'
UNION ALL SELECT 'work_tasks', count(*) FROM work_tasks WHERE "UserId" = '<YOUR_USER_UUID>'
UNION ALL SELECT 'work_days', count(*) FROM work_days WHERE "UserId" = '<YOUR_USER_UUID>'
UNION ALL SELECT 'work_notes', count(*) FROM work_notes WHERE "UserId" = '<YOUR_USER_UUID>'
UNION ALL SELECT 'topics', count(*) FROM topics WHERE "UserId" = '<YOUR_USER_UUID>'
UNION ALL SELECT 'courses', count(*) FROM courses WHERE "UserId" = '<YOUR_USER_UUID>';
