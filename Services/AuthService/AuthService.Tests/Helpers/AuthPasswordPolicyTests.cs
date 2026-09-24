using AuthService.Application.Helpers;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Helpers;

/// <summary>
/// Chính sách mật khẩu 4 yếu tố (CLAUDE.md 13.6.4): tối thiểu 8 ký tự,
/// có chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
/// </summary>
public class AuthPasswordPolicyTests
{
    [Theory]
    [InlineData("Abcdef1!")]      // đủ 4 yếu tố, đúng 8 ký tự
    [InlineData("P@ssw0rd123")]
    [InlineData("Aa1!Aa1!")]
    public void Matches_ValidPassword_ReturnsTrue(string password)
    {
        AuthPasswordPolicy.Matches(password).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Ab1!")]              // quá ngắn (< 8 ký tự)
    [InlineData("abcdefg1!")]         // thiếu chữ hoa
    [InlineData("ABCDEFG1!")]         // thiếu chữ thường
    [InlineData("Abcdefgh!")]         // thiếu chữ số
    [InlineData("Abcdefgh1")]         // thiếu ký tự đặc biệt
    [InlineData("        ")]         // toàn khoảng trắng, không đủ yếu tố
    public void Matches_InvalidPassword_ReturnsFalse(string? password)
    {
        AuthPasswordPolicy.Matches(password).Should().BeFalse();
    }

    [Fact]
    public void Matches_ExactlyEightCharsWithAllFourFactors_ReturnsTrue()
    {
        AuthPasswordPolicy.Matches("Ab3!Ab3!").Should().BeTrue();
    }

    [Fact]
    public void Matches_SevenCharsWithAllFourFactors_ReturnsFalse()
    {
        AuthPasswordPolicy.Matches("Ab3!Ab3").Should().BeFalse();
    }
}
