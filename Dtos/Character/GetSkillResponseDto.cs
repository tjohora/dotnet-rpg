using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Character
{
    public class GetSkillResponseDto
    {
        public string Name { get; set; } = string.Empty;
        public int Damage { get; set; }
    }
}