using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AnimePromotionBotReborn.Models;

namespace AnimePromotionBotReborn
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) :base(options) { }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<HumanType> HumanTypes { get; set; }
    }
}
