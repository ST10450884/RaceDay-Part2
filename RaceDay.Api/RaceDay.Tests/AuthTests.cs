
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests
{
    [Fact]
    public void RegisterRequest_InvalidEmail_FailsValidation()
    {
        var request = new RegisterRequest
        {
            Email = "not-an-email",
            Password = "StrongPass123!"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void RegisterRequest_ShortPassword_FailsValidation()
    {
        var request = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "abc"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void RegisterRequest_ValidDetails_PassesValidation()
    {
        var request = new RegisterRequest
        {
            Email = "runner@example.com",
            Password = "StrongPass123!"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void PasswordHasher_HashesPasswordAndVerifiesIt()
    {
        var user = new User
        {
            Email = "runner@example.com",
            Role = "Participant"
        };

        var hasher = new PasswordHasher<User>();
        var password = "StrongPass123!";

        var hashedPassword = hasher.HashPassword(user, password);

        var correctResult = hasher.VerifyHashedPassword(
            user, hashedPassword, password);

        var incorrectResult = hasher.VerifyHashedPassword(
            user, hashedPassword, "WrongPassword123!");

        Assert.NotEqual(password, hashedPassword);
        Assert.Equal(PasswordVerificationResult.Success, correctResult);
        Assert.Equal(PasswordVerificationResult.Failed, incorrectResult);
    }
}
