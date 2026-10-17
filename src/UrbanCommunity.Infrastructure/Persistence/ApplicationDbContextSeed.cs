using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    private const int BatchSize = 10;

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILoggerFactory loggerFactory,
        string contentRootPath,
        bool seedDevelopmentAdmin)
    {
        var logger = loggerFactory.CreateLogger<ApplicationDbContextSeed>();

        try
        {
            if (seedDevelopmentAdmin)
            {
                await SeedDevelopmentAdminAsync(userManager, logger);
            }

            await SeedReportCategoriesAsync(context);

            if (await context.Provinces.AnyAsync())
                return;

            // Published output (Docker): file is copied next to the assembly
            var publishedPath = Path.Combine(contentRootPath, "Persistence", "SeedData", "locations.json");
            // Development: file lives in the Infrastructure source project
            var devPath = Path.Combine(contentRootPath, "..", "UrbanCommunity.Infrastructure", "Persistence", "SeedData", "locations.json");

            var seedDataPath = File.Exists(publishedPath) ? publishedPath : devPath;

            if (!File.Exists(seedDataPath))
            {
                logger.LogWarning("Seed data file not found: {Path}", seedDataPath);
                return;
            }

            logger.LogInformation("Reading location seed data from {Path}...", seedDataPath);

            var jsonData = await File.ReadAllTextAsync(seedDataPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(jsonData);

            if (data is null)
            {
                logger.LogWarning("Seed data is empty or could not be parsed.");
                return;
            }

            var allProvinces = new List<Province>();

            foreach (var (provinceName, districtMap) in data)
            {
                var province = new Province
                {
                    Name = provinceName
                };

                foreach (var (districtName, neighborhoodNames) in districtMap)
                {
                    var district = new District
                    {
                        Name = districtName
                    };

                    foreach (var neighborhoodName in neighborhoodNames.Distinct())
                    {
                        district.Neighborhoods.Add(new Neighborhood
                        {
                            Name = neighborhoodName,
                            District = districtName,
                            City = provinceName
                        });
                    }

                    province.Districts.Add(district);
                }

                allProvinces.Add(province);
            }

            // Save in batches to avoid large transactions
            for (int i = 0; i < allProvinces.Count; i += BatchSize)
            {
                var batch = allProvinces.Skip(i).Take(BatchSize).ToList();
                await context.Provinces.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded provinces {From}-{To} of {Total}...", i + 1, Math.Min(i + BatchSize, allProvinces.Count), allProvinces.Count);
            }

            var totalDistricts = allProvinces.Sum(p => p.Districts.Count);
            var totalNeighborhoods = allProvinces.Sum(p => p.Districts.Sum(d => d.Neighborhoods.Count));

            logger.LogInformation(
                "Location seed completed: {Provinces} provinces, {Districts} districts, {Neighborhoods} neighborhoods.",
                allProvinces.Count, totalDistricts, totalNeighborhoods);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }

    private static async Task SeedDevelopmentAdminAsync(
        UserManager<User> userManager,
        ILogger logger)
    {
        var email = Environment.GetEnvironmentVariable("DEV_ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("DEV_ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "DEV_ADMIN_EMAIL or DEV_ADMIN_PASSWORD is not set. Development admin seeding was skipped.");
            return;
        }

        var firstName = Environment.GetEnvironmentVariable("DEV_ADMIN_FIRST_NAME");
        var lastName = Environment.GetEnvironmentVariable("DEV_ADMIN_LAST_NAME");
        firstName = string.IsNullOrWhiteSpace(firstName) ? "Admin" : firstName;
        lastName = string.IsNullOrWhiteSpace(lastName) ? "User" : lastName;

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                Role = UserRole.Admin,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(
                admin,
                password);

            EnsureIdentitySucceeded(
                createResult,
                "Development admin user could not be created.");

            logger.LogInformation(
                "Development admin user was created: {Email}",
                email);
            return;
        }

        admin.FirstName = firstName;
        admin.LastName = lastName;
        admin.Email = email;
        admin.UserName = email;
        admin.EmailConfirmed = true;
        admin.Role = UserRole.Admin;
        admin.IsActive = true;
        admin.UpdateAccess(UserRole.Admin);

        var updateResult = await userManager.UpdateAsync(admin);
        EnsureIdentitySucceeded(
            updateResult,
            "Development admin user could not be updated.");

        if (!await userManager.CheckPasswordAsync(
                admin,
                password))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
            var resetResult = await userManager.ResetPasswordAsync(
                admin,
                resetToken,
                password);

            EnsureIdentitySucceeded(
                resetResult,
                "Development admin password could not be reset.");
        }
    }

    private static void EnsureIdentitySucceeded(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join(
            "; ",
            result.Errors.Select(error => error.Description));

        throw new InvalidOperationException($"{message} {errors}");
    }

    private static async Task SeedReportCategoriesAsync(ApplicationDbContext context)
    {
        var defaultCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Yol ve Kaldırım"] = "Yol yüzeyi, çukur, kaldırım, bordür ve yaya yolu sorunları.",
            ["Aydınlatma ve Elektrik"] = "Sokak aydınlatması ve elektrik kaynaklı sorunlar.",
            ["Su ve Kanalizasyon"] = "Su kesintisi, kaçak, kanalizasyon ve drenaj sorunları.",
            ["Temizlik ve Atık"] = "Çöp, geri dönüşüm ve çevre temizliği sorunları.",
            ["Park ve Yeşil Alan"] = "Park, oyun alanı, ağaç ve yeşil alan sorunları.",
            ["Trafik ve Sinyalizasyon"] = "Trafik işaretleri, yol çizgileri ve sinyalizasyon sorunları.",
            ["Gürültü ve Hava Kirliliği"] = "Gürültü, kötü koku, duman ve hava kirliliği sorunları.",
            ["Sokak Hayvanları ve Veterinerlik"] = "Sahipsiz veya yaralı hayvanlar, barınak ve veterinerlik hizmetleri.",
            ["Erişilebilirlik"] = "Engelli rampası, hissedilebilir yüzey ve erişimi engelleyen geçiş sorunları.",
            ["Toplu Taşıma"] = "Otobüs durağı, güzergâh, sefer ve toplu taşıma hizmeti sorunları.",
            ["Otopark ve Hatalı Park"] = "Otopark alanları, engelli park yerleri, kaldırım üstü park ve geçişi engelleyen araçlarla ilgili sorunlar.",
            ["Yapı ve İmar Güvenliği"] = "Tehlikeli veya terk edilmiş yapılar, izinsiz inşaat ve yapı güvenliği riskleri.",
            ["Haşere ve İlaçlama"] = "Sivrisinek, fare, böcek ve ilaçlama talepleri.",
            ["Kar ve Buzlanma"] = "Kar küreme, yol ve kaldırım tuzlama, buzlanma ve kar nedeniyle kapanan ulaşım yolları.",
            ["Afet ve Acil Riskler"] = "Sel, heyelan, yangın, çökme ve devrilme riski taşıyan acil durumlar.",
            ["Diğer"] = "Diğer kategorilere uymayan veya sınıflandırılamayan sorunlar."
        };

        var existingCategories = await context.ReportCategories.ToListAsync();

        foreach (var (name, description) in defaultCategories)
        {
            var existingCategory = existingCategories.FirstOrDefault(category =>
                string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase));

            if (existingCategory is not null)
            {
                existingCategory.Description = description;
                continue;
            }

            var category = new ReportCategory
            {
                Name = name,
                Description = description
            };

            context.ReportCategories.Add(category);
            existingCategories.Add(category);
        }

        await context.SaveChangesAsync();
    }
}
