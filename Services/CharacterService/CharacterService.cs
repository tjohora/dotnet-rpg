using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CharacterService(IMapper mapper, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        // This method here accesses the user id from the claims (token). This is done as a seperate function as
        // we will be needing the user id a lot, and previously we had a line of code inside each controller action
        // to get the id from the claims. More efficent to has a function that does this (DRY)
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> AddCharacter(AddCharacterRequestDto newCharacter)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            character.User = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

            _dbContext.Characters.Add(character);
            await _dbContext.SaveChangesAsync();

            ServiceResponse.Data =
                await _dbContext.Characters
                    .Where(c => c.User!.Id == GetUserId())
                    .Select(c => _mapper.Map<GetCharacterResponseDto>(c))
                    .ToListAsync();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> DeleteCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            try
            {

                var character = await _dbContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
                if (character == null)
                {
                    throw new Exception($"Character with Id '{id}' not found.");
                }

                _dbContext.Characters.Remove(character);

                await _dbContext.SaveChangesAsync();

                ServiceResponse.Data = await _dbContext.Characters
                    .Where(c => c.User!.Id == GetUserId())
                    .Select(c => _mapper.Map<GetCharacterResponseDto>(c))
                    .ToListAsync();
            }
            catch (Exception e)
            {
                ServiceResponse.Success = false;
                ServiceResponse.Message = e.Message;
            }

            return ServiceResponse;

        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> GetAllCharacters()
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            var dbCharacters = await _dbContext.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .Where(c => c.User!.Id == GetUserId()).ToListAsync();
            ServiceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> GetCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterResponseDto>();
            var dbCharacter = await _dbContext.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
            ServiceResponse.Data = _mapper.Map<GetCharacterResponseDto>(dbCharacter);
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterResponseDto>();
            try
            {

                // This include is needed to get the user data for the character and will require and
                // Since User is relational data, the User object will not be added Include to be used.
                var character = await _dbContext.Characters
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
                if (character is null || character.User!.Id != GetUserId())
                {
                    throw new Exception($"Character with Id '{updatedCharacter.Id}' not found.");
                }

                character.Name = updatedCharacter.Name;
                character.HitPoints = updatedCharacter.HitPoints;
                character.Strength = updatedCharacter.Strength;
                character.Defense = updatedCharacter.Defense;
                character.Intelligence = updatedCharacter.Intelligence;
                character.Class = updatedCharacter.Class;

                await _dbContext.SaveChangesAsync();

                ServiceResponse.Data = _mapper.Map<GetCharacterResponseDto>(character);

            }
            catch (Exception e)
            {
                ServiceResponse.Success = false;
                ServiceResponse.Message = e.Message;
            }

            return ServiceResponse;

        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> AddCharacterSkill(AddCharacterSkillRequestDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterResponseDto>();
            try
            {
                var character = await _dbContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId &&
                        c.User!.Id == GetUserId());

                if (character is null)
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                    return response;
                }

                var skill = await _dbContext.Skills.FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);

                if (skill is null)
                {
                    response.Success = false;
                    response.Message = "Skill not found.";
                    return response;
                }

                character.Skills!.Add(skill);
                await _dbContext.SaveChangesAsync();
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