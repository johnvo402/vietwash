using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Finance = "Finance";

        public static class FundRoute
        {
            public const string Tags = $"{nameof(FundRoute)} endpoint";
            public const string Funds = $"{Finance}/{RouterBase.prefix}{nameof(Funds)}";
            public const string GetUpdateDelete =
                $"{Finance}/{RouterBase.prefix}{nameof(Funds)}/" + "{" + RouterBase.Id + "}";
        }

        public static class FundBehaviorRoute
        {
            public const string Tags = $"{nameof(FundBehaviorRoute)} endpoint";
            public const string FundBehaviors =
                $"{Finance}/{RouterBase.prefix}{nameof(FundBehaviors)}";
            public const string GetUpdateDelete =
                $"{Finance}/{RouterBase.prefix}{nameof(FundBehaviors)}/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class TransactionRoute
        {
            public const string Tags = $"{nameof(TransactionRoute)} endpoint";
            public const string Transaction = $"{Finance}/{RouterBase.prefix}{nameof(Transaction)}";
            public const string GetCustomerPoint =
                $"{Finance}/{RouterBase.prefix}{nameof(Transaction)}/{nameof(GetCustomerPoint)}";
            public const string GetPointByCustomerId =
                $"{Finance}/{RouterBase.prefix}{nameof(Transaction)}/{nameof(GetPointByCustomerId)}/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class EInvoiceRoute
        {
            public const string Tags = $"{nameof(EInvoiceRoute)} endpoint";
            public const string EInvoice = $"{Finance}/{RouterBase.prefix}{nameof(EInvoice)}";

            public const string GetByOrderId =
                $"{Finance}/{RouterBase.prefix}{nameof(EInvoice)}/{nameof(GetByOrderId)}/"
                + "{OrderId}";
            public const string GetByCode =
                $"{Finance}/{RouterBase.prefix}{nameof(EInvoice)}/{nameof(GetByCode)}/" + "{Code}";
        }
    }
}
