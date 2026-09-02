// index.ts (trong thư mục Db hoặc tương tự)

// 1. Định nghĩa và export type Customer
export type Customer = {
  id: number;
  displayName: string;
  email?: string;
  phoneNumber?: string;
  customerGroup?: CustomerGroup;
};

const DB_NAME = "CustomerDB";
const STORE_NAME = "customers";
const DB_VERSION = 1;

const openDB = (): Promise<IDBDatabase> => {
  return new Promise((resolve, reject) => {
    const request: IDBOpenDBRequest = indexedDB.open(DB_NAME, DB_VERSION);

    request.onupgradeneeded = (event: IDBVersionChangeEvent) => {
      const db = (event.target as IDBOpenDBRequest).result;
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME, { keyPath: "id" });
      }
    };

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
};

export const cacheCustomer = async (customer: Customer): Promise<void> => {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(STORE_NAME, "readwrite");
    transaction.objectStore(STORE_NAME).put(customer);
    transaction.oncomplete = () => {
      db.close();
      resolve();
    };
    transaction.onerror = () => {
      db.close();
      reject(transaction.error);
    };
    transaction.onabort = () => {
      db.close();
      reject(transaction.error);
    };
  });
};

// 5. Hàm lưu dữ liệu vào IndexedDB với ánh xạ
const saveToIndexedDB = async (
  apiCustomers: ListUserResponse[],
): Promise<void> => {
  const db: IDBDatabase = await openDB();
  const transaction: IDBTransaction = db.transaction(STORE_NAME, "readwrite");
  const store: IDBObjectStore = transaction.objectStore(STORE_NAME);

  apiCustomers.forEach((apiCustomer: ListUserResponse) => {
    const customer: Customer = {
      id: apiCustomer.id!, // Đảm bảo id luôn có giá trị
      displayName: `${apiCustomer.displayName || ""}`.trim() || "Unknown", // Ghép tên, mặc định "Unknown" nếu thiếu
      phoneNumber: apiCustomer.phoneNumber || "", // Lấy phoneNumber từ API
      email: apiCustomer.email || "", // Lấy email từ API
      customerGroup: apiCustomer.customerGroup || undefined, // Lấy customerGroup từ API,
    };
    store.put(customer); // Lưu customer đã ánh xạ
  });

  return new Promise((resolve, reject) => {
    transaction.oncomplete = () => resolve();
    transaction.onerror = () => reject(transaction.error);
  });
};

// 6. Hàm lấy dữ liệu từ IndexedDB
const getFromIndexedDB = async (): Promise<Customer[]> => {
  const db: IDBDatabase = await openDB();
  const transaction: IDBTransaction = db.transaction(STORE_NAME, "readonly");
  const store: IDBObjectStore = transaction.objectStore(STORE_NAME);
  const request: IDBRequest<Customer[]> = store.getAll();

  return new Promise((resolve, reject) => {
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
};

// 7. API Client và useQuery
import { apiClient } from "@/api/client";
import { CustomerGroup, ListUserResponse } from "@/api/generated/api";
import { useQuery, UseQueryResult } from "@tanstack/react-query";

const fetchCustomers = async (): Promise<{ users: Customer[] }> => {
  const response = await apiClient.ecommerceApiUsersGet();
  const apiCustomers: ListUserResponse[] = response.data.results?.data || [];

  // Ánh xạ từ ListUserResponse sang Customer
  const customers = apiCustomers.map(
    (apiCustomer) =>
      ({
        id: apiCustomer.id, // Đảm bảo id luôn có giá trị
        displayName: `${apiCustomer.displayName || ""} `.trim() || "Unknown",
        phoneNumber: apiCustomer.phoneNumber || "",
        email: apiCustomer.email || "",
        customerGroup: apiCustomer.customerGroup,
      }) as Customer,
  );

  await saveToIndexedDB(apiCustomers); // Lưu dữ liệu vào IndexedDB
  return { users: customers };
};

// 8. Hook useQuery với type đầy đủ
export const useCustomers = (): UseQueryResult<
  { users: Customer[] },
  Error
> => {
  return useQuery<{ users: Customer[] }, Error>({
    queryKey: ["customer"],
    queryFn: fetchCustomers,
    initialData: { users: [] },
  });
};

// 9. Hàm preload dữ liệu từ IndexedDB
export const preloadCustomers = async (): Promise<Customer[]> => {
  const cachedCustomers: Customer[] = await getFromIndexedDB();
  if (cachedCustomers.length > 0) {
    return cachedCustomers;
  }
  const { users }: { users: Customer[] } = await fetchCustomers();
  return users;
};
