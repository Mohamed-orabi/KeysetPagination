using System.Diagnostics;
using KeysetPagination;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main(string[] args)
    {
        await SeedDatabase();

        Console.WriteLine("Measuring keyset pagination performance...");
        await MeasureKeysetPagination();

        Console.WriteLine("Measuring offset pagination performance...");
        int totalPages = 100; // Adjust based on the number of pages you want to test
        await MeasureOffsetPagination(totalPages);
    }

    public static async Task SeedDatabase()
    {
        using (var context = new AppDbContext())
        {
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("Database already seeded.");
                return;
            }

            var users = new List<User>();
            var random = new Random();

            for (int i = 1; i <= 1000000; i++)
            {
                users.Add(new User
                {
                    Username = "User" + i,
                    CreatedAt = DateTime.Now.AddSeconds(-random.Next(1000000))
                });

                // Insert in batches of 10,000 to avoid high memory usage
                if (i % 10000 == 0)
                {
                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    users.Clear();
                    Console.WriteLine($"Inserted {i} users.");
                }
            }

            // Insert remaining users if any
            if (users.Count > 0)
            {
                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            Console.WriteLine("Database seeding completed.");
        }
    }

    public static async Task<List<User>> GetUsers(int? lastUserId, int pageSize = 10)
    {
        using (var context = new AppDbContext())
        {
            IQueryable<User> query = context.Users;

            if (lastUserId.HasValue)
            {
                query = query.Where(u => u.Id > lastUserId.Value);
            }

            var users = await query.OrderBy(u => u.Id)
                                   .Take(pageSize)
                                   .ToListAsync();

            return users;
        }
    }

    public static async Task<List<User>> GetUsersOffset(int pageNumber, int pageSize = 10)
    {
        using (var context = new AppDbContext())
        {
            var users = await context.Users
                                     .OrderBy(u => u.Id)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

            return users;
        }
    }

    public static async Task MeasureKeysetPagination()
    {
        int? lastUserId = null;
        int pageSize = 10;
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        while (true)
        {
            var users = await GetUsers(lastUserId, pageSize);

            if (users.Count == 0)
            {
                break;
            }

            lastUserId = users.Last().Id;
        }
        stopwatch.Stop();

        Console.WriteLine($"Keyset pagination took: {stopwatch.ElapsedMilliseconds} ms");
    }

    public static async Task MeasureOffsetPagination(int totalPages, int pageSize = 10)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        for (int i = 1; i <= totalPages; i++)
        {
            var users = await GetUsersOffset(i, pageSize);

            if (users.Count == 0)
            {
                break;
            }
        }
        stopwatch.Stop();

        Console.WriteLine($"Offset pagination took: {stopwatch.ElapsedMilliseconds} ms");
    }
}
