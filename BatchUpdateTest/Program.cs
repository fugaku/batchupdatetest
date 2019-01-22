using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace BatchUpdateTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using (var db = new DataContext())
                {
                    Console.WriteLine("Create initial Data");
                    var groups = await db.UserGroups.ToListAsync();
                    if (groups.Any())
                    {
                        db.RemoveRange(groups);
                        await db.SaveChangesAsync();
                    }

                    var userGroups = new List<UserGroup>();
                    userGroups.Add(new UserGroup { Name = "Group 1" });
                    userGroups.Add(new UserGroup { Name = "Group 2" });
                    userGroups.Add(new UserGroup { Name = "Group 3" });

                    db.AddRange(userGroups);
                    await db.SaveChangesAsync();

                    var users = await db.Users.ToListAsync();
                    if (users.Any())
                    {
                        db.RemoveRange(users);
                        await db.SaveChangesAsync();
                    }

                    db.Users.Add(new User { Name = "ABC", GroupId = userGroups[0].Id });
                    db.Users.Add(new User { Name = "DEF", GroupId = userGroups[1].Id });
                    db.Users.Add(new User { Name = "GHI", GroupId = userGroups[2].Id });

                    await db.SaveChangesAsync();

                    Console.WriteLine("Test Batch Update");
                    await db.Users
                        .Join(db.UserGroups,
                            x => x.GroupId,
                            x => x.Id,
                            (user, group) => new User { Id = user.Id, GroupId = group.Id, GroupName = group.Name })
                        .UpdateAsync(x => new User { GroupName = x.GroupName });

                    Console.WriteLine("Check Result");
                    users = await db.Users.ToListAsync();
                    foreach (var user in users)
                    {
                        Console.WriteLine($"Id:{user.Id} Name:{user.Name} GroupId:{user.GroupId} GroupName:{user.GroupName}");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
