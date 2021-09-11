using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimePromotionBotReborn.Models
{
    public class HumanType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public System.String Name { get; set; }

        public System.String Template { get; set; }

        public virtual ICollection<Channel> Channels { get; set; }
    }
}
