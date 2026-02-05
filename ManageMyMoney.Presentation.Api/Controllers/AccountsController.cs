using ManageMyMoney.Core.Application.DTOs.Accounts;
using ManageMyMoney.Core.Application.Features.Accounts;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class AccountsController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AccountResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var result = await _accountService.CreateAccountAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetAccountById), new { id = result.Value?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var result = await _accountService.GetAccountByIdAsync(CurrentUserId, id);
        return HandleNotFound(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts([FromQuery] bool activeOnly = true)
    {
        var result = await _accountService.GetAccountsAsync(CurrentUserId, activeOnly);
        return HandleResult(result);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<AccountSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsSummary()
    {
        var result = await _accountService.GetAccountsSummaryAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request)
    {
        var result = await _accountService.UpdateAccountAsync(CurrentUserId, id, request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateAccount(Guid id)
    {
        var result = await _accountService.DeactivateAccountAsync(CurrentUserId, id);
        return HandleResult(result, "Account deactivated successfully");
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<TransferResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result = await _accountService.TransferAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("transfers")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransferResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransfers([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _accountService.GetTransfersAsync(CurrentUserId, fromDate, toDate);
        return HandleResult(result);
    }

    [HttpPost("payment-methods")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
    {
        var result = await _accountService.CreatePaymentMethodAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentMethodResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var result = await _accountService.GetPaymentMethodsAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPut("payment-methods/{id:guid}/set-default")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid id)
    {
        var result = await _accountService.SetDefaultPaymentMethodAsync(CurrentUserId, id);
        return HandleResult(result, "Default payment method updated");
    }

    [HttpPost("credit-cards")]
    [ProducesResponseType(typeof(ApiResponse<CreditCardResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCreditCard([FromBody] CreateCreditCardRequest request)
    {
        var result = await _accountService.CreateCreditCardAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("credit-cards")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreditCardResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCreditCards()
    {
        var result = await _accountService.GetCreditCardsAsync(CurrentUserId);
        return HandleResult(result);
    }
}
