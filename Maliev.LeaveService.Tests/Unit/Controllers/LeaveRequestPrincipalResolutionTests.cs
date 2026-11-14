using System.Security.Claims;
using Maliev.LeaveService.Api.Controllers;
using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Maliev.LeaveService.Tests.Unit.Controllers;

public sealed class LeaveRequestPrincipalResolutionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PrincipalLookupAllowsOnlyTheResolvedEmployee(bool matches)
    {
        var principalId = Guid.NewGuid();
        var requestedEmployee = Guid.NewGuid();
        var employeeClient = new Mock<IEmployeeServiceClient>();
        employeeClient.Setup(x => x.GetEmployeeIdByPrincipalIdAsync(principalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches ? requestedEmployee : Guid.NewGuid());
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetLeaveRequestsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<LeaveRequestDto>());
        var controller = CreateController(mediator.Object, employeeClient.Object,
            new Claim("principal_id", principalId.ToString()));

        var result = await controller.GetByEmployee(requestedEmployee, 2026, CancellationToken.None);

        if (matches)
        {
            Assert.IsType<OkObjectResult>(result);
            mediator.Verify(x => x.Send(It.Is<GetLeaveRequestsQuery>(q => q.EmployeeId == requestedEmployee && q.Year == 2026),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            Assert.IsType<ForbidResult>(result);
            mediator.Verify(x => x.Send(It.IsAny<GetLeaveRequestsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task InvalidPrincipalCannotTriggerLookupOrDispatch()
    {
        var employees = new Mock<IEmployeeServiceClient>();
        var mediator = new Mock<IMediator>();
        var controller = CreateController(mediator.Object, employees.Object, new Claim("principal_id", "not-a-guid"));

        var result = await controller.GetByEmployee(Guid.NewGuid(), null, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        employees.VerifyNoOtherCalls();
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ServiceIdentityDoesNotRequireEmployeeLookup()
    {
        var employees = new Mock<IEmployeeServiceClient>();
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetLeaveRequestsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<LeaveRequestDto>());
        var controller = CreateController(mediator.Object, employees.Object, new Claim("user_type", "service"));

        var result = await controller.GetByEmployee(Guid.NewGuid(), null, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        employees.VerifyNoOtherCalls();
    }

    private static LeaveRequestsController CreateController(IMediator mediator, IEmployeeServiceClient employeeClient, params Claim[] claims)
    {
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) };
        return new LeaveRequestsController(mediator, employeeClient) { ControllerContext = new ControllerContext { HttpContext = context } };
    }
}
