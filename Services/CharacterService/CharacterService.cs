using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private static List<Character> characters = new List<Character>()
        {
            new Character(),
            new Character {Id = 1, Name = "Sam"}
        };

        private readonly IMapper _mapper;
        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> AddCharacter(AddCharacterRequestDto newCharacter)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            character.Id = characters.Max(c => c.Id) + 1;
            characters.Add(character);
            ServiceResponse.Data = characters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterResponseDto>>> DeleteCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterResponseDto>>();
            try
            {

                var character = characters.FirstOrDefault(c => c.Id == id);
                if (character == null)
                {
                    throw new Exception($"Character with Id '{id}' not found.");
                }

                characters.Remove(character);

                ServiceResponse.Data = characters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToList();
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
            ServiceResponse.Data = characters.Select(c => _mapper.Map<GetCharacterResponseDto>(c)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> GetCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterResponseDto>();
            var character = characters.FirstOrDefault(c => c.Id == id);
            ServiceResponse.Data = _mapper.Map<GetCharacterResponseDto>(character);
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterResponseDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterResponseDto>();
            try
            {

                var character = characters.FirstOrDefault(c => c.Id == updatedCharacter.Id);
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