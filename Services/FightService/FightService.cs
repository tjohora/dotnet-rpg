using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        public FightService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            var response = new ServiceResponse<FightResultDto>()
            {
                Data = new FightResultDto()
            };

            try
            {
                var characters = await _dbContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .Where(c => request.CharacterIds.Contains(c.Id))
                    .ToListAsync();

                bool defeated = false;
                while (!defeated)
                {
                    foreach (var attacker in characters)
                    {
                        var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useweapon = new Random().Next(2) == 0;
                        if (useweapon && attacker.Weapon is not null)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);
                        }
                        else if (!useweapon && attacker.Skills is not null && attacker.Skills.Count != 0)
                        {
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill);
                        }
                        else
                        {
                            response.Data.Log
                                .Add($"{attacker.Name} was not able to attack!");
                            continue;
                        }

                        response.Data.Log
                            .Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} with {(damage >= 0 ? damage : 0)} damage.");
                        if (opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            response.Data.Log.Add($"{opponent.Name} has been defeated!");
                            response.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                            break;
                        }
                    }

                }

                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = 100;
                });

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto skillAttack)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _dbContext.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == skillAttack.AttackerId);

                var opponent = await _dbContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == skillAttack.OpponentId);

                if (attacker is null || opponent is null || attacker.Skills is null)
                {
                    throw new Exception($"Somehting fishy happening...");
                }

                var skill = attacker.Skills.FirstOrDefault(s => s.Id == skillAttack.SkillId);

                if (skill is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't know that skill!";
                    return response;
                }

                int damage = DoSkillAttack(attacker, opponent, skill);
                if (opponent.HitPoints <= 0)
                {
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                await _dbContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto weaponAttack)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _dbContext.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == weaponAttack.AttackerId);

                var opponent = await _dbContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == weaponAttack.OpponentId);

                if (attacker is null || opponent is null || attacker.Weapon is null)
                {
                    throw new Exception($"Somehting fishy happening...");
                }

                int damage = DoWeaponAttack(attacker, opponent);
                if (opponent.HitPoints <= 0)
                {
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                await _dbContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            if (attacker.Weapon is null)
            {
                throw new Exception("Attacker has no weapon");
            }
            int damage = attacker.Weapon.Damage + new Random().Next(attacker.Strength);
            damage -= new Random().Next(opponent.Defense);

            if (damage > 0)
            {
                opponent.HitPoints -= damage;
            }

            return damage;
        }

        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            int damage = skill.Damage + new Random().Next(attacker.Intelligence);
            damage -= new Random().Next(opponent.Defense);

            if (damage > 0)
            {
                opponent.HitPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<List<HighScoreDto>>> GetHighScore()
        {
            var characters = await _dbContext.Characters
                .Where(c => c.Fights > 0)
                .OrderByDescending(c => c.Victories)
                .ThenBy(c => c.Defeats)
                .ToListAsync();

            var response = new ServiceResponse<List<HighScoreDto>>()
            {
                Data = characters.Select(c => _mapper.Map<HighScoreDto>(c)).ToList()
            };

            return response;
        }
    }
}