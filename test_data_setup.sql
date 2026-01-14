-- Test Data Setup Script for Tasks 8, 9, and 10
-- Run this script in your PostgreSQL database to set up test data

-- Create test users
INSERT INTO "Users" ("Id", "Email", "Name", "CreatedAt", "UpdatedAt")
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'admin@example.com', 'Admin User', NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222222', 'owner@example.com', 'Organization Owner', NOW(), NOW()),
    ('33333333-3333-3333-3333-333333333333', 'orgadmin@example.com', 'Org Admin', NOW(), NOW()),
    ('44444444-4444-4444-4444-444444444444', 'member@example.com', 'Regular Member', NOW(), NOW()),
    ('55555555-5555-5555-5555-555555555555', 'user1@example.com', 'User One', NOW(), NOW()),
    ('66666666-6666-6666-6666-666666666666', 'user2@example.com', 'User Two', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Create test organizations
INSERT INTO "Organizations" ("Id", "Name", "Description", "CreatedAt", "UpdatedAt")
VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Acme Corporation', 'A test organization', NOW(), NOW()),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Tech Solutions Inc', 'Another test organization', NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Create organization memberships
INSERT INTO "OrganizationMembers" ("Id", "UserId", "OrganizationId", "Role", "JoinedAt", "CreatedAt", "UpdatedAt")
VALUES 
    -- Acme Corporation members
    ('aaaaaaaa-0001-0001-0001-000000000001', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Owner', NOW(), NOW(), NOW()),
    ('aaaaaaaa-0002-0002-0002-000000000002', '33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'OrganizationAdmin', NOW(), NOW(), NOW()),
    ('aaaaaaaa-0003-0003-0003-000000000003', '44444444-4444-4444-4444-444444444444', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Member', NOW(), NOW(), NOW()),
    -- Tech Solutions Inc members
    ('bbbbbbbb-0001-0001-0001-000000000001', '55555555-5555-5555-5555-555555555555', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Owner', NOW(), NOW(), NOW())
ON CONFLICT DO NOTHING;

-- Verify the data
SELECT 'Users created' AS Status, COUNT(*) AS Count FROM "Users";
SELECT 'Organizations created' AS Status, COUNT(*) AS Count FROM "Organizations";
SELECT 'Memberships created' AS Status, COUNT(*) AS Count FROM "OrganizationMembers";
