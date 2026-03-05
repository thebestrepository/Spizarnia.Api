using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;
using Spizarnia.Infrastructure.Wolt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Spizarnia.Api.Controllers;

[ApiController]
[Route("api/webhooks/wolt")]
public class WebhooksController(IOrderRepository orders, IOptions<WoltOptions> woltOptions) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleWoltEvent(CancellationToken ct)
    {
        var signature = Request.Headers["WOLT-SIGNATURE"].FirstOrDefault();
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);

        if (!ValidateSignature(body, signature))
            return Unauthorized();

        var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("delivery_id", out var deliveryIdEl) ||
            !doc.RootElement.TryGetProperty("status", out var statusEl))
            return BadRequest();

        var deliveryId = deliveryIdEl.GetString();
        var status = statusEl.GetString();

        if (deliveryId is null) return BadRequest();

        var order = await orders.GetByWoltDeliveryIdAsync(deliveryId, ct);
        if (order is not null)
        {
            order.Status = status switch
            {
                "DELIVERED" => OrderStatus.Delivered,
                "CANCELED" => OrderStatus.Canceled,
                "PICKED_UP" => OrderStatus.PickedUp,
                _ => order.Status
            };
            order.UpdatedAt = DateTime.UtcNow;
            await orders.UpdateAsync(order, ct);
            await orders.SaveChangesAsync(ct);
        }

        return Ok();
    }

    private bool ValidateSignature(string body, string? signature)
    {
        if (string.IsNullOrEmpty(signature)) return false;
        var secret = woltOptions.Value.ClientSecret;
        if (string.IsNullOrEmpty(secret)) return true; // skip in dev if not configured

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return string.Equals(signature, expected, StringComparison.OrdinalIgnoreCase);
    }
}
