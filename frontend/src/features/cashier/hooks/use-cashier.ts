import { useState, useEffect, useRef } from "react";
import {
  cacheCustomer,
  Customer,
  useCustomers,
} from "@/utils/customer-indexedDb";
import { useOrder } from "./order-composition";
import { PickupTicketRef } from "@/features/orders/components/PickupTicket";
import { useAuth } from "@/hooks/use-auth";
import { useTranslations } from "next-intl";
import { toast } from "react-toastify";
import { ServiceItem, Order, OrderEquipment } from "../types";
import { orderIndexedDB } from "@/utils/indexDb-order";
import { Gender } from "@/api/generated";
import { apiClient } from "@/api/client";
import { usePrices } from "./use-tariff";
import { useQueryClient } from "@tanstack/react-query";
import {
  CashierDraft,
  changeDraftTariff,
  emptyDraft,
  itemKey,
  previewInput,
  restoreDraft,
  selectTariffId,
} from "./cashier-draft";
import { synchronizeCustomer } from "./customer-sync";
import { useOrderPreview } from "./use-order-preview";

export interface OrderTab {
  id: string;
  isActive: boolean;
}
export interface UpdateOrder {
  code?: string;
  orderId?: number;
  branchId: number;
  tariffId: number;
  note: string;
  deliveryTime: string;
  orderItems: {
    serviceId: number;
    unitRelationId: number;
    price: number;
    quantity: number;
    unitRelationName: string;
    processingTime: number;
    serviceName: string;
    unitPrice: number;
  }[];
  customer: Customer;
  orderEquipments?: OrderEquipment[];
}

export function useCashier() {
  const branchId = useAuth((state) => state.branchActive?.branchId) ?? 0;
  const t = useTranslations();
  const queryClient = useQueryClient();
  const { data: customerData } = useCustomers();
  const { data: tariffData } = usePrices(branchId);
  const { createOrder, updateOrder } = useOrder();
  const [state, setState] = useState<Record<string, CashierDraft>>({});
  const stateRef = useRef(state);
  const [orderTabs, setOrderTabs] = useState<OrderTab[]>([]);
  const [activeTab, setActiveTab] = useState("");
  const [hydratedBranch, setHydratedBranch] = useState(0);
  const [processingTab, setProcessingTab] = useState<string | null>(null);
  const processingRef = useRef(false);
  const [customerPhase, setCustomerPhase] = useState<
    "idle" | "creating" | "syncing"
  >("idle");
  const customerBusy = useRef(false);
  const [completedOrder, setCompletedOrder] = useState<Order | null>(null);
  const pickupTicketRef = useRef<PickupTicketRef>(null);
  const orderListRef = useRef<Record<string, HTMLDivElement | null>>({});
  const persistence = useRef(Promise.resolve());

  const replaceState = (next: Record<string, CashierDraft>) => {
    stateRef.current = next;
    setState(next);
  };
  const updateTab = (
    id: string,
    updater: (draft: CashierDraft) => CashierDraft,
  ) => {
    const draft = stateRef.current[id];
    if (!draft || draft.branchId !== branchId) return;
    replaceState({ ...stateRef.current, [id]: updater(draft) });
  };

  useEffect(() => {
    if (!branchId) return;
    let cancelled = false;
    setHydratedBranch(0);
    if (
      Object.values(stateRef.current).some(
        (draft) => draft.branchId !== branchId && draft.items.length,
      )
    ) {
      toast.info(t("cashier.branchDraftReset"));
    }
    void (async () => {
      await persistence.current;
      const tabs = await orderIndexedDB.loadOrderTabs();
      const saved = await Promise.all(
        tabs.map((tab) => orderIndexedDB.loadOrderState(tab.id)),
      );
      if (cancelled) return;
      replaceState(
        Object.fromEntries(
          tabs.map((tab, i) => [tab.id, restoreDraft(saved[i], branchId)]),
        ),
      );
      setOrderTabs(tabs);
      setActiveTab(tabs.find((tab) => tab.isActive)?.id ?? tabs[0].id);
      setHydratedBranch(branchId);
    })().catch(() => {
      if (cancelled) return;
      replaceState({ "#1": emptyDraft(branchId) });
      setOrderTabs([{ id: "#1", isActive: true }]);
      setActiveTab("#1");
      setHydratedBranch(branchId);
      toast.error(t("cashier.loadFailed"));
    });
    return () => {
      cancelled = true;
    };
    // Restoration is tied to branch identity, not changes to draft contents.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [branchId, t]);

  useEffect(() => {
    if (hydratedBranch !== branchId || !branchId || !orderTabs.length) return;
    const persist = () => {
      persistence.current = persistence.current
        .then(async () => {
          await orderIndexedDB.saveOrderTabs(orderTabs);
          await Promise.all(
            orderTabs.map((tab) =>
              orderIndexedDB.saveOrderState(tab.id, state[tab.id]),
            ),
          );
        })
        .catch(() => {
          toast.error(t("cashier.saveFailed"));
        });
    };
    const timer = setTimeout(persist, 250);
    window.addEventListener("pagehide", persist);
    return () => {
      clearTimeout(timer);
      window.removeEventListener("pagehide", persist);
    };
  }, [state, orderTabs, hydratedBranch, branchId, t]);

  useEffect(() => {
    if (!tariffData || hydratedBranch !== branchId) return;
    let changed = false;
    let resetItems = false;
    const next = Object.fromEntries(
      Object.entries(stateRef.current).map(([id, draft]) => {
        const tariffId = selectTariffId(draft.tariffId, tariffData.prices);
        changed ||= tariffId !== draft.tariffId;
        resetItems ||= tariffId !== draft.tariffId && draft.items.length > 0;
        return [id, changeDraftTariff(draft, tariffId)];
      }),
    );
    if (changed) replaceState(next);
    if (resetItems) toast.info(t("cashier.tariffItemsReset"));
  }, [tariffData, hydratedBranch, branchId, state, t]);

  const currentDraft =
    state[activeTab]?.branchId === branchId && hydratedBranch === branchId
      ? state[activeTab]
      : emptyDraft(branchId);
  const pricing = useOrderPreview(previewInput(currentDraft));
  const validTariff = !!tariffData?.prices.some(
    (tariff) => tariff.id === currentDraft.tariffId,
  );
  const readyToSubmit =
    customerPhase === "idle" &&
    !!currentDraft.customer &&
    !currentDraft.pendingCustomerId &&
    currentDraft.items.length > 0 &&
    validTariff &&
    hydratedBranch === branchId &&
    (!!currentDraft.orderId || !!pricing.preview);

  const selectTab = (id: string) => {
    if (processingRef.current) return;
    setActiveTab(id);
    setOrderTabs((tabs) =>
      tabs.map((tab) => ({ ...tab, isActive: tab.id === id })),
    );
  };
  const addNewOrderTab = () => {
    if (processingRef.current || hydratedBranch !== branchId) return;
    let index = 1;
    while (stateRef.current[`#${index}`]) index++;
    const id = `#${index}`;
    replaceState({ ...stateRef.current, [id]: emptyDraft(branchId) });
    setOrderTabs((tabs) => [
      ...tabs.map((tab) => ({ ...tab, isActive: false })),
      { id, isActive: true },
    ]);
    setActiveTab(id);
  };
  const removeOrderTab = async (id: string) => {
    if (processingRef.current || orderTabs.length <= 1) return;
    const next = { ...stateRef.current };
    delete next[id];
    replaceState(next);
    const nextId = activeTab === id ? Object.keys(next)[0] : activeTab;
    setActiveTab(nextId);
    setOrderTabs((tabs) =>
      tabs
        .filter((tab) => tab.id !== id)
        .map((tab) => ({ ...tab, isActive: tab.id === nextId })),
    );
    await orderIndexedDB.clearOrderState(id);
  };

  const handleSelectTariff = (tariffId: number) => {
    if (
      !tariffData?.prices.some((tariff) => tariff.id === tariffId) ||
      processingRef.current
    )
      return;
    if (currentDraft.tariffId !== tariffId && currentDraft.items.length)
      toast.info(t("cashier.tariffItemsReset"));
    updateTab(activeTab, (draft) => changeDraftTariff(draft, tariffId));
  };
  const handleAddItem = (item: ServiceItem, id: string) => {
    if (
      processingRef.current ||
      !validTariff ||
      !Number.isInteger(item.quantity) ||
      item.quantity <= 0
    )
      return;
    updateTab(id, (draft) => {
      const exists = draft.items.some((row) => itemKey(row) === itemKey(item));
      return {
        ...draft,
        items: exists
          ? draft.items.map((row) =>
              itemKey(row) === itemKey(item)
                ? { ...row, quantity: row.quantity + item.quantity }
                : row,
            )
          : [...draft.items, item],
      };
    });
  };
  const handleRemoveItem = (key: string, id: string) => {
    if (!processingRef.current)
      updateTab(id, (draft) => ({
        ...draft,
        items: draft.items.filter((item) => itemKey(item) !== key),
      }));
  };
  const handleUpdateQuantity = (key: string, quantity: number, id: string) => {
    if (processingRef.current || !Number.isInteger(quantity) || quantity <= 0)
      return;
    updateTab(id, (draft) => ({
      ...draft,
      items: draft.items.map((item) =>
        itemKey(item) === key ? { ...item, quantity } : item,
      ),
    }));
  };
  const handleSelectCustomer = (customer: Customer | null, id: string) => {
    if (!processingRef.current)
      updateTab(id, (draft) => ({ ...draft, customer, voucherCode: "" }));
  };
  const handleApplyVoucher = (voucherCode: string, id: string) => {
    if (!processingRef.current)
      updateTab(id, (draft) => ({ ...draft, voucherCode: voucherCode.trim() }));
  };
  const handleSetNote = (note: string | null, id: string) => {
    if (!processingRef.current)
      updateTab(id, (draft) => ({ ...draft, note: note ?? "" }));
  };
  const handleSetDeliveryTime = (date: Date | undefined, id: string) => {
    if (!processingRef.current)
      updateTab(id, (draft) => ({
        ...draft,
        deliveryTime: date?.toISOString() ?? "",
      }));
  };

  const handleUpdateOrder = async (order: UpdateOrder) => {
    if (order.branchId !== branchId) {
      toast.error(t("cashier.branchDraftReset"));
      return;
    }
    const id = `#${order.code}`;
    const draft: CashierDraft = {
      ...emptyDraft(branchId),
      ...order,
      orderId: order.orderId ?? null,
      items: order.orderItems.map((item) => ({
        ...item,
        id: item.serviceId,
        name: item.serviceName,
      })),
    };
    const tabs = [
      ...orderTabs
        .filter((tab) => tab.id !== id)
        .map((tab) => ({ ...tab, isActive: false })),
      { id, isActive: true },
    ];
    replaceState({ ...stateRef.current, [id]: draft });
    setOrderTabs(tabs);
    setActiveTab(id);
    await persistence.current;
    await orderIndexedDB.saveOrderState(id, draft);
    await orderIndexedDB.saveOrderTabs(tabs);
  };

  const handleProcessOrder = async (id: string) => {
    const draft = stateRef.current[id];
    if (
      processingRef.current ||
      customerBusy.current ||
      id !== activeTab ||
      !draft ||
      draft.branchId !== branchId ||
      !readyToSubmit ||
      (!draft.orderId &&
        JSON.stringify(previewInput(draft)) !==
          JSON.stringify(previewInput(currentDraft)))
    )
      return;
    if (!draft.deliveryTime) {
      toast.error(t("cashier.pickupTimeRequired"));
      return;
    }
    processingRef.current = true;
    setProcessingTab(id);
    const payload: Order = {
      id: draft.orderId ?? undefined,
      customer: draft.customer,
      orderItems: draft.items,
      branchId: draft.branchId,
      tariffId: draft.tariffId,
      voucherCode: draft.voucherCode || undefined,
      note: draft.note,
      deliveryTime: new Date(draft.deliveryTime),
      // Legacy DTO fields are never sent by order-composition.
      amount: 0,
      total: 0,
      discountValue: 0,
      discountFixed: false,
    };
    try {
      if (draft.orderId) {
        await updateOrder.mutateAsync(payload);
      } else {
        const response = await createOrder.mutateAsync(payload);
        setCompletedOrder(response.data.results as Order);
      }
    } catch (error) {
      const title = (error as { response?: { data?: { title?: string } } })
        .response?.data?.title;
      toast.error(title || t("cashier.createRetry"));
      if (!draft.orderId) void pricing.retry();
      return; // Preserve customer, items, note, tariff and pickup time on failure.
    } finally {
      processingRef.current = false;
      setProcessingTab(null);
    }
    // A cache/print failure after server success must never invite a duplicate create.
    if (stateRef.current[id]?.branchId === draft.branchId)
      replaceState({ ...stateRef.current, [id]: emptyDraft(draft.branchId) });
    try {
      await persistence.current;
      await orderIndexedDB.saveOrderState(
        id,
        stateRef.current[id] ?? emptyDraft(branchId),
      );
    } catch {
      toast.error(t("cashier.saveFailed"));
    }
    void queryClient.invalidateQueries({ queryKey: ["orders"] });
    toast.success(
      t(draft.orderId ? "cashier.updateOrder" : "cashier.create.withoutError"),
    );
    if (!draft.orderId) setTimeout(() => pickupTicketRef.current?.print(), 100);
  };

  const createCustomerHandle = async (data: {
    displayName: string;
    phoneNumber: string;
    gender: Gender;
  }) => {
    if (customerBusy.current)
      throw new Error(t("cashier.customerSynchronizing"));
    const tabId = activeTab;
    const originBranch = branchId;
    if (!stateRef.current[tabId] || hydratedBranch !== branchId)
      throw new Error(t("cashier.loadFailed"));
    customerBusy.current = true;
    let customerId = stateRef.current[tabId].pendingCustomerId;
    try {
      if (!customerId) {
        setCustomerPhase("creating");
        const response = await apiClient.authApiCustomersPost(data);
        customerId = response.data.results?.id ?? null;
        if (!customerId) throw new Error(t("common.error.submissionFailed"));
        updateTab(tabId, (draft) => ({
          ...draft,
          pendingCustomerId: customerId,
          customer: null,
          voucherCode: "",
        }));
        await persistence.current;
        await orderIndexedDB.saveOrderState(tabId, stateRef.current[tabId]);
      }
      setCustomerPhase("syncing");
      await synchronizeCustomer(
        customerId,
        async (id) => {
          const response = await apiClient.ecommerceApiUsersIdGet(id, {
            timeout: 4000,
          });
          const synced = response.data.results;
          if (
            !synced?.id ||
            synced.id !== id ||
            synced.role !== "CUSTOMER" ||
            synced.status !== "Active"
          ) {
            throw new Error(t("cashier.customerSyncPending"));
          }
          return {
            ...synced,
            id: synced.id,
            displayName: synced.displayName ?? "",
            email: synced.email ?? undefined,
          } as Customer;
        },
        async (customer) => {
          await cacheCustomer(customer);
          await queryClient.invalidateQueries({ queryKey: ["customer"] });
          queryClient.setQueryData<{ users: Customer[] }>(
            ["customer"],
            (previous) => ({
              users: [
                ...(previous?.users ?? []).filter(
                  (row) => row.id !== customer.id,
                ),
                customer,
              ],
            }),
          );
          if (stateRef.current[tabId]?.branchId === originBranch) {
            updateTab(tabId, (draft) => ({
              ...draft,
              customer,
              pendingCustomerId: null,
            }));
          }
        },
      );
    } catch (error) {
      if (customerId) throw new Error(t("cashier.customerSyncPending"));
      throw error;
    } finally {
      customerBusy.current = false;
      setCustomerPhase("idle");
    }
  };

  const byTab = <T>(select: (draft: CashierDraft) => T) =>
    Object.fromEntries(
      orderTabs.map((tab) => [
        tab.id,
        select(
          state[tab.id]?.branchId === branchId && hydratedBranch === branchId
            ? state[tab.id]
            : emptyDraft(branchId),
        ),
      ]),
    );
  const customers = new Map(
    (customerData?.users ?? []).map((customer) => [customer.id, customer]),
  );
  Object.values(state).forEach((draft) => {
    if (draft.customer) customers.set(draft.customer.id, draft.customer);
  });
  return {
    customer: byTab((draft) => draft.customer),
    items: byTab((draft) => draft.items),
    tariffId: byTab((draft) =>
      tariffData?.prices.some((tariff) => tariff.id === draft.tariffId)
        ? draft.tariffId
        : 0,
    ),
    voucherCode: byTab((draft) => draft.voucherCode),
    note: byTab((draft) => draft.note),
    deliveryTime: byTab((draft) => draft.deliveryTime),
    orderId: byTab((draft) => draft.orderId),
    isProcessing: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, processingTab === tab.id]),
    ),
    customerData: [...customers.values()],
    tariffData,
    customerPhase,
    customerPending: !!currentDraft.pendingCustomerId,
    readyToSubmit,
    pricing,
    completedOrder,
    pickupTicketRef,
    orderListRef,
    orderTabs,
    activeTab,
    handleSelectTariff,
    handleAddItem,
    handleRemoveItem,
    handleUpdateQuantity,
    handleSelectCustomer,
    handleSetNote,
    handleSetDeliveryTime,
    handleApplyVoucher,
    handleProcessOrder,
    handleUpdateOrder,
    createCustomerHandle,
    addNewOrderTab,
    removeOrderTab,
    setActiveTab: selectTab,
    handlePrint: () => pickupTicketRef.current?.print(),
  };
}
