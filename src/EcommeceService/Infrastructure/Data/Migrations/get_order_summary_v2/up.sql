DROP FUNCTION IF EXISTS public.get_order_summary(
    bigint[],
    timestamp with time zone,
    timestamp with time zone,
    text
);

CREATE OR REPLACE FUNCTION public.get_order_summary(
	branch_ids bigint[],
	from_date timestamp with time zone,
	to_date timestamp with time zone,
	search_keywords text)
    RETURNS TABLE(order_id bigint, public_id VARCHAR, code citext, branch_id bigint, customer_name text, order_item_count integer, amount numeric, order_date timestamp with time zone) 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
    SELECT 
        o.id,
        o.public_id,
        o.code,
        o.branch_id,
        u.display_name as customer_name,
        COUNT(oi.id) AS order_item_count,
        o.amount AS amount,
        o.order_date
    FROM "order" o
    LEFT JOIN order_item oi ON oi.order_id = o.id
    LEFT JOIN "user" u ON u.id = o.customer_id
    WHERE 
        (
            branch_ids IS NULL OR
            cardinality(branch_ids) = 0 OR
            o.branch_id = ANY(branch_ids)
        )
        AND (from_date IS NULL OR o.order_date >= from_date)
        AND (to_date IS NULL OR o.order_date <= to_date)
        AND (
            search_keywords IS NULL 
            OR o.code ILIKE '%' || search_keywords || '%'
        )
    GROUP BY o.id, u.display_name;
$BODY$;

ALTER FUNCTION public.get_order_summary(branch_ids bigint[], from_date timestamp with time zone, to_date timestamp with time zone, search_keywords text)
    OWNER TO postgres;
