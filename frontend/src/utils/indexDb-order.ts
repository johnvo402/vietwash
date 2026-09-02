import type { OrderTab } from "@/features/cashier/hooks/use-cashier";
import type { CashierDraft } from "@/features/cashier/hooks/cashier-draft";

type OrderState = CashierDraft;

export class OrderIndexedDB {
  private dbName = "cashier-orders";
  private dbVersion = 1; // no bump needed; object stores are schemaless
  private db: IDBDatabase | null = null;

  async initialize(): Promise<void> {
    return new Promise((resolve, reject) => {
      const request = window.indexedDB.open(this.dbName, this.dbVersion);

      request.onupgradeneeded = () => {
        const db = request.result;
        try {
          if (!db.objectStoreNames.contains("orderTabs")) {
            db.createObjectStore("orderTabs", { keyPath: "id" });
          }
          if (!db.objectStoreNames.contains("orderState")) {
            db.createObjectStore("orderState", { keyPath: "tabId" });
          }
        } catch (error) {
          reject(new Error("Failed to upgrade IndexedDB schema"));
        }
      };

      request.onsuccess = () => {
        this.db = request.result;
        resolve();
      };
      request.onerror = () => reject(new Error("Failed to open IndexedDB"));
    });
  }

  private async ensureInitialized(): Promise<IDBDatabase> {
    if (!this.db) await this.initialize();
    if (!this.db) throw new Error("IndexedDB not initialized");
    return this.db as IDBDatabase;
  }

  async saveOrderTabs(tabs: OrderTab[]): Promise<void> {
    const db = await this.ensureInitialized();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(["orderTabs"], "readwrite");
      const store = tx.objectStore("orderTabs");
      const clearRequest = store.clear();
      clearRequest.onsuccess = () => {
        tabs.forEach((tab) => store.put(tab));
      };
      clearRequest.onerror = () =>
        reject(new Error("Failed to clear order tabs"));
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(new Error("Failed to save order tabs"));
    });
  }

  async loadOrderTabs(): Promise<OrderTab[]> {
    const db = await this.ensureInitialized();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(["orderTabs"], "readonly");
      const store = tx.objectStore("orderTabs");
      const request = store.getAll();
      request.onsuccess = () => {
        const tabs =
          request.result.length > 0
            ? request.result
            : [{ id: "#1", isActive: true }];
        resolve(tabs);
      };
      request.onerror = () => reject(new Error("Failed to load order tabs"));
    });
  }

  async saveOrderState(tabId: string, state: OrderState): Promise<void> {
    const db = await this.ensureInitialized();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(["orderState"], "readwrite");
      const store = tx.objectStore("orderState");
      const request = store.put({ tabId, ...state });
      request.onsuccess = () => (tx.oncomplete = () => resolve());
      request.onerror = () => reject(new Error("Failed to save order state"));
    });
  }

  async loadOrderState(tabId: string): Promise<OrderState | undefined> {
    const db = await this.ensureInitialized();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(["orderState"], "readonly");
      const store = tx.objectStore("orderState");
      const request = store.get(tabId);
      request.onsuccess = () => {
        const result = request.result;
        if (!result) return resolve(undefined);
        resolve({
          branchId: typeof result.branchId === "number" ? result.branchId : 0,
          pendingCustomerId:
            typeof result.pendingCustomerId === "number"
              ? result.pendingCustomerId
              : null,
          customer: result.customer || null,
          items: Array.isArray(result.items) ? result.items : [],
          voucherCode:
            typeof result.voucherCode === "string" ? result.voucherCode : "",
          note: typeof result.note === "string" ? result.note : "",
          tariffId: typeof result.tariffId === "number" ? result.tariffId : 0,
          deliveryTime:
            typeof result.deliveryTime === "string" ? result.deliveryTime : "",
          orderId: typeof result.orderId === "number" ? result.orderId : null,
          // NEW: load equipments (fallback to empty array)
          orderEquipments: Array.isArray(result.orderEquipments)
            ? result.orderEquipments
            : [],
        });
      };
      request.onerror = () => reject(new Error("Failed to load order state"));
    });
  }

  async clearOrderState(tabId: string): Promise<void> {
    const db = await this.ensureInitialized();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(["orderState"], "readwrite");
      const store = tx.objectStore("orderState");
      const request = store.delete(tabId);
      request.onsuccess = () => (tx.oncomplete = () => resolve());
      request.onerror = () => reject(new Error("Failed to clear order state"));
    });
  }

  async deleteOrderTab(tabId: string): Promise<void> {
    const db = await this.ensureInitialized();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(["orderTabs"], "readwrite");
      const store = tx.objectStore("orderTabs");
      const request = store.delete(tabId);
      request.onsuccess = () => (tx.oncomplete = () => resolve());
      request.onerror = () => reject(new Error("Failed to delete order tab"));
    });
  }
}

export const orderIndexedDB = new OrderIndexedDB();
