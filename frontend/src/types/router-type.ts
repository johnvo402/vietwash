export const ROUTE_HOME = "/";
export const ROUTE_MANAGE = "/manage";
export const ROUTE_DASHBOARD = `${ROUTE_MANAGE}/dashboard`;
export const ROUTE_REPORT = `${ROUTE_MANAGE}/report`;
export const ROUTE_REPORT_SERVICE = `${ROUTE_REPORT}/service`;
export const ROUTE_REPORT_ORDER = `${ROUTE_REPORT}/order`;
export const ROUTE_REPORT_CUSTOMER = `${ROUTE_REPORT}/customer-revenue`;
export const ROUTE_REPORT_FINANCE = `${ROUTE_REPORT}/finance`;
export const ROUTE_REPORT_SUPPLIER = `${ROUTE_REPORT}/import-export`;
export const ROUTE_REPORT_REVENUE = `${ROUTE_REPORT}/revenue`;

export const ROUTE_FUND = `${ROUTE_MANAGE}/fund`;
export const ROUTE_FUND_DETAIL = `${ROUTE_FUND}/[publicId]`;
export const ROUTE_FUND_EDIT = `${ROUTE_FUND}/edit/[publicId]`;

export const ROUTE_SERVICE = `${ROUTE_MANAGE}/service`;
export const ROUTE_SERVICE_DETAIL = `${ROUTE_SERVICE}/[publicId]`;
export const ROUTE_SERVICE_EDIT = `${ROUTE_SERVICE}/edit/[publicId]`;
export const ROUTE_SERVICE_CREATE = `${ROUTE_SERVICE}/create`;

export const ROUTE_ORDERS = `${ROUTE_MANAGE}/orders`;
export const ROUTE_ORDERS_DETAIL = `${ROUTE_ORDERS}/[publicId]`;
export const ROUTE_USERS = `${ROUTE_MANAGE}/user`;
export const ROUTE_USER_CREATE = `${ROUTE_USERS}/create`;
export const ROUTE_USER_EDIT = `${ROUTE_USERS}/edit/[publicId]`;
export const ROUTE_USER_DETAIL = `${ROUTE_USERS}/[publicId]`;

export const ROUTE_INVENTORY = `${ROUTE_MANAGE}/inventory`;
export const ROUTE_INVENTORY_MATERIAL = `${ROUTE_INVENTORY}/material`;
export const ROUTE_INVENTORY_MATERIAL_CREATE = `${ROUTE_INVENTORY}/material/create`;
export const ROUTE_INVENTORY_MATERIAL_DETAIL = `${ROUTE_INVENTORY_MATERIAL}/[publicId]`;
export const ROUTE_INVENTORY_MATERIAL_EDIT = `${ROUTE_INVENTORY_MATERIAL}/edit/[publicId]`;

export const ROUTE_INVENTORY_IMPORT = `${ROUTE_INVENTORY}/import`;
export const ROUTE_INVENTORY_EXPORT = `${ROUTE_INVENTORY}/export`;

export const ROUTE_INVENTORY_DOC_CREATE = `${ROUTE_INVENTORY}/[type]/create`;
export const ROUTE_INVENTORY_DOC_UPDATE = `${ROUTE_INVENTORY}/[type]/update/[publicId]`;
export const ROUTE_INVENTORY_DOC_DETAIL = `${ROUTE_INVENTORY}/[type]/[publicId]`;

export const ROUTE_EQUIPMENT = `${ROUTE_INVENTORY}/equipment`;

export const ROUTE_EQUIPMENT_DETAIL = `${ROUTE_EQUIPMENT}/[publicId]`;

export const ROUTE_CUSTOMER = `${ROUTE_MANAGE}/customer`;
export const ROUTE_CUSTOMER_CREATE = `${ROUTE_CUSTOMER}/create`;
export const ROUTE_CUSTOMER_DETAIL = `${ROUTE_CUSTOMER}/[publicId]`;

export const ROUTE_SUPPLIER = `${ROUTE_MANAGE}/supplier`;
export const ROUTE_SUPPLIER_CREATE = `${ROUTE_SUPPLIER}/create`;
export const ROUTE_SUPPLIER_EDIT = `${ROUTE_SUPPLIER}/edit/[publicId]`;
export const ROUTE_SUPPLIER_DETAIL = `${ROUTE_SUPPLIER}/[publicId]`;

export const ROUTE_SETTINGS = `${ROUTE_MANAGE}/setting`;
export const ROUTE_SETTING_DATA = `${ROUTE_SETTINGS}/data`;
export const ROUTE_SETTING_SYSTEM = `${ROUTE_SETTINGS}/internal`;

export const ROUTE_AUTH = "/auth";
export const ROUTE_LOGIN = `${ROUTE_AUTH}/login`;

export const ROUTE_CASHIER = `${ROUTE_MANAGE}/cashier`;
export const ROUTE_CASHIER_ORDERS = `${ROUTE_CASHIER}/orders`;
export const ROUTE_CASHIER_ORDERS_DETAIL = `${ROUTE_CASHIER_ORDERS}/[publicId]`;

export const ROUTE_VOUCHER = `${ROUTE_MANAGE}/voucher`;
