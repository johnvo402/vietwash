"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Plus, Search, X } from "lucide-react";
import { useTranslations } from "next-intl";
import { useCashier } from "./hooks/use-cashier";
import { OrderSummary } from "./components/order-summary";
import { ServiceSectionView } from "./service-sections/service-section-view";
import { OrderPaymentSection } from "./components/order-payment";
import PickupTicket from "../orders/components/PickupTicket";
import { useState } from "react";
import { CustomerFormDialog } from "../customer/components/user-create/create-customer-dialog";
import { Input } from "@/components/ui/input";

export default function Cashier() {
  const [searchTermService, setSearchTermService] = useState("");

  const {
    customer,
    items,
    isProcessing,
    completedOrder,
    pickupTicketRef,
    customerData,
    total,
    amount,
    discountValue,
    discountFixed,
    voucherCode,
    note,
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
    setActiveTab,
    orderTabs,
    orderListRef,
    handleApplyVoucher,
    createCustomerHandle,
    handleSetNote,
    tariffData,
    handleSelectTariff,
    tariffId,
    point,
    handleUpdatePoints,
    deliveryTime,
    orderId,
  } = useCashier();
  const [openCreateCustomer, setOpenCreateCustomer] = useState<boolean>(false);
  const t = useTranslations();

  return (
    <div className="w-full px-6 py-12">
      <Tabs
        value={activeTab}
        onValueChange={setActiveTab}
        className="space-y-4"
      >
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          <div className="lg:col-span-3 space-y-6">
            <Card>
              <CardHeader className="pb-3">
                <CardTitle>
                  <div className="flex justify-between items-center">
                    {t("common.service").replace(/^./, (c) => c.toUpperCase())}
                    <div className="relative">
                      <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-border h-4 w-4" />
                      <Input
                        type="text"
                        placeholder={t(
                          "equipment.equipmentList.searchPlaceholder"
                        )}
                        value={searchTermService}
                        onChange={(e) => setSearchTermService(e.target.value)}
                        className="pl-9 h-9 text-sm rounded-md"
                      />
                    </div>
                  </div>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <ServiceSectionView
                  onAddItem={(item) => handleAddItem(item, activeTab)}
                  tariffId={tariffId[activeTab]}
                  search={searchTermService}
                />
              </CardContent>
            </Card>

            <TabsList className="flex justify-start items-center overflow-auto h-12 scrollbar-hide">
              {orderTabs.map((tab) => (
                <div key={tab.id} className="flex items-center">
                  <TabsTrigger
                    value={tab.id}
                    className="px-4 py-2 text-sm font-medium data-[state=active]:bg-primary data-[state=active]:text-background"
                  >
                    {t("cashier.order")} {tab.id}
                    <span
                      onClick={() =>
                        orderTabs.length > 1 ? removeOrderTab(tab.id) : {}
                      }
                      className={`ml-1 px-2 ${orderTabs.length > 1 ? "cursor-pointer" : "cursor-not-allowed"}`}
                      title={t("cashier.removeTab")}
                    >
                      <X className="h-4 w-4" />
                    </span>
                  </TabsTrigger>
                </div>
              ))}
              <Button
                variant="outline"
                size="sm"
                onClick={addNewOrderTab}
                className="ml-2"
              >
                <Plus className="h-4 w-4" />
              </Button>
            </TabsList>

            {orderTabs.map((tab) => (
              <TabsContent key={tab.id} value={tab.id}>
                <Card>
                  <CardHeader className="pb-3">
                    <CardTitle>
                      {t("cashier.order").replace(/^./, (c) => c.toUpperCase())}
                    </CardTitle>
                  </CardHeader>
                  <CardContent
                    className="max-h-[40vh] overflow-auto"
                    ref={(el) => {
                      orderListRef.current[tab.id] = el;
                    }}
                  >
                    <OrderSummary
                      items={items[tab.id] || []}
                      onRemoveItem={(itemId) =>
                        handleRemoveItem(itemId, tab.id)
                      }
                      onUpdateQuantity={(itemId, quantity) =>
                        handleUpdateQuantity(itemId, quantity, tab.id)
                      }
                      onUpdatePrice={(itemId, price) =>
                        handleUpdatePrice(itemId, price, tab.id)
                      }
                    />
                  </CardContent>
                </Card>
              </TabsContent>
            ))}
          </div>

          <div className="space-y-6">
            <OrderPaymentSection
              disable={
                !customer[activeTab] || (items[activeTab] || []).length === 0
              }
              isProcessing={isProcessing[activeTab] || false}
              total={total[activeTab] || 0}
              amount={amount[activeTab] || 0}
              discountValue={discountValue[activeTab] || 0}
              discountFixed={discountFixed[activeTab]}
              voucherCode={voucherCode[activeTab] || ""}
              handleProcessOrder={(value) =>
                handleProcessOrder(value, activeTab)
              }
              handlePrint={() => handlePrint(activeTab)}
              printDisabled={!completedOrder}
              handleApplyVoucher={handleApplyVoucher}
              activeTab={activeTab}
              voucherDisabled={!customer[activeTab]}
              customers={customerData}
              onSelect={(customer) => handleSelectCustomer(customer, activeTab)}
              customerInit={customer[activeTab] || null}
              openCreate={() => setOpenCreateCustomer(true)}
              onSetNote={(note) => handleSetNote(note, activeTab)}
              note={note[activeTab]}
              tariffId={tariffId[activeTab]}
              onSetTariff={(id) => handleSelectTariff(id!)}
              tariffs={tariffData?.prices ?? []}
              point={point[activeTab] || 0}
              handleUpdatePoints={handleUpdatePoints}
              deliveryTime={deliveryTime[activeTab] || null}
              isEdit={!!orderId[activeTab]} // Sử dụng orderId từ useCashier
            />
            {completedOrder && (
              <PickupTicket ref={pickupTicketRef} order={completedOrder} />
            )}
          </div>
        </div>
      </Tabs>
      {openCreateCustomer && (
        <CustomerFormDialog
          isOpen={openCreateCustomer}
          onClose={() => setOpenCreateCustomer(false)}
          onSubmit={(data) =>
            createCustomerHandle({
              displayName: data.displayName,
              gender: data.gender,
              phoneNumber: data.phoneNumber,
            })
          }
        />
      )}
    </div>
  );
}
