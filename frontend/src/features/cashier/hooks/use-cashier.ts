/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect, useRef, useCallback } from "react";
import { Customer, useCustomers } from "@/utils/customer-indexedDb";
import { useOrder } from "./order-composition";
import { PickupTicketRef } from "@/features/orders/components/PickupTicket";
import { useAuth } from "@/hooks/use-auth";
import { useTranslations } from "next-intl";
import { toast } from "react-toastify";
import { ServiceItem, Order, OrderEquipment } from "../types";
import { orderIndexedDB } from "@/utils/indexDb-order";
import debounce from "lodash/debounce";
import { useStringUtil } from "@/lib/stringUtil";
import { useCustomerMutations } from "@/features/customer/hooks/use-customer-hook";
import { Gender } from "@/api/generated";
import { formatPriceVN } from "@/utils/format";
import { usePrices } from "./use-tariff";
import { useQueryClient } from "@tanstack/react-query";

export interface OrderTab {
  id: string;
  isActive: boolean;
}

interface OrderItem {
  serviceId: number;
  unitRelationId: number;
  price: number;
  quantity: number;
  unitRelationName: string;
  processingTime: number;
  serviceName: string;
  unitPrice: number;
}

// NEW: equipment type used by payload and UI

export interface UpdateOrder {
  code?: string;
  orderId?: number;
  tariffId: number;
  point: number;
  note: string;
  deliveryTime: string;
  orderItems: OrderItem[];
  customer: Customer;
  // NEW: include equipments when editing an order (optional)
  orderEquipments?: OrderEquipment[];
}

let tabIdCounter = 1;

export const useCashier = () => {
  const { branchActive } = useAuth();
  const t = useTranslations();
  const { data: customerData } = useCustomers();
  const { createOrder, checkVoucher, updateOrder } = useOrder();
  const { createCustomer } = useCustomerMutations();
  const { textByLang } = useStringUtil();
  const { data: tariffData } = usePrices(branchActive?.branchId!);
  const queryClient = useQueryClient();

  const pickupTicketRef = useRef<PickupTicketRef>(null);
  const orderListRef = useRef<{ [key: string]: HTMLDivElement | null }>({});

  const initializeTabs = async (): Promise<OrderTab[]> => {
    try {
      await orderIndexedDB.initialize();
      const savedTabs = await orderIndexedDB.loadOrderTabs();
      if (savedTabs.length > 0) {
        const maxId = Math.max(
          ...savedTabs.map((tab) => {
            const num = parseInt(tab.id.replace("#", ""), 10);
            return isNaN(num) ? 0 : num;
          })
        );
        tabIdCounter = Math.max(tabIdCounter, maxId + 1);
      }
      const activeTabExists = savedTabs.some((tab) => tab.isActive);
      const tabs =
        savedTabs.length > 0
          ? savedTabs.map((tab, index) => ({
              ...tab,
              isActive: activeTabExists ? tab.isActive : index === 0,
            }))
          : [{ id: `#${tabIdCounter++}`, isActive: true }];
      return tabs;
    } catch (error) {
      console.error("Lỗi khởi tạo tabs:", error);
      return [{ id: `#${tabIdCounter++}`, isActive: true }];
    }
  };

  const initializeTabState = (tabId: string) => ({
    customer: null as Customer | null,
    items: [] as ServiceItem[],
    isProcessing: false as boolean,
    total: 0 as number,
    amount: 0 as number,
    discountValue: 0 as number,
    discountFixed: true as boolean,
    voucherCode: "" as string,
    note: "" as string,
    tariffId: 0 as number,
    point: 0 as number,
    deliveryTime: "" as string,
    orderId: null as number | null,
    // NEW: per-tab equipments
    orderEquipments: [] as OrderEquipment[],
  });

  const loadInitialState = async () => {
    try {
      const tabs = await initializeTabs();
      const initialState: {
        [key: string]: ReturnType<typeof initializeTabState>;
      } = {};
      for (const tab of tabs) {
        const savedState = await orderIndexedDB.loadOrderState(tab.id);
        initialState[tab.id] = savedState
          ? {
              ...initializeTabState(tab.id),
              customer: savedState.customer || null,
              items: Array.isArray(savedState.items) ? savedState.items : [],
              total:
                typeof savedState.total === "number" ? savedState.total : 0,
              amount:
                typeof savedState.amount === "number" ? savedState.amount : 0,
              discountValue:
                typeof savedState.discountValue === "number"
                  ? savedState.discountValue
                  : 0,
              discountFixed:
                typeof savedState.discountFixed === "boolean"
                  ? savedState.discountFixed
                  : true,
              voucherCode:
                typeof savedState.voucherCode === "string"
                  ? savedState.voucherCode
                  : "",
              note: typeof savedState.note === "string" ? savedState.note : "",
              tariffId:
                typeof savedState.tariffId === "number"
                  ? savedState.tariffId
                  : 0,
              point:
                typeof savedState.point === "number" ? savedState.point : 0,
              deliveryTime:
                typeof savedState.deliveryTime === "string"
                  ? savedState.deliveryTime
                  : "",
              orderId:
                typeof savedState.orderId === "number"
                  ? savedState.orderId
                  : null,
              // NEW: restore equipments if present
              orderEquipments: Array.isArray(savedState.orderEquipments)
                ? savedState.orderEquipments
                : [],
            }
          : initializeTabState(tab.id);
      }
      setOrderTabs(tabs);
      const activeTabId = tabs.find((tab) => tab.isActive)?.id || tabs[0].id;
      setActiveTab(activeTabId);
      return initialState;
    } catch (error) {
      console.error("Lỗi load state ban đầu:", error);
      const defaultTabId = `#${tabIdCounter}`;
      const defaultState = {
        [defaultTabId]: initializeTabState(defaultTabId),
      };
      setState(defaultState);
      setOrderTabs([{ id: defaultTabId, isActive: true }]);
      setActiveTab(defaultTabId);
      tabIdCounter++;
      toast.error(t("cashier.loadFailed"));
      return defaultState;
    }
  };

  type TabState = ReturnType<typeof initializeTabState>;
  const [state, setState] = useState<{ [key: string]: TabState }>({});
  const [orderTabs, setOrderTabs] = useState<OrderTab[]>([]);
  const [activeTab, setActiveTab] = useState<string>("");
  const [completedOrder, setCompletedOrder] = useState<Order | null>(null);

  useEffect(() => {
    loadInitialState().then((initialState) => setState(initialState));
  }, []);

  const saveToIndexedDB = useCallback(
    debounce(async (tabId: string) => {
      if (!state[tabId]) return;
      try {
        await orderIndexedDB.saveOrderState(tabId, {
          customer: state[tabId].customer,
          items: state[tabId].items,
          total: state[tabId].total,
          amount: state[tabId].amount,
          discountValue: state[tabId].discountValue,
          discountFixed: state[tabId].discountFixed,
          voucherCode: state[tabId].voucherCode,
          note: state[tabId].note,
          tariffId: state[tabId].tariffId,
          point: state[tabId].point,
          deliveryTime: state[tabId].deliveryTime,
          orderId: state[tabId].orderId,
          // NEW
          orderEquipments: state[tabId].orderEquipments,
        });
      } catch (error) {
        console.error(`Lỗi lưu state cho tab ${tabId}:`, error);
        toast.error(t("cashier.saveFailed"));
      }
    }, 500),
    [state, t]
  );

  useEffect(() => {
    Object.keys(state).forEach((tabId) => {
      saveToIndexedDB(tabId);
    });
  }, [state, saveToIndexedDB]);

  useEffect(() => {
    const handleBeforeUnload = async () => {
      try {
        await orderIndexedDB.saveOrderTabs(orderTabs);
        for (const tab of orderTabs) {
          if (state[tab.id]) {
            await orderIndexedDB.saveOrderState(tab.id, {
              customer: state[tab.id].customer,
              items: state[tab.id].items,
              total: state[tab.id].total,
              amount: state[tab.id].amount,
              discountValue: state[tab.id].discountValue,
              discountFixed: state[tab.id].discountFixed,
              voucherCode: state[tab.id].voucherCode,
              note: state[tab.id].note,
              tariffId: state[tab.id].tariffId,
              point: state[tab.id].point,
              deliveryTime: state[tab.id].deliveryTime,
              orderId: state[tab.id].orderId,
              orderEquipments: state[tab.id].orderEquipments,
            });
          }
        }
      } catch (error) {
        console.error("Lỗi lưu state trước khi unload:", error);
      }
    };
    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [orderTabs, state]);

  const updateActiveTab = (newActiveTabId: string) => {
    setOrderTabs((prevTabs) => {
      const updatedTabs = prevTabs.map((tab) => ({
        ...tab,
        isActive: tab.id === newActiveTabId,
      }));
      orderIndexedDB.saveOrderTabs(updatedTabs).catch((error) => {
        console.error("Lỗi lưu order tabs:", error);
        toast.error(t("cashier.saveFailed"));
      });
      return updatedTabs;
    });
    setActiveTab(newActiveTabId);
  };

  const addNewOrderTab = () => {
    const newTabId = `#${tabIdCounter++}`;
    const newTabs = [
      ...orderTabs.map((tab) => ({ ...tab, isActive: false })),
      { id: newTabId, isActive: true },
    ];
    setOrderTabs(newTabs);
    setActiveTab(newTabId);
    setState((prev) => ({
      ...prev,
      [newTabId]: initializeTabState(newTabId),
    }));
    orderIndexedDB.saveOrderTabs(newTabs).catch((error) => {
      console.error("Lỗi lưu order tabs:", error);
      toast.error(t("cashier.saveFailed"));
    });
    return newTabId;
  };

  const removeOrderTab = async (tabId: string) => {
    const isOnlyOneTab = orderTabs.length === 1;
    if (isOnlyOneTab) {
      const remainingTab = orderTabs[0];
      if (remainingTab.id !== "#1") {
        try {
          await renameTabToOne(remainingTab.id, state);
        } catch (error) {
          console.error(error);
          toast.error(t("cashier.renameTabFailed"));
        }
      }
      return;
    }

    try {
      const updatedTabs = orderTabs
        .filter((tab) => tab.id !== tabId)
        .map((tab, index) => ({
          ...tab,
          isActive: activeTab === tabId && index === 0 ? true : tab.isActive,
        }));

      await orderIndexedDB.deleteOrderTab(tabId);
      await orderIndexedDB.clearOrderState(tabId);
      await orderIndexedDB.saveOrderTabs(updatedTabs);

      setOrderTabs(updatedTabs);
      let index = 1;
      if (activeTab === tabId) {
        index = updatedTabs.length;
      } else {
        index =
          updatedTabs.findIndex((x) => x.id === activeTab) > 0
            ? updatedTabs.findIndex((x) => x.id === tabId)
            : 1;
      }
      if (activeTab === tabId && updatedTabs.length > 0) {
        updateActiveTab(updatedTabs[index - 1].id);
      }

      setState((prev) => {
        const newState = { ...prev } as any;
        delete newState[tabId];
        return newState;
      });

      if (updatedTabs.length === 1 && updatedTabs[0].id !== "#1") {
        await renameTabToOne(updatedTabs[0].id, state);
      }
    } catch (error) {
      console.error(`Lỗi xóa tab ${tabId}:`, error);
      toast.error(t("cashier.removeTabFailed"));
    }
  };

  const renameTabToOne = async (
    remainingTabId: string,
    currentState: typeof state
  ) => {
    const newState = { ...currentState[remainingTabId] };

    await orderIndexedDB.clearOrderState(remainingTabId);
    await orderIndexedDB.saveOrderState("#1", {
      customer: newState.customer,
      items: newState.items,
      total: newState.total,
      amount: newState.amount,
      discountValue: newState.discountValue,
      discountFixed: newState.discountFixed,
      voucherCode: newState.voucherCode,
      note: newState.note || "",
      tariffId: newState.tariffId,
      point: newState.point,
      deliveryTime: newState.deliveryTime,
      orderId: newState.orderId,
      orderEquipments: newState.orderEquipments,
    });

    const newTabs = [{ id: "#1", isActive: true }];
    await orderIndexedDB.deleteOrderTab(remainingTabId);
    await orderIndexedDB.saveOrderTabs(newTabs);

    setOrderTabs(newTabs);
    setActiveTab("#1");
    setState((prev) => {
      const newStateCopy = { ...prev } as any;
      newStateCopy["#1"] = newState;
      delete newStateCopy[remainingTabId];
      return newStateCopy;
    });

    tabIdCounter = 2;
  };

  const validateVoucher = async (
    voucherCode: string,
    customerId: number
  ): Promise<{
    discountValue: number;
    discountFixed: boolean;
    message: string;
  }> => {
    try {
      const response = await checkVoucher.mutateAsync({
        code: voucherCode,
        customerId,
      });
      const data = response.data;
      const value = data.results?.discountFixed
        ? formatPriceVN(data.results?.discountValue ?? 0)
        : `${data.results?.discountValue}%`;
      return {
        discountValue: data.results?.discountValue || 0,
        discountFixed: data.results?.discountFixed!,
        message: t("cashier.voucherApplied", { value }),
      };
    } catch (error: any) {
      const invalidParam = error?.invalidParams?.[0];
      const reason = invalidParam?.reasons?.[0];
      return {
        discountValue: 0,
        discountFixed: true,
        message: textByLang(reason) || t("cashier.invalidVoucher"),
      };
    }
  };

  const updateTotal = (tabId: string) => {
    updateStateForTab(tabId, (prev) => {
      const amount =
        prev.items.length > 0
          ? prev.items.reduce(
              (sum, item) => sum + item.price * item.quantity,
              0
            )
          : 0;
      const discount = prev.discountFixed
        ? prev.discountValue
        : (amount * prev.discountValue) / 100;
      const pointsDeduction = prev.point * 10;

      // Tính VAT 10%
      const vat = (amount - discount - pointsDeduction) * 0.1;

      const total = Math.max(0, amount - discount - pointsDeduction + vat);

      return { amount, total } as Partial<TabState>;
    });
  };

  const updateStateForTab = (
    tabId: string,
    updater: (prev: TabState) => Partial<TabState>
  ) => {
    setState((prev) => {
      const base = prev[tabId] || initializeTabState(tabId);
      const updatedTabState = { ...base, ...updater(base) } as TabState;
      return { ...prev, [tabId]: updatedTabState };
    });
    saveToIndexedDB(tabId);
  };

  const handleAddItem = (item: ServiceItem, tabId: string) => {
    if (item.quantity <= 0 || item.price < 0) {
      toast.error(t("cashier.quantityAndPriceValidation"));
      return;
    }
    updateStateForTab(tabId, (prev) => {
      const existingItem = prev.items.find((i) => i.id === item.id);
      const updatedItems = existingItem
        ? prev.items.map((i) =>
            i.id === item.id
              ? { ...i, quantity: i.quantity + item.quantity }
              : i
          )
        : [...prev.items, item];
      return { items: updatedItems } as Partial<TabState>;
    });
    updateTotal(tabId);
    setTimeout(() => {
      const container = orderListRef.current[tabId];
      if (container) container.scrollTop = container.scrollHeight;
    }, 0);
  };

  const handleRemoveItem = (itemId: number, tabId: string) => {
    updateStateForTab(
      tabId,
      (prev) =>
        ({
          items: prev.items.filter((item) => item.id !== itemId),
        }) as Partial<TabState>
    );
    updateTotal(tabId);
  };

  const handleUpdateQuantity = (
    itemId: number,
    quantity: number,
    tabId: string
  ) => {
    if (quantity <= 0) {
      toast.error(t("cashier.quantityValidation"));
      return;
    }
    updateStateForTab(
      tabId,
      (prev) =>
        ({
          items: prev.items.map((item) =>
            item.id === itemId ? { ...item, quantity } : item
          ),
        }) as Partial<TabState>
    );
    updateTotal(tabId);
  };

  const handleUpdatePrice = (itemId: number, price: number, tabId: string) => {
    if (price < 0) {
      toast.error(t("cashier.priceMustBePositive"));
      return;
    }
    updateStateForTab(
      tabId,
      (prev) =>
        ({
          items: prev.items.map((item) =>
            item.id === itemId ? { ...item, price } : item
          ),
        }) as Partial<TabState>
    );
    updateTotal(tabId);
  };

  const handleSelectCustomer = (customer: Customer | null, tabId: string) => {
    updateStateForTab(
      tabId,
      () => ({ customer, point: 0 }) as Partial<TabState>
    );
    if (!customer) {
      updateStateForTab(
        tabId,
        () =>
          ({
            discountValue: 0,
            discountFixed: true,
            voucherCode: "",
            point: 0,
          }) as Partial<TabState>
      );
    }
  };

  const handleSetNote = (note: string | null, tabId: string) => {
    updateStateForTab(tabId, () => ({ note: note || "" }) as Partial<TabState>);
  };

  const handleApplyVoucher = async (voucherCode: string, tabId: string) => {
    if (!voucherCode) {
      return {
        discountValue: 0,
        discountFixed: true,
        message: t("cashier.enterVoucherCode"),
      };
    }
    const customer = state[tabId]?.customer;
    const result = await validateVoucher(voucherCode, customer?.id!);
    updateStateForTab(
      tabId,
      () =>
        ({
          discountValue: result.discountValue,
          discountFixed: result.discountFixed,
          voucherCode,
        }) as Partial<TabState>
    );
    updateTotal(tabId);
    return result;
  };

  const handleUpdatePoints = (
    points: number,
    tabId: string,
    maxPoints: number
  ) => {
    if (points < 0 || points > maxPoints) return;
    updateStateForTab(tabId, () => ({ point: points }) as Partial<TabState>);
    updateTotal(tabId);
  };

  // NEW: toggle equipment selection for a tab
  const handleToggleEquipment = (eq: OrderEquipment, tabId: string) => {
    updateStateForTab(tabId, (prev) => {
      const exists = prev.orderEquipments.some(
        (x) => x.equipmentId === eq.equipmentId
      );
      const next = exists
        ? prev.orderEquipments.filter((x) => x.equipmentId !== eq.equipmentId)
        : [...prev.orderEquipments, eq];
      return { orderEquipments: next } as Partial<TabState>;
    });
  };

  const handleUpdateOrder = (order: UpdateOrder) => {
    const tabId = `#${order.code}`;
    const existingTab = orderTabs.find((tab) => tab.id === tabId);

    if (!existingTab) {
      const newTabs = [
        ...orderTabs.map((tab) => ({ ...tab, isActive: false })),
        { id: tabId, isActive: true },
      ];
      setOrderTabs(newTabs);
      setActiveTab(tabId);
      orderIndexedDB.saveOrderTabs(newTabs).catch((error) => {
        console.error("Lỗi lưu order tabs:", error);
        toast.error(t("cashier.saveFailed"));
      });
    } else {
      updateActiveTab(tabId);
    }

    updateStateForTab(
      tabId,
      () =>
        ({
          orderId: order.orderId || null,
          tariffId: order.tariffId,
          point: order.point,
          note: order.note,
          deliveryTime: order.deliveryTime,
          customer: order.customer,
          items: order.orderItems.map((item) => ({
            id: item.serviceId,
            name: item.serviceName,
            serviceId: item.serviceId,
            unitRelationId: item.unitRelationId,
            price: item.price,
            quantity: item.quantity,
            unitRelationName: item.unitRelationName,
            processingTime: item.processingTime,
            serviceName: item.serviceName,
            unitPrice: item.unitPrice,
          })),
          // NEW: seed equipments when editing
          orderEquipments: order.orderEquipments || [],
        }) as Partial<TabState>
    );
    updateTotal(tabId);
  };

  const handleProcessOrder = async (
    value: {
      discountValue: number;
      discountFixed: boolean;
      bookingDate?: Date;
      voucherCode?: string;
    },
    tabId: string
  ) => {
    if (!state[tabId]?.customer || state[tabId]?.items.length === 0) {
      toast.error(t("cashier.customerAndServiceRequired"));
      return;
    }
    if (!value.bookingDate) {
      toast.error(t("cashier.pickupTimeRequired"));
      return;
    }
    updateStateForTab(
      tabId,
      () => ({ isProcessing: true }) as Partial<TabState>
    );

    // Build payload with equipments
    const payload: any = {
      id: state[tabId].orderId || undefined,
      customer: state[tabId].customer!,
      orderItems: state[tabId].items,
      total: state[tabId].total,
      amount: state[tabId].amount,
      discountValue: value.discountValue,
      discountFixed: value.discountFixed,
      voucherCode: value.voucherCode || undefined,
      deliveryTime: value.bookingDate,
      branchId: branchActive?.branchId!,
      note: state[tabId].note || "",
      point: state[tabId].point,
      tariffId: state[tabId].tariffId || 0,
      // NEW: match API sample
      orderEquipments: (state[tabId].orderEquipments || []).map((e) => ({
        equipmentId: e.equipmentId,
        equipmentName: e.equipmentName,
      })),
    };

    try {
      let orderData;
      if (state[tabId].orderId) {
        await updateOrder.mutateAsync(payload);
      } else {
        const response = await createOrder.mutateAsync(payload);
        orderData = response.data.results;
        setCompletedOrder(orderData as Order);
      }

      const isOnlyOneTab = orderTabs.length === 1;
      if (isOnlyOneTab) {
        await orderIndexedDB.clearOrderState(tabId);
        const newTabId = "#1";
        tabIdCounter = 2;
        const defaultTab = { id: newTabId, isActive: true };
        const defaultState = {
          [newTabId]: initializeTabState(newTabId),
        } as any;
        await orderIndexedDB.saveOrderTabs([defaultTab]);
        setOrderTabs([defaultTab]);
        setActiveTab(newTabId);
        setState(defaultState);
      } else {
        await removeOrderTab(tabId);
        if (orderTabs.length <= 1) {
          const newTabId = "#1";
          tabIdCounter = 2;
          const defaultTab = { id: newTabId, isActive: true };
          const defaultState = {
            [newTabId]: initializeTabState(newTabId),
          } as any;
          await orderIndexedDB.saveOrderTabs([defaultTab]);
          setOrderTabs([defaultTab]);
          setActiveTab(newTabId);
          setState(defaultState);
        }
      }

      setTimeout(async () => {
        if (!state[tabId].orderId) {
          if (pickupTicketRef.current) {
            try {
              await pickupTicketRef.current.print();
              toast.info(t("cashier.create.withoutError"));
            } catch (printError) {
              toast.error(t("cashier.create.notPrint"));
            }
          } else {
            toast.error(t("cashier.create.notFound"));
          }
        } else {
          toast.info(
            t("toast.update.success", {
              entity: t("common.orders").toLowerCase(),
            })
          );
        }
      }, 100);
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    } catch (error) {
      console.error("Lỗi xử lý đơn giặt:", error);
      toast.error(
        state[tabId].orderId
          ? t("toast.update.failed", { entity: t("common.order") })
          : t("toast.create.failed", { entity: t("common.order") })
      );
    } finally {
      updateStateForTab(
        tabId,
        () => ({ isProcessing: false }) as Partial<TabState>
      );
    }
  };

  const handlePrint = async (tabId: string) => {
    try {
      if (pickupTicketRef.current) await pickupTicketRef.current.print();
      else toast.error(t("cashier.appoimentSlipPrint.notFound"));
    } catch (error) {
      console.error("Lỗi in:", error);
      toast.error(t("cashier.appoimentSlipPrint.unablePrint"));
    }
  };

  const createCustomerHandle = async ({
    displayName,
    gender,
    phoneNumber,
  }: {
    displayName: string;
    phoneNumber: string;
    gender: Gender;
  }) => {
    try {
      const response = await createCustomer.mutateAsync({
        displayName,
        gender,
        phoneNumber,
      });
      if (response.data.results) {
        const customer: Customer = {
          id: response.data.results.id!,
          displayName: response.data.results.displayName!,
          phoneNumber: response.data.results.phoneNumber || "",
        };
        handleSelectCustomer(customer, activeTab);
      }
    } catch (error) {}
  };

  const handleSelectTariff = async (id: number) => {
    updateStateForTab(activeTab, () => ({ tariffId: id }) as Partial<TabState>);
  };

  return {
    customer: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.customer])
    ),
    items: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.items || []])
    ),
    isProcessing: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.isProcessing || false])
    ),
    completedOrder: completedOrder,
    pickupTicketRef,
    orderListRef,
    customerData: customerData?.users || [],
    total: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.total || 0])
    ),
    amount: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.amount || 0])
    ),
    discountValue: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.discountValue || 0])
    ),
    discountFixed: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.discountFixed])
    ),
    voucherCode: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.voucherCode || ""])
    ),
    note: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.note || ""])
    ),
    tariffId: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.tariffId || 0])
    ),
    point: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.point || 0])
    ),
    deliveryTime: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.deliveryTime || null])
    ),
    orderId: Object.fromEntries(
      orderTabs.map((tab) => [tab.id, state[tab.id]?.orderId || null])
    ),

    handleSelectTariff,
    tariffData,
    handleAddItem,
    handleRemoveItem,
    handleUpdateQuantity,
    handleUpdatePrice,
    handleSelectCustomer,
    handlePrint,
    handleProcessOrder,
    addNewOrderTab,
    removeOrderTab,
    activeTab,
    setActiveTab: updateActiveTab,
    orderTabs,
    handleApplyVoucher,
    createCustomerHandle,
    handleSetNote,
    handleUpdatePoints,
    handleUpdateOrder,
  };
};
