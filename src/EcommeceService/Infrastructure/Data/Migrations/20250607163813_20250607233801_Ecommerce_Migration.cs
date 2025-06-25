using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250607233801_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
    CREATE OR REPLACE FUNCTION public.get_revenue_statistics(
        _branch_id bigint DEFAULT NULL::bigint,
        _from date DEFAULT NULL::date,
        _to date DEFAULT NULL::date
    )
    RETURNS TABLE(revenue_date date, total_revenue numeric) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE 
    PARALLEL UNSAFE
    ROWS 1000
    AS $BODY$
    BEGIN
        RETURN QUERY
        SELECT
            d.day::DATE AS revenue_date,
            COALESCE(dr.total_revenue, 0)::NUMERIC AS total_revenue
        FROM
            generate_series(
                COALESCE(_from, date_trunc('month', CURRENT_DATE AT TIME ZONE 'Asia/Ho_Chi_Minh')::DATE),
                COALESCE(_to, (date_trunc('month', CURRENT_DATE AT TIME ZONE 'Asia/Ho_Chi_Minh') + interval '1 month - 1 day')::DATE),
                interval '1 day'
            ) AS d(day)
        LEFT JOIN (
            SELECT
                (o.order_date AT TIME ZONE 'Asia/Ho_Chi_Minh')::DATE AS revenue_date,
                SUM(o.amount) AS total_revenue
            FROM ""order"" o
            WHERE o.status = 3
              AND (_branch_id IS NULL OR o.branch_id = _branch_id)
              AND o.order_date >= COALESCE(_from, date_trunc('month', CURRENT_DATE AT TIME ZONE 'Asia/Ho_Chi_Minh'))
              AND o.order_date < COALESCE(_to, (date_trunc('month', CURRENT_DATE AT TIME ZONE 'Asia/Ho_Chi_Minh') + interval '1 month'))
            GROUP BY (o.order_date AT TIME ZONE 'Asia/Ho_Chi_Minh')::DATE
        ) dr ON d.day = dr.revenue_date
        ORDER BY d.day;
    END;
    $BODY$;
"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS public.get_revenue_statistics(BIGINT);");
        }
    }
}
