using AutoMapper;
using ReadingListPlus.DataAccess.Models;

namespace ReadingListPlus.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Deck, ImportExportDeck>();
            CreateMap<ImportExportDeck, Deck>();

            CreateMap<Card, ImportExportCard>();
            CreateMap<ImportExportCard, Card>();
        }
    }
}
