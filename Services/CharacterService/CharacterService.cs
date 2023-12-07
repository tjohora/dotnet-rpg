using System;
using System.Collections.Generic;
using System.Linq;
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
        public CharacterService(IMapper mapper, ApplicationDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> AddCharacter(AddCharacterRequestDto newCharacter)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            _dbContext.Characters.Add(character);
            await _dbContext.SaveChangesAsync();
            ServiceResponse.Data = await _dbContext.Characters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToListAsync();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> DeleteCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            try
            {

                var character = await _dbContext.Characters.FirstOrDefaultAsync(c => c.Id == id);
                if (character == null)
                {
                    throw new Exception($"Character with Id '{id}' not found.");
                }

                _dbContext.Characters.Remove(character);

                await _dbContext.SaveChangesAsync();

                ServiceResponse.Data = await _dbContext.Characters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToListAsync();
            }
            catch (Exception e)
            {
                ServiceResponse.Success = false;
                ServiceResponse.Message = e.Message;
            }

            return ServiceResponse;

        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> GetAllCharacters(int userId)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            var dbCharacters = await _dbContext.Characters.Where(c => c.User!.Id == userId).ToListAsync();
            ServiceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> GetCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterResponseDto>();
            var dbCharacter = await _dbContext.Characters.FirstOrDefaultAsync(c => c.Id == id);
            ServiceResponse.Data = _mapper.Map<GetCharacterResponseDto>(dbCharacter);
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterResponseDto>();
            try
            {

                var character = await _dbContext.Characters.FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
                if (character == null)
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
    }
}