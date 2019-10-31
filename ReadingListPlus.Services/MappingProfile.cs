using AutoMapper;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Deck, ImportExportDeck2>();
            CreateMap<ImportExportDeck, Deck>();
            CreateMap<ImportExportDeck2, Deck>();

            CreateMap<Card, ImportExportCard2>();
            CreateMap<ImportExportCard, Card>()
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.Type));
            CreateMap<ImportExportCard2, Card>();
        }
    }
}
