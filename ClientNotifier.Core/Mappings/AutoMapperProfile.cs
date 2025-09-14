using AutoMapper;
using ClientNotifier.Core.DTOs;
using ClientNotifier.Core.Models;
using ClientNotifier.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // People mappings
            CreateMap<People, PersonDto>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => EgnUtils.GetAge(src.EGN)));

            CreateMap<People, PersonListDto>()
                .ForMember(dest => dest.HasBirthdayToday, opt => opt.MapFrom(src => 
                    src.Birthday.Month == DateTime.Today.Month && src.Birthday.Day == DateTime.Today.Day))
                .ForMember(dest => dest.HasNamedayToday, opt => opt.MapFrom(src => 
                    src.Nameday.HasValue && src.Nameday.Value.Month == DateTime.Today.Month && src.Nameday.Value.Day == DateTime.Today.Day))
                .ForMember(dest => dest.HasBirthdayThisWeek, opt => opt.MapFrom(src => 
                    IsCelebrationThisWeek(src.Birthday)))
                .ForMember(dest => dest.HasNamedayThisWeek, opt => opt.MapFrom(src => 
                    src.Nameday.HasValue && IsCelebrationThisWeek(src.Nameday.Value)));

            CreateMap<CreatePersonDto, People>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Birthday, opt => opt.Ignore()) // Will be set from EGN
                .ForMember(dest => dest.Nameday, opt => opt.Ignore()) // Will be set from service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore());

            CreateMap<UpdatePersonDto, People>()
                .ForMember(dest => dest.Birthday, opt => opt.Ignore()) // Will be set from EGN
                .ForMember(dest => dest.Nameday, opt => opt.Ignore()) // Will be set from service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore());

            // NamedayMapping mappings
            CreateMap<NamedayMapping, NamedayMappingDto>()
                .ForMember(dest => dest.NextOccurrence, opt => opt.MapFrom(src => GetNextOccurrence(src.Month, src.Day)));

            CreateMap<CreateNamedayMappingDto, NamedayMapping>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DateDisplay, opt => opt.Ignore());

            CreateMap<UpdateNamedayMappingDto, NamedayMapping>()
                .ForMember(dest => dest.DateDisplay, opt => opt.Ignore());
        }

        private static bool IsCelebrationThisWeek(DateTime date)
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var celebrationThisYear = new DateTime(currentYear, date.Month, date.Day);
            
            // If celebration already passed this year, check next year
            if (celebrationThisYear < today)
            {
                celebrationThisYear = celebrationThisYear.AddYears(1);
            }

            var daysUntilCelebration = (celebrationThisYear - today).Days;
            return daysUntilCelebration >= 0 && daysUntilCelebration <= 7;
        }

        private static DateTime GetNextOccurrence(int month, int day)
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var occurrence = new DateTime(currentYear, month, day);
            
            if (occurrence < today)
            {
                occurrence = occurrence.AddYears(1);
            }
            
            return occurrence;
        }
    }
}
