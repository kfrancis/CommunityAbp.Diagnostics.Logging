﻿using System;
using System.Threading.Tasks;
using CommunityAbp.Diagnostics.Logging.Sample.Services.Books;
using CommunityAbp.Diagnostics.Logging.Sample.Services.Dtos.Books;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace CommunityAbp.Diagnostics.Logging.Sample.Pages.Books;

public class EditModalModel : AbpPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateBookDto Book { get; set; }

    private readonly IBookAppService _bookAppService;

    public EditModalModel(IBookAppService bookAppService)
    {
        _bookAppService = bookAppService;
    }

    public async Task OnGetAsync()
    {
        var bookDto = await _bookAppService.GetAsync(Id);
        Book = ObjectMapper.Map<BookDto, CreateUpdateBookDto>(bookDto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _bookAppService.UpdateAsync(Id, Book);
        return NoContent();
    }
}