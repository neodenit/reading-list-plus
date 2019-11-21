using AutoMapper;
using ReadingListPlus.DataAccess.Models;
using ReadingListPlus.Services.ViewModels;

namespace ReadingListPlus.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Deck, ImportExportDeck3>();
            CreateMap<ImportExportDeck, Deck>();
            CreateMap<ImportExportDeck2, Deck>();
            CreateMap<ImportExportDeck3, Deck>();

            CreateMap<Card, ImportExportCard3>();
            CreateMap<ImportExportCard, Card>()
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.Type));
            CreateMap<ImportExportCard2, Card>();
            CreateMap<ImportExportCard3, Card>();

            CreateMap<Card, CardViewModel>()
                .ForMember(dest => dest.DeckTitle,
                    opt => opt.MapFrom(src => src.Deck == null ? null : src.Deck.Title));
            CreateMap<Card, EditCardViewModel>()
                .ForMember(dest => dest.DeckTitle,
                    opt => opt.MapFrom(src => src.Deck == null ? null : src.Deck.Title));
        }
    }
}
