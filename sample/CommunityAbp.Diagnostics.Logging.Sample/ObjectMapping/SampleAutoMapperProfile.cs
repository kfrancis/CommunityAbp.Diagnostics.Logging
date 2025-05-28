using AutoMapper;
using CommunityAbp.Diagnostics.Logging.Sample.Entities.Books;
using CommunityAbp.Diagnostics.Logging.Sample.Services.Dtos.Books;

namespace CommunityAbp.Diagnostics.Logging.Sample.ObjectMapping;

public class SampleAutoMapperProfile : Profile
{
    public SampleAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        CreateMap<BookDto, CreateUpdateBookDto>();
        /* Create your AutoMapper object mappings here */
    }
}
