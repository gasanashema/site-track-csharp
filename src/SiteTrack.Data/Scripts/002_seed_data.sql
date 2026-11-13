-- =============================================================================
-- SITETRACK CONSTRUCTION MANAGER
-- Seed Data: 002_seed_data.sql
-- Default Administrator, Reference Catalogs, Sample Project & Workers
-- =============================================================================

INSERT INTO users (id, full_name, username, email, password, role, phone, status, created_at, updated_at)
VALUES 
(1, 'System Administrator', 'admin', 'admin@sitetrack.rw', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'Administrator', '+250788100001', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 'Patrick Mutabazi', 'sitemgr', 'pmutabazi@sitetrack.rw', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'SiteManager', '+250788100002', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 'Aline Uwimana', 'aline.mgr', 'aline@sitetrack.rw', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'SiteManager', '+250788100003', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (username) DO NOTHING;

INSERT INTO projects (id, project_name, location, description, start_date, expected_end_date, status, created_by, created_at, updated_at)
VALUES 
(1, 'Kigali Heights Commercial Complex Wing B', 'Kimihurura, Kigali', 'Construction of 6-storey commercial and office complex with basement parking.', '2026-01-15', '2026-12-30', 'Ongoing', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 'Gahanga Logistics Hub Phase 1', 'Kicukiro, Gahanga', 'Development of multi-bay warehousing and heavy vehicle loading docks.', '2026-03-01', '2026-10-15', 'Ongoing', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 'Nyagatare Modern Secondary School', 'Nyagatare District', 'Educational facility expansion including 12 smart classrooms and laboratory block.', '2026-06-01', '2027-02-28', 'Planning', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

INSERT INTO project_managers (id, project_id, user_id, assigned_date, status, created_at, updated_at)
VALUES 
(1, 1, 2, '2026-01-15', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 2, 2, '2026-03-01', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 3, 3, '2026-06-01', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (project_id, user_id) DO NOTHING;

INSERT INTO material_categories (id, category_name, description, created_at, updated_at)
VALUES 
(1, 'Cement & Aggregates', 'Portland cements, river sands, gravel, ballast and crushed stones', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 'Steel & Reinforcement', 'Deformed reinforcement bars, BRC mesh, binding wire, structural steel beams', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 'Masonry & Blocks', 'Hollow concrete blocks, solid masonry bricks, paving stones', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(4, 'Electrical Supplies', 'Copper conduit wiring, switches, distribution boards, breakers, lighting', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(5, 'Plumbing & Drainage', 'PPR pressure pipes, PVC drainage pipes, sanitary fixtures, brass valves', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(6, 'Finishing & Paints', 'Interior wall paints, weather-guard exterior coating, primers, thinners', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (category_name) DO NOTHING;

INSERT INTO materials (id, category_id, material_name, unit, current_price, description, status, created_at, updated_at)
VALUES 
(1, 1, 'Dangote 42.5R Portland Cement (50kg)', 'Bags', 12500.00, 'High-strength structural cement for concrete slabs and columns', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 1, 'Clean River Sand', 'Tons', 18000.00, 'Coarse river sand for high-grade mortar and concrete mixing', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 1, 'Machine-Crushed Aggregate (20mm)', 'Tons', 22000.00, 'Screened blue basalt aggregate for reinforced concrete', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(4, 2, '12mm Deformed High-Yield Rebar (12m)', 'Bars', 14500.00, 'BS4449 Grade 500 rebar for structural reinforcement', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(5, 2, '16mm Deformed High-Yield Rebar (12m)', 'Bars', 26000.00, 'Grade 500 tensile rebar for main foundation footings and columns', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(6, 2, 'Annealed Binding Wire (25kg Roll)', 'Rolls', 35000.00, '16-gauge soft black binding wire for steel tying', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(7, 3, 'Hollow Concrete Blocks (20x20x40cm)', 'Pieces', 850.00, 'Standard load-bearing masonry blocks', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(8, 4, '2.5mm Single Core Copper Cable (100m)', 'Rolls', 48000.00, 'Flame-retardant electrical wiring for power sockets', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(9, 5, 'PVC Pressure Pipe 110mm (6m)', 'Pipes', 28500.00, 'Class 10 heavy-duty drainage and sewer pipe', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(10, 6, 'Sadolin Weather-Shield Exterior White (20L)', 'Buckets', 65000.00, 'Premium acrylic emulsion coating with fungal protection', 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

INSERT INTO worker_types (id, type_name, default_daily_rate, description, created_at, updated_at)
VALUES 
(1, 'General Site Foreman', 15000.00, 'Supervises daily operations, safety, and trade coordination', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 'Master Mason (Fundi)', 10000.00, 'Bricklaying, blockwork, rendering, and plastering work', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 'Certified Steel Fixer', 10000.00, 'Rebar bending, cutting, tying, and structural placement', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(4, 'Licensed Electrician', 12000.00, 'Conduit piping, cable pulling, termination, and panel assembly', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(5, 'Master Plumber', 11000.00, 'Piping installation, pressure testing, and sanitary drainage', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(6, 'Casual Site Worker (Umubyizi)', 5000.00, 'Material offloading, concrete mixing, excavation, site cleanup', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (type_name) DO NOTHING;

INSERT INTO site_workers (id, worker_type_id, full_name, phone, daily_rate, status, created_at, updated_at)
VALUES 
(1, 1, 'Jean Baptiste Karekezi', '+250788200010', 15000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 2, 'Emmanuel Ndungutse', '+250788200011', 10000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 2, 'Innocent Habimana', '+250788200012', 10000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(4, 3, 'Eric Mugisha', '+250788200013', 10000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(5, 4, 'David Hakizimana', '+250788200014', 12000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(6, 5, 'Vincent Bizimana', '+250788200015', 11000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(7, 6, 'Claudine Uwase', '+250788200016', 5000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(8, 6, 'Theogene Nsabimana', '+250788200017', 5000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(9, 6, 'Faustin Gasana', '+250788200018', 5000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(10, 6, 'Marie Claire Mukamana', '+250788200019', 5000.00, 'ACTIVE', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

INSERT INTO project_material_stock (id, project_id, material_id, quantity_available, minimum_quantity, average_unit_price, created_at, updated_at)
VALUES 
(1, 1, 1, 120.00, 30.00, 12500.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 1, 4, 85.00, 20.00, 14500.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 1, 7, 500.00, 150.00, 850.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (project_id, material_id) DO NOTHING;

