using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using CommunityAbp.Diagnostics.Logging.Sample.Services.Dtos.Books;
using CommunityAbp.Diagnostics.Logging.Sample.Entities.Books;

namespace CommunityAbp.Diagnostics.Logging.Sample.Services.Books;

public interface IBookAppService :
    ICrudAppService< //Defines CRUD methods
        BookDto, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateBookDto> //Used to create/update a book
{

}