using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using smart_expenses_web_app.Data;
using smart_expenses_web_app.Enums;
using smart_expenses_web_app.Services;

namespace smart_expenses_web_app.Pages.Transaction;

public class CreateModel : PageModel
{
    private readonly TransactionService _transactionService;
    private readonly UserService _userService;
    private readonly AccountService _accountService;

    public CreateModel(TransactionService transactionService, UserService userService, AccountService accountService)
    {
        _transactionService = transactionService;
        _userService = userService;
        _accountService = accountService;

        Transaction = new Models.Transaction();
    }

    public string? UserId { get; set; }
    public SelectList? TransactionTypesSelectList { get; set; }
    public SelectList? CurrencyCodesSelectList { get; set; }
    public SelectList? AccountsSelectList { get; set; }

    [BindProperty]
    public Models.Transaction Transaction { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        // Access current session user guid
        UserId = _userService.GetUserId();
        
        // Prepare form account types list
        var transactionTypesSelectListItems = Enum.GetValues<TransactionType>()
            .Select(transactionType => new SelectListItem { Text = transactionType.ToString(), Value = transactionType.ToString() })
            .ToList();
        TransactionTypesSelectList = new SelectList(transactionTypesSelectListItems, "Value", "Text");
        
        // Prepare form currency codes list
        var currencyCodesSelectListItems = Enum.GetValues<CurrencyCode>()
            .Select(currencyCode => new SelectListItem { Text = currencyCode.ToString(), Value = currencyCode.ToString() })
            .ToList();
        CurrencyCodesSelectList = new SelectList(currencyCodesSelectListItems, "Value", "Text");
        
        // Prepare form accounts list
        var accountsSelectListItems = (await _accountService.GetUserAccountsAsync(UserId))!
            .Select(account => new SelectListItem { Text = account.Title, Value = account.Id.ToString() })
            .ToList();
        AccountsSelectList = new SelectList(accountsSelectListItems, "Value", "Text");
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Check if can be pushed to database
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
            
            return Page();
        }

        // Push transactions to database & save
        await _transactionService.AddUserTransactionAsync(Transaction);

        return RedirectToPage("./Index");
    }
}