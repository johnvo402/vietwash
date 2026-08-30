export type PriceItem = {
  id: number;
  name: string;
};

const DB_NAME = "PriceDB";
const STORE_NAME = "prices";
const DB_VERSION = 1;

const openPriceDB = (): Promise<IDBDatabase> => {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);

    request.onupgradeneeded = (event) => {
      const db = (event.target as IDBOpenDBRequest).result;
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME, { keyPath: "id" });
      }
    };

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
};

// Hàm lưu bảng giá
export const savePricesToIndexedDB = async (
  prices: PriceItem[]
): Promise<void> => {
  const db = await openPriceDB();
  const transaction = db.transaction(STORE_NAME, "readwrite");
  const store = transaction.objectStore(STORE_NAME);

  prices.forEach((item) => store.put(item));

  return new Promise((resolve, reject) => {
    transaction.oncomplete = () => resolve();
    transaction.onerror = () => reject(transaction.error);
  });
};

// Hàm lấy bảng giá
export const getPricesFromIndexedDB = async (): Promise<PriceItem[]> => {
  const db = await openPriceDB();
  const transaction = db.transaction(STORE_NAME, "readonly");
  const store = transaction.objectStore(STORE_NAME);
  const request = store.getAll();

  return new Promise((resolve, reject) => {
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
};
