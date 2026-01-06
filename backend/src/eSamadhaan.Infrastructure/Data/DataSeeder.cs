using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eSamadhaan.Infrastructure.Data;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DataSeeder>? _logger;

    public DataSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger<DataSeeder>? logger = null)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger?.LogInformation("Starting database seeding...");

        try
        {
            // Ensure database exists (schema should already be created via migrations)
            // This will only create the database if it doesn't exist
            if (!await _context.Database.CanConnectAsync())
            {
                _logger?.LogWarning("Cannot connect to database. Ensure migrations are applied first.");
                throw new InvalidOperationException("Cannot connect to database. Please run migrations first: dotnet ef database update");
            }

            // Seed in order respecting foreign key constraints
            // Save after each step to ensure data is available for subsequent queries
            await SeedDepartmentsAsync();
            await _context.SaveChangesAsync();
            
            await SeedUsersAsync();
            await _context.SaveChangesAsync();
            
            await SeedCategoriesAsync();
            await _context.SaveChangesAsync();
            
            await SeedGrievancesAsync();
            await _context.SaveChangesAsync();
            
            await SeedAssignmentsAsync();
            await _context.SaveChangesAsync();
            
            await SeedStatusHistoryAsync();
            await _context.SaveChangesAsync();
            
            await SeedResolutionsAsync();
            await _context.SaveChangesAsync();
            
            await SeedFeedbacksAsync();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred during database seeding.");
            throw;
        }
    }

    private async Task SeedDepartmentsAsync()
    {
        if (await _context.Departments.AnyAsync())
        {
            _logger?.LogInformation("Departments already exist. Skipping department seeding.");
            return;
        }

        var departments = new List<Department>
        {
            new Department
            {
                Name = "Public Works Department",
                Description = "Responsible for construction and maintenance of public infrastructure including roads, bridges, and government buildings."
            },
            new Department
            {
                Name = "Water Supply & Sanitation",
                Description = "Manages water supply systems, sewage treatment, and sanitation services for urban and rural areas."
            },
            new Department
            {
                Name = "Electricity Board",
                Description = "Oversees power generation, distribution, and maintenance of electrical infrastructure."
            },
            new Department
            {
                Name = "Municipal Corporation",
                Description = "Handles urban planning, waste management, property tax collection, and civic amenities."
            },
            new Department
            {
                Name = "Health Department",
                Description = "Manages public health services, hospitals, clinics, and health programs."
            },
            new Department
            {
                Name = "Education Department",
                Description = "Oversees public schools, colleges, and educational programs and policies."
            },
            new Department
            {
                Name = "Transport Department",
                Description = "Manages public transportation, vehicle registration, and traffic management."
            },
            new Department
            {
                Name = "Revenue Department",
                Description = "Handles land records, property registration, and revenue collection."
            }
        };

        await _context.Departments.AddRangeAsync(departments);
        _logger?.LogInformation($"Seeded {departments.Count} departments.");
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger?.LogInformation("Users already exist. Skipping user seeding.");
            return;
        }

        // Get departments from database (they should be saved by now)
        var departments = await _context.Departments.ToListAsync();
        if (!departments.Any())
        {
            // Also check change tracker as fallback
            var trackedDepartments = _context.ChangeTracker.Entries<Department>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();
            
            if (!trackedDepartments.Any())
            {
                throw new InvalidOperationException(
                    "Departments must be seeded before users. " +
                    "Ensure SeedDepartmentsAsync() completes successfully and changes are saved.");
            }
            
            departments = trackedDepartments;
            _logger?.LogWarning("Using departments from change tracker. Consider saving departments before seeding users.");
        }

        var defaultPassword = "Password123!"; // Development password
        var passwordHash = _passwordHasher.HashPassword(defaultPassword);

        var users = new List<User>
        {
            // System Admin
            new User
            {
                Name = "Admin User",
                Email = "admin@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "SystemAdmin",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            // Supervisory Officers (one per department)
            new User
            {
                Name = "Rajesh Kumar",
                Email = "supervisor.pwd@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "SupervisoryOfficer",
                DepartmentId = departments[0].Id, // Public Works
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new User
            {
                Name = "Priya Sharma",
                Email = "supervisor.water@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "SupervisoryOfficer",
                DepartmentId = departments[1].Id, // Water Supply
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new User
            {
                Name = "Amit Patel",
                Email = "supervisor.electricity@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "SupervisoryOfficer",
                DepartmentId = departments[2].Id, // Electricity
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new User
            {
                Name = "Sneha Reddy",
                Email = "supervisor.municipal@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "SupervisoryOfficer",
                DepartmentId = departments[3].Id, // Municipal
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            // Department Officers (multiple per department)
            new User
            {
                Name = "Vikram Singh",
                Email = "officer.pwd1@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Name = "Anjali Mehta",
                Email = "officer.pwd2@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[0].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Name = "Rohit Verma",
                Email = "officer.water1@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[1].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Name = "Kavita Nair",
                Email = "officer.electricity1@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[2].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Name = "Manoj Joshi",
                Email = "officer.municipal1@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[3].Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Name = "Deepak Iyer",
                Email = "officer.health1@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[4].Id, // Health
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Name = "Sunita Desai",
                Email = "officer.education1@esamadhaan.test",
                PasswordHash = passwordHash,
                Role = "DepartmentOfficer",
                DepartmentId = departments[5].Id, // Education
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            // Citizens (multiple)
            new User
            {
                Name = "Ramesh Kumar",
                Email = "citizen.ramesh@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new User
            {
                Name = "Lakshmi Devi",
                Email = "citizen.lakshmi@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new User
            {
                Name = "Mohammed Ali",
                Email = "citizen.mohammed@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new User
            {
                Name = "Geeta Sharma",
                Email = "citizen.geeta@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new User
            {
                Name = "Suresh Reddy",
                Email = "citizen.suresh@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new User
            {
                Name = "Anita Patel",
                Email = "citizen.anita@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new User
            {
                Name = "Kiran Nair",
                Email = "citizen.kiran@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new User
            {
                Name = "Vijay Mehta",
                Email = "citizen.vijay@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Name = "Pooja Iyer",
                Email = "citizen.pooja@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Name = "Arjun Singh",
                Email = "citizen.arjun@test.com",
                PasswordHash = passwordHash,
                Role = "Citizen",
                DepartmentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        await _context.Users.AddRangeAsync(users);
        _logger?.LogInformation($"Seeded {users.Count} users.");
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.GrievanceCategories.AnyAsync())
        {
            _logger?.LogInformation("Categories already exist. Skipping category seeding.");
            return;
        }

        var departments = await _context.Departments.ToListAsync();
        if (!departments.Any())
        {
            throw new InvalidOperationException(
                "Departments must be seeded before categories. " +
                "Ensure SeedDepartmentsAsync() and SeedUsersAsync() complete successfully and changes are saved.");
        }

        var categories = new List<GrievanceCategory>
        {
            // Public Works Department categories
            new GrievanceCategory
            {
                Name = "Road Repair",
                Description = "Issues related to potholes, road damage, and road maintenance",
                DepartmentId = departments[0].Id
            },
            new GrievanceCategory
            {
                Name = "Bridge Maintenance",
                Description = "Complaints about bridge conditions and safety issues",
                DepartmentId = departments[0].Id
            },
            new GrievanceCategory
            {
                Name = "Street Lighting",
                Description = "Non-functional street lights and lighting infrastructure",
                DepartmentId = departments[0].Id
            },
            // Water Supply categories
            new GrievanceCategory
            {
                Name = "Water Supply Interruption",
                Description = "Complaints about irregular or no water supply",
                DepartmentId = departments[1].Id
            },
            new GrievanceCategory
            {
                Name = "Water Quality Issues",
                Description = "Concerns about water quality, contamination, or taste",
                DepartmentId = departments[1].Id
            },
            new GrievanceCategory
            {
                Name = "Sewage Problems",
                Description = "Blocked drains, overflow, and sewage system issues",
                DepartmentId = departments[1].Id
            },
            // Electricity Board categories
            new GrievanceCategory
            {
                Name = "Power Outage",
                Description = "Unplanned power cuts and electricity interruptions",
                DepartmentId = departments[2].Id
            },
            new GrievanceCategory
            {
                Name = "Billing Disputes",
                Description = "Issues with electricity bills, meter readings, and charges",
                DepartmentId = departments[2].Id
            },
            new GrievanceCategory
            {
                Name = "Electrical Safety",
                Description = "Hanging wires, exposed connections, and safety hazards",
                DepartmentId = departments[2].Id
            },
            // Municipal Corporation categories
            new GrievanceCategory
            {
                Name = "Waste Management",
                Description = "Garbage collection, disposal, and waste management issues",
                DepartmentId = departments[3].Id
            },
            new GrievanceCategory
            {
                Name = "Property Tax",
                Description = "Property tax assessment, billing, and payment issues",
                DepartmentId = departments[3].Id
            },
            new GrievanceCategory
            {
                Name = "Building Permits",
                Description = "Delays or issues with building plan approvals and permits",
                DepartmentId = departments[3].Id
            },
            // Health Department categories
            new GrievanceCategory
            {
                Name = "Hospital Services",
                Description = "Issues with hospital facilities, staff, and services",
                DepartmentId = departments[4].Id
            },
            new GrievanceCategory
            {
                Name = "Health Programs",
                Description = "Complaints about public health programs and initiatives",
                DepartmentId = departments[4].Id
            },
            // Education Department categories
            new GrievanceCategory
            {
                Name = "School Infrastructure",
                Description = "Issues with school buildings, facilities, and amenities",
                DepartmentId = departments[5].Id
            },
            new GrievanceCategory
            {
                Name = "Admission Issues",
                Description = "Problems with school admissions and enrollment",
                DepartmentId = departments[5].Id
            }
        };

        await _context.GrievanceCategories.AddRangeAsync(categories);
        _logger?.LogInformation($"Seeded {categories.Count} categories.");
    }

    private async Task SeedGrievancesAsync()
    {
        if (await _context.Grievances.AnyAsync())
        {
            _logger?.LogInformation("Grievances already exist. Skipping grievance seeding.");
            return;
        }

        var citizens = await _context.Users.Where(u => u.Role == "Citizen").ToListAsync();
        var categories = await _context.GrievanceCategories.Include(c => c.Department).ToListAsync();
        var departments = await _context.Departments.ToListAsync();

        if (!citizens.Any())
        {
            throw new InvalidOperationException(
                "Citizen users must be seeded before grievances. " +
                "Ensure SeedUsersAsync() completes successfully and changes are saved.");
        }
        
        if (!categories.Any())
        {
            throw new InvalidOperationException(
                "Categories must be seeded before grievances. " +
                "Ensure SeedCategoriesAsync() completes successfully and changes are saved.");
        }

        var random = new Random(42); // Fixed seed for reproducibility
        var grievances = new List<Grievance>();
        var baseDate = DateTime.UtcNow.AddMonths(-3);

        // Generate grievances with various statuses
        for (int i = 0; i < 50; i++)
        {
            var category = categories[random.Next(categories.Count)];
            var citizen = citizens[random.Next(citizens.Count)];
            var createdDate = baseDate.AddDays(random.Next(90));
            var status = GetRandomStatus(random, i);

            var grievanceNumber = $"GRV-{createdDate:yyyyMMdd}-{i + 1:D4}";

            var grievance = new Grievance
            {
                GrievanceNumber = grievanceNumber,
                CitizenId = citizen.Id,
                CategoryId = category.Id,
                DepartmentId = category.DepartmentId,
                Description = GenerateGrievanceDescription(category.Name, random),
                AttachmentUrl = random.Next(10) < 3 ? $"https://example.com/attachments/{grievanceNumber}.pdf" : null,
                CurrentStatus = status,
                CreatedAt = createdDate,
                UpdatedAt = createdDate.AddDays(random.Next(0, 30)),
                IsEscalated = random.Next(10) < 2, // 20% escalated
                EscalatedAt = random.Next(10) < 2 ? createdDate.AddDays(random.Next(1, 20)) : null,
                EscalationReason = random.Next(10) < 2 ? "Delayed response from assigned officer" : null
            };

            grievances.Add(grievance);
        }

        await _context.Grievances.AddRangeAsync(grievances);
        _logger?.LogInformation($"Seeded {grievances.Count} grievances.");
    }

    private GrievanceStatus GetRandomStatus(Random random, int index)
    {
        // Distribute statuses to test various scenarios
        var statuses = new[]
        {
            GrievanceStatus.Submitted,  // 20%
            GrievanceStatus.Assigned,    // 25%
            GrievanceStatus.InReview,    // 25%
            GrievanceStatus.Resolved,    // 20%
            GrievanceStatus.Closed       // 10%
        };

        // Ensure we have at least some of each status
        if (index < 10) return statuses[index % statuses.Length];
        return statuses[random.Next(statuses.Length)];
    }

    private string GenerateGrievanceDescription(string categoryName, Random random)
    {
        var descriptions = new Dictionary<string, string[]>
        {
            ["Road Repair"] = new[]
            {
                "Large pothole on Main Street causing vehicle damage. Needs immediate attention.",
                "Road surface completely damaged after recent rains. Multiple vehicles stuck.",
                "Cracked road near school entrance poses safety risk to children."
            },
            ["Water Supply Interruption"] = new[]
            {
                "No water supply for the past 5 days. Affecting entire neighborhood.",
                "Irregular water supply - only available for 2 hours in the morning.",
                "Water pressure is extremely low, unable to fill overhead tank."
            },
            ["Power Outage"] = new[]
            {
                "Frequent power cuts in the area, 4-5 times daily. Affecting work from home.",
                "Power outage for 12 hours yesterday. No prior notification.",
                "Voltage fluctuations damaging electrical appliances."
            },
            ["Waste Management"] = new[]
            {
                "Garbage not collected for past week. Piling up and causing health issues.",
                "Garbage truck not coming to our street. Need regular collection.",
                "Dumping site near residential area causing foul smell and health problems."
            }
        };

        if (descriptions.TryGetValue(categoryName, out var categoryDescriptions))
        {
            return categoryDescriptions[random.Next(categoryDescriptions.Length)];
        }

        return $"Issue related to {categoryName}. Requires urgent attention from the department.";
    }

    private async Task SeedAssignmentsAsync()
    {
        if (await _context.GrievanceAssignments.AnyAsync())
        {
            _logger?.LogInformation("Assignments already exist. Skipping assignment seeding.");
            return;
        }

        // All grievances that are Assigned or beyond must have an assignment
        var grievances = await _context.Grievances
            .Where(g => g.CurrentStatus >= GrievanceStatus.Assigned)
            .ToListAsync();
        var officers = await _context.Users
            .Where(u => u.Role == "DepartmentOfficer" || u.Role == "SupervisoryOfficer")
            .ToListAsync();

        if (!grievances.Any() || !officers.Any())
        {
            _logger?.LogWarning("No grievances or officers available for assignment. Skipping.");
            return;
        }

        var random = new Random(42);
        var assignments = new List<GrievanceAssignment>();

        foreach (var grievance in grievances)
        {
            // Assign officers from the same department
            var departmentOfficers = officers
                .Where(o => o.DepartmentId == grievance.DepartmentId)
                .ToList();

            if (!departmentOfficers.Any())
            {
                _logger?.LogWarning($"No officers found for department {grievance.DepartmentId}. Skipping assignment for grievance {grievance.Id}.");
                continue;
            }

            var assignedOfficer = departmentOfficers[random.Next(departmentOfficers.Count)];
            // Assignment should happen when status changes to Assigned
            var assignedAt = grievance.CreatedAt.AddDays(random.Next(1, 3));

            assignments.Add(new GrievanceAssignment
            {
                GrievanceId = grievance.Id,
                OfficerId = assignedOfficer.Id,
                AssignedAt = assignedAt,
                IsActive = grievance.CurrentStatus != GrievanceStatus.Closed
            });
        }

        await _context.GrievanceAssignments.AddRangeAsync(assignments);
        _logger?.LogInformation($"Seeded {assignments.Count} assignments.");
    }

    private async Task SeedStatusHistoryAsync()
    {
        if (await _context.GrievanceStatusHistories.AnyAsync())
        {
            _logger?.LogInformation("Status history already exists. Skipping status history seeding.");
            return;
        }

        var grievances = await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Assignments)
            .ThenInclude(a => a.Officer)
            .ToListAsync();
        var officers = await _context.Users
            .Where(u => u.Role == "DepartmentOfficer" || u.Role == "SupervisoryOfficer" || u.Role == "SystemAdmin")
            .ToListAsync();

        if (!grievances.Any())
        {
            _logger?.LogWarning("No grievances available for status history. Skipping.");
            return;
        }

        var random = new Random(42);
        var statusHistories = new List<GrievanceStatusHistory>();

        foreach (var grievance in grievances)
        {
            // ALL grievances must have an initial "Submitted" entry in status history
            // oldStatus = Submitted, newStatus = Submitted, changedBy = Citizen, remarks = null
            statusHistories.Add(new GrievanceStatusHistory
            {
                GrievanceId = grievance.Id,
                OldStatus = GrievanceStatus.Submitted,
                NewStatus = GrievanceStatus.Submitted,
                ChangedByUserId = grievance.CitizenId,
                ChangedAt = grievance.CreatedAt,
                Remarks = null
            });

            // Enforce strict lifecycle: Submitted -> Assigned -> InReview -> Resolved -> Closed
            var statuses = new List<GrievanceStatus> { GrievanceStatus.Submitted };
            var currentDate = grievance.CreatedAt;

            // Build complete status progression based on current status
            if (grievance.CurrentStatus >= GrievanceStatus.Assigned)
            {
                statuses.Add(GrievanceStatus.Assigned);
                currentDate = currentDate.AddDays(random.Next(1, 3));
            }

            if (grievance.CurrentStatus >= GrievanceStatus.InReview)
            {
                statuses.Add(GrievanceStatus.InReview);
                currentDate = currentDate.AddDays(random.Next(2, 7));
            }

            if (grievance.CurrentStatus >= GrievanceStatus.Resolved)
            {
                statuses.Add(GrievanceStatus.Resolved);
                currentDate = currentDate.AddDays(random.Next(1, 5));
            }

            if (grievance.CurrentStatus == GrievanceStatus.Closed)
            {
                statuses.Add(GrievanceStatus.Closed);
                currentDate = currentDate.AddDays(random.Next(1, 3));
            }

            // Create history entries for each status transition
            for (int i = 0; i < statuses.Count - 1; i++)
            {
                var oldStatus = statuses[i];
                var newStatus = statuses[i + 1];

                // Determine who changed the status based on the transition
                User changedBy;
                string? remarks = null;

                if (oldStatus == GrievanceStatus.Submitted && newStatus == GrievanceStatus.Assigned)
                {
                    // Supervisor or admin assigns
                    var assignment = grievance.Assignments.FirstOrDefault();
                    if (assignment != null)
                    {
                        // Find the supervisor who likely made the assignment
                        changedBy = officers.FirstOrDefault(o => 
                            o.DepartmentId == grievance.DepartmentId && 
                            o.Role == "SupervisoryOfficer") 
                            ?? officers.FirstOrDefault(o => o.Role == "SystemAdmin")
                            ?? officers.First();
                        
                        // Remark: "Grievance assigned to {Officer Name}"
                        if (assignment.Officer != null)
                        {
                            remarks = $"Grievance assigned to {assignment.Officer.Name}";
                        }
                        else
                        {
                            // Fallback to officers list if navigation property not loaded
                            var assignedOfficer = officers.FirstOrDefault(o => o.Id == assignment.OfficerId);
                            remarks = assignedOfficer != null 
                                ? $"Grievance assigned to {assignedOfficer.Name}"
                                : "Grievance assigned to officer";
                        }
                    }
                    else
                    {
                        // Fallback if assignment doesn't exist yet
                        changedBy = officers.FirstOrDefault(o => 
                            o.DepartmentId == grievance.DepartmentId && 
                            o.Role == "SupervisoryOfficer") 
                            ?? officers.FirstOrDefault(o => o.Role == "SystemAdmin")
                            ?? officers.First();
                        remarks = "Grievance assigned to officer";
                    }
                }
                else if (oldStatus == GrievanceStatus.Assigned && newStatus == GrievanceStatus.InReview)
                {
                    // Officer updates to InReview - NO REMARKS
                    var assignment = grievance.Assignments.FirstOrDefault();
                    if (assignment != null)
                    {
                        changedBy = officers.FirstOrDefault(o => o.Id == assignment.OfficerId) 
                            ?? officers.First();
                    }
                    else
                    {
                        changedBy = officers.FirstOrDefault(o => 
                            o.DepartmentId == grievance.DepartmentId) 
                            ?? officers.First();
                    }
                    remarks = null; // No remarks for InReview
                }
                else if (oldStatus == GrievanceStatus.InReview && newStatus == GrievanceStatus.Resolved)
                {
                    // Officer resolves - generate remark that will be used for resolution
                    var assignment = grievance.Assignments.FirstOrDefault();
                    if (assignment != null)
                    {
                        changedBy = officers.FirstOrDefault(o => o.Id == assignment.OfficerId) 
                            ?? officers.First();
                    }
                    else
                    {
                        changedBy = officers.FirstOrDefault(o => 
                            o.DepartmentId == grievance.DepartmentId) 
                            ?? officers.First();
                    }
                    // Generate resolution remark
                    remarks = GenerateResolutionRemarks(grievance, random);
                }
                else if (oldStatus == GrievanceStatus.Resolved && newStatus == GrievanceStatus.Closed)
                {
                    // Officer closes - NO REMARKS
                    var assignment = grievance.Assignments.FirstOrDefault();
                    if (assignment != null)
                    {
                        changedBy = officers.FirstOrDefault(o => o.Id == assignment.OfficerId) 
                            ?? officers.First();
                    }
                    else
                    {
                        changedBy = officers.FirstOrDefault(o => 
                            o.DepartmentId == grievance.DepartmentId) 
                            ?? officers.First();
                    }
                    remarks = null; // No remarks for Closed
                }
                else
                {
                    // Fallback for any other transitions
                    var assignment = grievance.Assignments.FirstOrDefault();
                    if (assignment != null)
                    {
                        changedBy = officers.FirstOrDefault(o => o.Id == assignment.OfficerId) 
                            ?? officers.First();
                    }
                    else
                    {
                        changedBy = officers.FirstOrDefault(o => 
                            o.DepartmentId == grievance.DepartmentId) 
                            ?? officers.First();
                    }
                }

                statusHistories.Add(new GrievanceStatusHistory
                {
                    GrievanceId = grievance.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedByUserId = changedBy.Id,
                    ChangedAt = currentDate,
                    Remarks = remarks
                });

                // Increment date for next transition
                if (i < statuses.Count - 2) // Don't increment after the last transition
                {
                    currentDate = currentDate.AddDays(random.Next(1, 5));
                }
            }
        }

        await _context.GrievanceStatusHistories.AddRangeAsync(statusHistories);
        _logger?.LogInformation($"Seeded {statusHistories.Count} status history entries.");
    }

    // This method is no longer used - remarks are now generated inline in SeedStatusHistoryAsync
    // Keeping for reference but can be removed if needed

    private async Task SeedResolutionsAsync()
    {
        if (await _context.GrievanceResolutions.AnyAsync())
        {
            _logger?.LogInformation("Resolutions already exist. Skipping resolution seeding.");
            return;
        }

        // ALL grievances with status Resolved or Closed MUST have a resolution
        var resolvedGrievances = await _context.Grievances
            .Where(g => g.CurrentStatus == GrievanceStatus.Resolved || g.CurrentStatus == GrievanceStatus.Closed)
            .Include(g => g.Assignments)
            .ToListAsync();

        if (!resolvedGrievances.Any())
        {
            _logger?.LogWarning("No resolved grievances available. Skipping resolution seeding.");
            return;
        }

        var random = new Random(42);
        var resolutions = new List<GrievanceResolution>();
        var officers = await _context.Users
            .Where(u => u.Role == "DepartmentOfficer" || u.Role == "SupervisoryOfficer")
            .ToListAsync();

        foreach (var grievance in resolvedGrievances)
        {
            // Get the assigned officer, or find a fallback officer from the same department
            var assignment = grievance.Assignments.FirstOrDefault();
            int resolvedByOfficerId;

            if (assignment != null)
            {
                resolvedByOfficerId = assignment.OfficerId;
            }
            else
            {
                // Fallback: find any officer from the same department
                var departmentOfficer = officers.FirstOrDefault(o => o.DepartmentId == grievance.DepartmentId);
                if (departmentOfficer == null)
                {
                    _logger?.LogWarning($"No officer found for grievance {grievance.Id}. Skipping resolution.");
                    continue;
                }
                resolvedByOfficerId = departmentOfficer.Id;
                _logger?.LogWarning($"Grievance {grievance.Id} has no assignment, using fallback officer {resolvedByOfficerId}.");
            }

            // ResolvedAt should be when status changed to Resolved
            // Get the date and remark from status history (seeded before this step)
            var statusHistory = await _context.GrievanceStatusHistories
                .Where(sh => sh.GrievanceId == grievance.Id && sh.NewStatus == GrievanceStatus.Resolved)
                .OrderByDescending(sh => sh.ChangedAt)
                .FirstOrDefaultAsync();

            var resolvedAt = statusHistory?.ChangedAt ?? 
                (grievance.CurrentStatus == GrievanceStatus.Closed
                    ? grievance.UpdatedAt.AddDays(-random.Next(1, 3))
                    : grievance.UpdatedAt);

            // Resolution remarks must be the same as the remark when moved to Resolved status
            var resolutionRemarks = statusHistory?.Remarks;
            if (string.IsNullOrEmpty(resolutionRemarks))
            {
                // Fallback if status history remark is missing
                resolutionRemarks = GenerateResolutionRemarks(grievance, random);
                _logger?.LogWarning($"Status history remark missing for grievance {grievance.Id}, using generated remark.");
            }

            resolutions.Add(new GrievanceResolution
            {
                GrievanceId = grievance.Id,
                ResolvedByOfficerId = resolvedByOfficerId,
                ResolutionRemarks = resolutionRemarks,
                ResolvedAt = resolvedAt
            });
        }

        if (resolutions.Count != resolvedGrievances.Count)
        {
            _logger?.LogWarning($"Created {resolutions.Count} resolutions for {resolvedGrievances.Count} resolved grievances. Some may be missing assignments.");
        }

        await _context.GrievanceResolutions.AddRangeAsync(resolutions);
        _logger?.LogInformation($"Seeded {resolutions.Count} resolutions (all resolved/closed grievances must have resolutions).");
    }

    private string GenerateResolutionRemarks(Grievance grievance, Random random)
    {
        var remarks = new[]
        {
            "Issue has been addressed. Required repairs completed.",
            "Problem resolved. Regular monitoring will be maintained.",
            "Action taken as per complaint. Situation normalized.",
            "Issue fixed. Preventive measures implemented.",
            "Complaint resolved. Citizen notified of the resolution.",
            "Work completed successfully. Quality verified.",
            "Issue resolved. Follow-up scheduled to ensure sustainability."
        };

        return remarks[random.Next(remarks.Length)];
    }

    private async Task SeedFeedbacksAsync()
    {
        if (await _context.Feedbacks.AnyAsync())
        {
            _logger?.LogInformation("Feedbacks already exist. Skipping feedback seeding.");
            return;
        }

        var closedGrievances = await _context.Grievances
            .Where(g => g.CurrentStatus == GrievanceStatus.Closed)
            .ToListAsync();

        if (!closedGrievances.Any())
        {
            _logger?.LogWarning("No closed grievances available. Skipping feedback seeding.");
            return;
        }

        var random = new Random(42);
        var feedbacks = new List<Feedback>();

        // Not all closed grievances have feedback (about 70%)
        var grievancesWithFeedback = closedGrievances
            .Where((_, index) => random.Next(10) < 7)
            .ToList();

        foreach (var grievance in grievancesWithFeedback)
        {
            var rating = random.Next(1, 6); // 1-5 rating
            var comments = new[]
            {
                "Very satisfied with the resolution. Thank you!",
                "Good response time. Issue resolved satisfactorily.",
                "Could be better. Some delays in response.",
                "Excellent service. Quick resolution.",
                "Satisfactory. Would appreciate faster response next time.",
                "Issue resolved but took longer than expected.",
                "Very happy with the outcome. Keep up the good work!",
                "Average service. Room for improvement.",
                "Outstanding support. Highly recommend.",
                "Issue addressed but needs follow-up."
            };

            feedbacks.Add(new Feedback
            {
                GrievanceId = grievance.Id,
                Rating = rating,
                Comment = comments[random.Next(comments.Length)],
                SubmittedAt = grievance.UpdatedAt.AddDays(random.Next(1, 7))
            });
        }

        await _context.Feedbacks.AddRangeAsync(feedbacks);
        _logger?.LogInformation($"Seeded {feedbacks.Count} feedbacks.");
    }
}

