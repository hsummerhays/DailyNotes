-- Check current tenant_users
SELECT * FROM tenant_users;
SELECT * FROM tenants;

-- Check if user has a tenant association
SELECT tu.*, t.name as tenant_name 
FROM tenant_users tu 
JOIN tenants t ON tu.tenant_id = t.id;
