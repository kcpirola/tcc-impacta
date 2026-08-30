namespace PhishingDetectionAPI.Tests.Unit.Repositories;

using Microsoft.EntityFrameworkCore;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Infrastructure.Data;
using PhishingDetectionAPI.Infrastructure.Data.Repositories;
using Xunit;

public class EmailRepositoryTests
{
    private static PhishingDetectionContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PhishingDetectionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PhishingDetectionContext(options);
    }

    private static async Task SeedEmailsAsync(PhishingDetectionContext context)
    {
        var repository = new EmailRepository(context);
        await repository.AddAsync(new Email
        {
            MessageId = "1",
            Sender = "safe@empresa.com",
            Classification = EmailClassification.Safe,
            ReceivedDate = DateTime.UtcNow
        });
        await repository.AddAsync(new Email
        {
            MessageId = "2",
            Sender = "phishing@malicious.tk",
            Classification = EmailClassification.Phishing,
            ReceivedDate = DateTime.UtcNow
        });
        await repository.AddAsync(new Email
        {
            MessageId = "3",
            Sender = "phishing@malicious.tk",
            Classification = EmailClassification.Phishing,
            ReceivedDate = DateTime.UtcNow,
            IsValidated = true
        });
    }

    [Fact]
    public async Task GetByClassificationAsync_ReturnsOnlyMatchingEmails()
    {
        await using var context = CreateContext();
        await SeedEmailsAsync(context);
        var repository = new EmailRepository(context);

        var result = (await repository.GetByClassificationAsync(EmailClassification.Phishing)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(EmailClassification.Phishing, e.Classification));
    }

    [Fact]
    public async Task CountByClassificationAsync_ReturnsCorrectCount()
    {
        await using var context = CreateContext();
        await SeedEmailsAsync(context);
        var repository = new EmailRepository(context);

        var count = await repository.CountByClassificationAsync(EmailClassification.Safe);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetByMessageIdAsync_ReturnsMatchingEmail()
    {
        await using var context = CreateContext();
        await SeedEmailsAsync(context);
        var repository = new EmailRepository(context);

        var result = await repository.GetByMessageIdAsync("2");

        Assert.NotNull(result);
        Assert.Equal("phishing@malicious.tk", result!.Sender);
    }

    [Fact]
    public async Task GetUnvalidatedEmailsAsync_ExcludesValidatedEmails()
    {
        await using var context = CreateContext();
        await SeedEmailsAsync(context);
        var repository = new EmailRepository(context);

        var result = (await repository.GetUnvalidatedEmailsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.False(e.IsValidated));
    }
}
