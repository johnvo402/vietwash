CREATE OR REPLACE FUNCTION public.get_customer_revenue_report(
    _branch_ids bigint[],
    _from timestamp with time zone,
    _to timestamp with time zone,
    _search_keywords text
)
RETURNS TABLE (
    customer_id bigint,
    customer_code text,
    phone_number text,
    display_name text,
    avt_url text,
    revenue numeric,
    cancel_value numeric,
    net_revenue numeric,
    order_sale_quantity integer,
    order_cancel_quantity integer
)
LANGUAGE sql
STABLE
AS $$
    WITH order_customer AS (
        SELECT
            o.customer_id,
            SUM(CASE WHEN o.status = 4 THEN o.amount ELSE 0 END) AS revenue,
            SUM(CASE WHEN o.status = 5 THEN o.amount ELSE 0 END) AS cancel_value,
            SUM(CASE WHEN o.status = 4 THEN o.total ELSE 0 END) AS net_revenue,
            COUNT(CASE WHEN o.status = 4 THEN 1 END) AS order_sale_quantity,
            COUNT(CASE WHEN o.status = 5 THEN 1 END) AS order_cancel_quantity
        FROM "order" o
        WHERE
            _branch_ids IS NOT NULL
            AND cardinality(_branch_ids) > 0
            AND o.branch_id = ANY(_branch_ids)
            AND (
                (
                    o.status = 4
                    AND (_from IS NULL OR o.order_date >= _from)
                    AND (_to IS NULL OR o.order_date < _to)
                )
                OR
                (
                    o.status = 5
                    AND o.cancelled_at IS NOT NULL
                    AND (_from IS NULL OR o.cancelled_at >= _from)
                    AND (_to IS NULL OR o.cancelled_at < _to)
                )
            )
        GROUP BY o.customer_id
    )
    SELECT
        oc.customer_id,
        u.code as customer_code,
        u.phone_number,
        COALESCE(u.display_name, 'Khách lẻ') AS display_name,
        u.avt_url,
        oc.revenue,
        oc.cancel_value,
        oc.net_revenue,
        oc.order_sale_quantity,
        oc.order_cancel_quantity
    FROM order_customer oc
    LEFT JOIN "user" u ON u.id = oc.customer_id
    WHERE
        (_search_keywords IS NULL
         OR u.phone_number ILIKE '%' || _search_keywords || '%'
         OR u.display_name ILIKE '%' || _search_keywords || '%'
         OR (oc.customer_id IS NULL AND 'Khách lẻ' ILIKE '%' || _search_keywords || '%'));
$$;
