
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests
{
    [Fact]
    public void RegisterRequest_InvalidEmail_FailsValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "not-an-email",
            Password = "StrongPass123!",
            Role = "Participant"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        // Act
        var isValid = Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void RegisterRequest_ShortPassword_FailsValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "abc",
            Role = "Participant"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        // Act
        var isValid = Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void RegisterRequest_InvalidRole_FailsValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "StrongPass123!",
            Role = "Administrator"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        // Act
        var isValid = Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(nameof(RegisterRequest.Role)));
    }

    [Fact]
    public void RegisterRequest_ValidDetails_PassesValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "StrongPass123!",
            Role = "Participant"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        // Act
        var isValid = Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

}
