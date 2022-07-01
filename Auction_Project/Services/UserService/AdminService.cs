﻿using Auction_Project.DataBase;
using Auction_Project.Models.BannedUsers;
using Microsoft.EntityFrameworkCore;

namespace Auction_Project.Services.UserService
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BannedUser>> GetBannedUsers()
        {
            var result = await _context.BannedUsers.Include(b => b.User).Include(b => b.Admin).ToListAsync();

            if (result.Count > 0)
                return result;

            return null;
        }

        public async Task<BannedUser> GetBannedUser(int id)
        {
            var listOfUsers = await _context.BannedUsers.Include(b => b.User).Include(b => b.Admin).ToListAsync();
            //Console.WriteLine(#)
            var result = listOfUsers.Find(r => r.User.Id == id);

            if (result == null)
                return null;

            return result;
        }

        public async Task<BannedUser> UnbanUser(int id, int adminId)
        {
            var bannedUser = await _context.BannedUsers.FirstOrDefaultAsync(bannedUserId => bannedUserId.User.Id == id);

            if(bannedUser == null)
            {
                return null;
            }

            return bannedUser;

        }

        public async Task<int> GetUnbannedUser(int id)
        {
            var unbannedUser = await _context.Users.FirstOrDefaultAsync(userId => userId.Id == id);

            if (unbannedUser != null)
                return unbannedUser.Id;

            return 0;
        }

        public async Task<int> DeleteBannedUser(int id)
        {
            var findUser = await _context.BannedUsers.FirstOrDefaultAsync(bannedUser => bannedUser.User.Id == id);

            if (findUser != null)
            {
                //Console.WriteLine($"{findUser.User.Id}");
                _context.BannedUsers.Remove(findUser);
                await _context.SaveChangesAsync();
                return id;
            }

            return 0;
        }

        public async Task<BannedUser> AddBannedUser(int id, int adminId)
        {
            var unbannedUser = await _context.Users.FirstOrDefaultAsync(unbannedUser => unbannedUser.Id == id);
            var foundAdmin = await _context.Users.FirstOrDefaultAsync(adminUser => adminUser.Id == adminId);

            if (unbannedUser != null && foundAdmin != null)
            {
                var bannedUser = new BannedUser
                {
                    User = unbannedUser,
                    Admin = foundAdmin,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                };

                await _context.BannedUsers.AddAsync(bannedUser);

                await _context.SaveChangesAsync();
                return bannedUser;
            }

            return null;

        }


    }
}
