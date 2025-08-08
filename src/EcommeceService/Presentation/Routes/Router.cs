using Contracts.Routers;
using Domain.Aggregates.Equipments;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Ecommerce = "Ecommerce";

        public static class ServiceRoute
        {
            public const string Tags = $"{nameof(ServiceRoute)} endpoint";
            public const string Services = $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}";
            public const string ServicesByTariff =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/{nameof(ServicesByTariff)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
            public const string TopService =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/{nameof(TopService)}";
            public const string Feedbacks =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/"
                + "{"
                + RouterBase.Id
                + "}/"
                + $"{nameof(Feedbacks)}";
            public const string CreateFeedback =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/"
                + "{"
                + RouterBase.Id
                + "}/"
                + $"{nameof(CreateFeedback)}";
        }

        public static class TariffRoute
        {
            public const string Tags = $"{nameof(TariffRoute)} endpoint";
            public const string Tariffs = $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}";
            public const string TariffByBranch =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}/{nameof(TariffByBranch)}";

            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class UnitRoute
        {
            public const string Tags = $"{nameof(UnitRoute)} endpoint";
            public const string Units = $"{Ecommerce}/{RouterBase.prefix}{nameof(Units)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Units)}/" + "{" + RouterBase.Id + "}";
        }

        public static class CategoryRoute
        {
            public const string Tags = $"{nameof(CategoryRoute)} endpoint";
            public const string Categories = $"{Ecommerce}/{RouterBase.prefix}{nameof(Categories)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Categories)}/" + "{" + RouterBase.Id + "}";
        }

        public static class OrderRoute
        {
            public const string Tags = $"{nameof(OrderRoute)} endpoint";
            public const string Orders = $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/" + "{" + RouterBase.Id + "}";

            public const string GetByCode =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(GetByCode)}";
            public const string GetLinkPayment =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(GetLinkPayment)}"
                + "{"
                + RouterBase.Id
                + "}";
            public const string UpdateStatus =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(UpdateStatus)}"
                + "{"
                + RouterBase.Id
                + "}";
            public const string GetReceipt =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(GetReceipt)}/"
                + "{"
                + RouterBase.Id
                + "}";

            public const string GetByStaff =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(GetByStaff)}/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class Webhook
        {
            public const string Tags = $"{nameof(Webhook)} endpoint";
            public const string CompletedOrder =
                $"{nameof(Webhook)}/{RouterBase.prefix}{nameof(CompletedOrder)}";
        }

        public static class InventoryRoute
        {
            public const string Tags = $"{nameof(InventoryRoute)} endpoint";
            public const string Inventories =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Inventories)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Inventories)}/"
                + "{"
                + RouterBase.Id
                + "}";
            public const string UpdateStatus =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Inventories)}/{nameof(UpdateStatus)}/"
                + "{"
                + RouterBase.Id
                + "}";

            public const string GetReceipt =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Inventories)}/{nameof(GetReceipt)}/"
                + "{"
                + RouterBase.Id
                + "}/"
                + "{SupplierId}";
        }

        public static class SaleResultRoute
        {
            public const string Tags = $"{nameof(SaleResultRoute)} endpoint";

            public const string DashboardCard =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(DashboardCard)}";
            public const string RevenueStatistic =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(RevenueStatistic)}";
            public const string TopService = $"{Ecommerce}/{RouterBase.prefix}{nameof(TopService)}";
            public const string NetRevenueBranch =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(NetRevenueBranch)}";
        }

        public static class UserRoute
        {
            public const string Users = $"{Ecommerce}/{RouterBase.prefix}{nameof(Users)}";
            public const string GetRouteName = $"{Users}DetailEndpoint";
            public const string Tags = $"{nameof(Users)} endpoint";
        }

        public static class ReportRoute
        {
            public const string Tags = $"{nameof(ReportRoute)} endpoint";
            public const string ReportServiceOrder =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportServiceOrder)}";
            public const string Order =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportRoute)}/{nameof(Order)}";
            public const string CustomerRevenue =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportRoute)}/{nameof(CustomerRevenue)}";
            public const string RevenueReport =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportRoute)}/{nameof(RevenueReport)}";
            public const string FinancialReport =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportRoute)}/{nameof(FinancialReport)}";
            public const string ProductSupplierReport =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportRoute)}/{nameof(ProductSupplierReport)}";
        }

        public static class SupplierRoute
        {
            public const string Tags = $"{nameof(SupplierRoute)} endpoint";
            public const string Suppliers = $"{Ecommerce}/{RouterBase.prefix}{nameof(Suppliers)}";

            public const string ImportExportHistories =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ImportExportHistories)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Suppliers)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Suppliers)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class BranchProductRoute
        {
            public const string Tags = $"{nameof(BranchProductRoute)} endpoint";
            public const string BranchProducts =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(BranchProducts)}";
            public const string BranchProductCardInv =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(BranchProductCardInv)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(BranchProducts)}/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class EquipmentRoute
        {
            public const string Tags = $"{nameof(EquipmentRoute)} endpoint";
            public const string Equipments = $"{Ecommerce}/{RouterBase.prefix}{nameof(Equipments)}";
            public const string Activities =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Equipments)}/"
                + "{"
                + RouterBase.Id
                + "}"
                + $"/{nameof(Activities)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Equipments)}/" + "{" + RouterBase.Id + "}";
            public const string UpdateStatus =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Equipments)}/{nameof(UpdateStatus)}"
                + "{"
                + RouterBase.Id
                + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Equipments)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class VoucherRoute
        {
            public const string Tags = $"{nameof(VoucherRoute)} endpoint";
            public const string Vouchers = $"{Ecommerce}/{RouterBase.prefix}{nameof(Vouchers)}";
            public const string VoucherUsage =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(VoucherUsage)}";
            public const string VoucherUsageDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(VoucherUsage)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
            public const string CheckCode =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(VoucherUsage)}/"
                + "{"
                + RouterBase.Id
                + "}"
                + $"/{nameof(CheckCode)}/"
                + "{CustomerId}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Vouchers)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Vouchers)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class EquipmentActivityRoute
        {
            public const string Tags = $"{nameof(EquipmentActivityRoute)} endpoint";
            public const string EquipmentActivities =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(EquipmentActivities)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(EquipmentActivities)}/"
                + "{"
                + RouterBase.Id
                + "}";
            public const string UpdateStatus =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(EquipmentActivities)}/{nameof(UpdateStatus)}"
                + "{"
                + RouterBase.Id
                + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(EquipmentActivities)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class FeedbackRoute
        {
            public const string Tags = $"{nameof(FeedbackRoute)} endpoint";
            public const string Feedbacks = $"{Ecommerce}/{RouterBase.prefix}{nameof(Feedbacks)}";
            public const string FeedbackReplies =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Feedbacks)}/"
                + "{"
                + RouterBase.Id
                + "}"
                + $"/{nameof(FeedbackReplies)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Feedbacks)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Feedbacks)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
            public const string React =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Feedbacks)}/"
                + "{"
                + RouterBase.Id
                + "}/"
                + $"{nameof(React)}";
        }
    }
}
