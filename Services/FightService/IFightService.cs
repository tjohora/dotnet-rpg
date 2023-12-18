using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;

namespace dotnet_rpg.Services.FightService
{
    public interface IFightService
    {
        Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto weaponAttack);
        Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto skillAttack);
        Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto skillAttack);

    }
}