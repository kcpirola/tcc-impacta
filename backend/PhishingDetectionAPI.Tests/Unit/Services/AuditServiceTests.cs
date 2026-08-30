namespace PhishingDetectionAPI.Tests.Unit.Services;

using AutoMapper;
using Moq;
using PhishingDetectionAPI.Application.Mapping;
using PhishingDetectionAPI.Application.Services;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;
using Xunit;

public class AuditServiceTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
    private readonly IMapper _mapper;

    public AuditServiceTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    private AuditService CreateService() => new(_auditLogRepositoryMock.Object, _mapper);

    [Fact]
    public async Task LogActionAsync_PersistsAuditLogWithProvidedData()
    {
        var service = CreateService();

        await service.LogActionAsync(1, "login", "user", 1, "Usuário fez login", "127.0.0.1", "test-agent");

        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.Is<AuditLog>(log =>
            log.UserId == 1 &&
            log.Action == "login" &&
            log.ResourceType == "user" &&
            log.IpAddress == "127.0.0.1")), Times.Once);
    }

    [Fact]
    public async Task GetUserAuditLogsAsync_ReturnsMappedDtos()
    {
        var logs = new List<AuditLog>
        {
            new() { Id = 1, UserId = 1, Action = "login", ResourceType = "user", IpAddress = "127.0.0.1" }
        };
        _auditLogRepositoryMock
            .Setup(r => r.GetByUserIdAsync(1, 0, 20))
            .ReturnsAsync(logs);
        var service = CreateService();

        var result = await service.GetUserAuditLogsAsync(1);

        Assert.Single(result);
        Assert.Equal("login", result[0].Action);
    }
}
