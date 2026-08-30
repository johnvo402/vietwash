import { apiClient } from "@/api/client";
import { InventoryDocumentDetailResponse } from "@/api/generated";
import { useState, useRef, useEffect } from "react";
import { useTranslations } from "next-intl";
import { ChevronDown, ChevronUp, Loader2 } from "lucide-react";
import { usePushRouter } from "@/utils/router-utli";
import { Button } from "@/components/ui/button";
import { createPopper } from "@popperjs/core";

interface Option {
  label: string;
  value: number;
}

const DownloadButton: React.FC<{
  supply: InventoryDocumentDetailResponse;
  invId: number;
}> = ({ supply, invId }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);
  const t = useTranslations();
  const { pushRouter } = usePushRouter();
  const buttonRef = useRef<HTMLButtonElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  function getSuppliers(): Option[] {
    const supplierMap = new Map<number, string>();

    supply.equipmentSupplyings?.forEach((item) => {
      supplierMap.set(item.supplierId!, item.supplierName!);
    });

    supply.productSupplyings?.forEach((item) => {
      supplierMap.set(item.supplierId!, item.supplierName!);
    });

    return Array.from(supplierMap).map(([value, label]) => ({
      label,
      value,
    }));
  }

  const handleDownload = async (supplierId: number) => {
    setIsDownloading(true);
    try {
      const response =
        await apiClient.ecommerceApiInventoriesGetReceiptIdSupplierIdGet(
          invId,
          supplierId
        );

      const url = response.data.results?.url;
      if (url) {
        pushRouter({
          router: url,
          redirect: "blank",
        });
      } else {
        console.warn("No receipt URL found");
      }
    } catch (error) {
      console.error("Download error", error);
    } finally {
      setIsDownloading(false);
      setIsOpen(false);
    }
  };

  // Initialize Popper.js when dropdown opens
  useEffect(() => {
    if (isOpen && buttonRef.current && dropdownRef.current) {
      const popperInstance = createPopper(
        buttonRef.current,
        dropdownRef.current,
        {
          placement: "bottom-start", // Open downward, aligned to the left
          modifiers: [
            {
              name: "preventOverflow",
              options: {
                boundariesElement: "viewport", // Ensure dropdown stays within viewport
              },
            },
            {
              name: "flip",
              options: {
                fallbackPlacements: ["top-start", "right-start", "left-start"], // Flip to top or sides if no space
              },
            },
          ],
        }
      );

      return () => {
        popperInstance.destroy(); // Cleanup on unmount or when isOpen changes
      };
    }
  }, [isOpen]);

  const suppliers = getSuppliers();

  return (
    suppliers && (
      <div className="relative">
        <Button
          ref={buttonRef}
          onClick={() => setIsOpen(!isOpen)}
          disabled={isDownloading}
          className="bg-background border text-primary px-4 py-2 rounded-md hover:bg-background transition-colors"
        >
          {isDownloading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              {t("common.loading")}
            </>
          ) : (
            <>
              {t("inventory.detail.export_file")}
              {isOpen ? (
                <ChevronUp className="h-4 w-4" />
              ) : (
                <ChevronDown className="h-4 w-4" />
              )}
            </>
          )}
        </Button>
        {isOpen && (
          <div
            ref={dropdownRef}
            className="w-64 max-h-60 overflow-y-auto bg-popover border rounded-md shadow-lg z-10"
          >
            <ul className="py-1">
              <li className="px-4 py-2 text-foreground font-semibold bg-muted cursor-default select-none">
                {t("AddressSelector.select", {
                  label: t("common.supplier").toLowerCase(),
                })}
              </li>
              {suppliers.map((supplier) => (
                <li
                  key={supplier.value}
                  onClick={() => handleDownload(supplier.value)}
                  className="px-4 py-2 text-foreground hover:bg-accent hover:text-accent-foreground cursor-pointer transition-colors"
                >
                  {supplier.label}
                </li>
              ))}
              {suppliers.length === 0 && (
                <li className="px-4 py-2 text-muted-foreground">
                  {t("common.noData")}
                </li>
              )}
            </ul>
          </div>
        )}
      </div>
    )
  );
};

export default DownloadButton;
