using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class KitchenConfiguration
    {
        public const int DefaultMaxConcurrentDishes = 20;
        public const decimal DefaultFactorMultiplierTime = 0.1m;
        public const decimal DefaultMaxQuantityTimeMultiplier = 2m;

        public int Id { get; set; }
        public int MaxConcurrentDishes { get; set; }
        public decimal FactorMultiplierTime { get; set; }
        public decimal MaxQuantityTimeMultiplier { get; set; }
    }
}
