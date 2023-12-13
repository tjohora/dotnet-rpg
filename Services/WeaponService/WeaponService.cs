using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.WeaponService
{
    public class WeaponService : IWeaponService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public WeaponService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _db = db;

        }
        public async Task<ServiceResponse<GetCharacterResponseDto>> AddWeapon(AddWeaponRequestDto newWeapon)
        {
            var response = new ServiceResponse<GetCharacterResponseDto>();
            try
            {
                // Vid 60
                var character = await _db.Characters
                    .FirstOrDefaultAsync(c => c.Id == newWeapon.CharacterId &&
                        c.User!.Id == int.Parse(_httpContextAccessor.HttpContext!.User
                            .FindFirstValue(ClaimTypes.NameIdentifier)!));

                if (character is null)
                {
                    response.Success = false;
                    response.Message = "Character not found";
                    return response;
                }

                var weapon = new Weapon
                {
                    Name = newWeapon.Name,
                    Damage = newWeapon.Damage,
                    Character = character
                };

                _db.Weapons.Add(weapon);
                await _db.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterResponseDto>(character);

            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }
    }
}