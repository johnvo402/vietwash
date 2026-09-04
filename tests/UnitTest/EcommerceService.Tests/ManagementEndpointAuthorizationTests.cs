using System.Reflection;
using Application.Common.Auth;
using Presentation.Endpoints.Categories;
using Presentation.Endpoints.Inventories;
using Presentation.Endpoints.Reports;
using Presentation.Endpoints.Tariffs;
using Presentation.Endpoints.Units;
using Presentation.Endpoints.Vouchers;

namespace EcommerceService.Tests;

public class ManagementEndpointAuthorizationTests
{
    [Fact]
    public void PreviouslyPublicManagementEndpoints_RequireIntendedRoles()
    {
        AssertRoles(typeof(CreateVoucherEndpoint), "ADMIN", "MANAGER", "STAFF");
        AssertRoles(typeof(ListVoucherEndpoint), "ADMIN", "MANAGER", "STAFF");
        AssertRoles(typeof(InventoryDocumentDetailEndpoint), "ADMIN", "MANAGER");
        AssertRoles(typeof(GetInventoryReceiptEndpoint), "ADMIN", "MANAGER");
        AssertRoles(typeof(CreateCategoryEndpoint), "ADMIN", "MANAGER");
        AssertRoles(typeof(ListCategoryEndpoint), "ADMIN", "MANAGER", "STAFF");
        AssertRoles(typeof(CreateUnitEndpoint), "ADMIN", "MANAGER", "STAFF");
        AssertRoles(typeof(ListUnitEndpoint), "ADMIN", "MANAGER", "STAFF");
    }

    [Fact]
    public void ReportsAndTariffAdministration_RejectCustomerAndStaffRoles()
    {
        AssertRoles(typeof(FinancialReportEndpoint), "ADMIN", "MANAGER");
        AssertRoles(typeof(OrderReportEndpoint), "ADMIN", "MANAGER");
        AssertRoles(typeof(ProductSupplierReportEndpoint), "ADMIN", "MANAGER");
        AssertRoles(typeof(CreateTariffEndpoint), "ADMIN");
        AssertRoles(typeof(ListTariffEndpoint), "ADMIN");
    }

    private static void AssertRoles(Type endpointType, params string[] expectedRoles)
    {
        var method = endpointType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Single(candidate => candidate.Name == "HandleAsync");
        var authorization = Assert.IsType<AuthorizeByAttribute>(
            method.GetCustomAttribute<AuthorizeByAttribute>()
        );

        Assert.NotEmpty(authorization.Value);
        foreach (string role in new[] { "ADMIN", "MANAGER", "STAFF", "CUSTOMER" })
            Assert.Equal(expectedRoles.Contains(role), authorization.Value.Contains($"\"{role}\""));
    }
}
