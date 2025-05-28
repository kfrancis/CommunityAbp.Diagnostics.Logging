using System;
using System.ComponentModel.DataAnnotations;
using CommunityAbp.Diagnostics.Logging.Sample.Entities.Books;

namespace CommunityAbp.Diagnostics.Logging.Sample.Services.Dtos.Books;

public class CreateUpdateBookDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public BookType Type { get; set; } = BookType.Undefined;

    [Required]
    [DataType(DataType.Date)]
    public DateTime PublishDate { get; set; } = DateTime.Now;

    [Required]
    public float Price { get; set; }
}