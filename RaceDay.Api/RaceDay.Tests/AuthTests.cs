
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

    [Fact]
    public void PasswordHasher_HashesPasswordAndVerifiesIt()
    {
        // Arrange
        var user = new RaceDay.Api.Models.User
        {
            Email = "runner@example.com",
            Role = "Participant"
        };

        var passwordHasher =
            new Microsoft.AspNetCore.Identity.PasswordHasher<RaceDay.Api.Models.User>();

        var password = "StrongPass123!";

        // Act
        var hashedPassword = passwordHasher.HashPassword(user, password);

        var correctPasswordResult = passwordHasher.VerifyHashedPassword(
            user, hashedPassword, password);

        var incorrectPasswordResult = passwordHasher.VerifyHashedPassword(
            user, hashedPassword, "WrongPassword123!");

        // Assert
        Assert.NotEqual(password, hashedPassword);
        Assert.Equal(
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success,
            correctPasswordResult);
        Assert.Equal(
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed,
            incorrectPasswordResult);
    }

}
