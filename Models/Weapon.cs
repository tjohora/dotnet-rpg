using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Damage { get; set; }
        public Character? Character { get; set; }
        // Given this naming convention, EF knows this is pointing to the Model Character using Id
        public int CharacterId { get; set; }
    }
}