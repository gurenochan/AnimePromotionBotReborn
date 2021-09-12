using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AnimePromotionBotReborn.Models
{
    public class Channel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Tid { get; set; }

        public bool IsActive { get; set; }

        public bool EnablePromotion { get; set; }

        public int HumanTypeId { get; set; }

        public virtual HumanType HumanType { get; set; }
    }
}
