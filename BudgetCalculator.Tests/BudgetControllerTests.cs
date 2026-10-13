using System.Text.Json;
using BudgetCalculator.Api.Controllers;
using BudgetCalculator.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BudgetCalculator.Tests;

public class BudgetControllerTests
{
    private readonly BudgetController _controller = new();

    [Fact]
    public void Calculate_WithoutInsurance_ReturnsOkAnd500()
    {
        var request = new BudgetRequest
        {
            DailyCost = 100m,
            Days = 5,
            IncludeInsurance = false
        };

        var result = _controller.Calculate(request);

        AssertTotalAmount(result, 500m);
    }

    [Fact]
    public void Calculate_WithInsurance_ReturnsOkAnd525()
    {
        var request = new BudgetRequest
        {
            DailyCost = 100m,
            Days = 5,
            IncludeInsurance = true
        };

        var result = _controller.Calculate(request);

        AssertTotalAmount(result, 525m);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Calculate_InvalidDays_ReturnsBadRequest(int days)
    {
        var request = new BudgetRequest
        {
            DailyCost = 100m,
            Days = days,
            IncludeInsurance = false
        };

        var result = _controller.Calculate(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        Assert.Equal("Количество дней должно быть больше нуля", badRequest.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Calculate_ZeroDailyCost_ReturnsOkAndZero(bool includeInsurance)
    {
        var request = new BudgetRequest
        {
            DailyCost = 0m,
            Days = 5,
            IncludeInsurance = includeInsurance
        };

        var result = _controller.Calculate(request);

        AssertTotalAmount(result, 0m);
    }

    [Fact]
    public void Calculate_FractionalDailyCost_ReturnsOkAnd301_50()
    {
        var request = new BudgetRequest
        {
            DailyCost = 100.50m,
            Days = 3,
            IncludeInsurance = false
        };

        var result = _controller.Calculate(request);

        AssertTotalAmount(result, 301.50m);
    }

    [Fact]
    public void Calculate_WithoutInsurance_RoundsTotalToKopecks()
    {
        var request = new BudgetRequest
        {
            DailyCost = 100.505m,
            Days = 1,
            IncludeInsurance = false
        };

        var result = _controller.Calculate(request);

        AssertTotalAmount(result, 100.51m);
    }

    [Fact]
    public void Calculate_WithInsurance_RoundsTotalToKopecks()
    {
        var request = new BudgetRequest
        {
            DailyCost = 100.50m,
            Days = 3,
            IncludeInsurance = true
        };

        var result = _controller.Calculate(request);

        AssertTotalAmount(result, 316.58m);
    }

    private static void AssertTotalAmount(IActionResult result, decimal expected)
    {
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.NotNull(okResult.Value);

        // Получение суммы из анонимного объекта
        var response = JsonSerializer.SerializeToElement(okResult.Value);
        var actual = response.GetProperty("TotalAmount").GetDecimal();

        Assert.Equal(expected, actual);
    }
}
