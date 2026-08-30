/* eslint-disable react-hooks/exhaustive-deps */
"use client";

import { useState, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Search } from "lucide-react";
import { useInvoiceSearch } from "../hooks/use-einvoice";

export default function InvoiceSearchForm() {
  const { invoiceCode, setInvoiceCode, searchResult, handleSearch } =
    useInvoiceSearch();

  const [blobUrl, setBlobUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  // Mỗi lần có searchResult mới -> fetch PDF & tạo blob URL
  useEffect(() => {
    const fetchPdf = async () => {
      if (!searchResult) {
        setBlobUrl(null);
        return;
      }

      setLoading(true);
      try {
        const res = await fetch(searchResult);
        if (!res.ok) throw new Error("Tải PDF thất bại");

        const blob = await res.blob();
        const url = URL.createObjectURL(blob);

        // Giải phóng URL cũ nếu có
        if (blobUrl) URL.revokeObjectURL(blobUrl);

        setBlobUrl(url);
      } catch (err) {
        console.error("Lỗi khi tải PDF:", err);
        alert("Không thể tải file PDF");
      } finally {
        setLoading(false);
      }
    };

    fetchPdf();
  }, [searchResult]);
  useEffect(() => {
    if (!invoiceCode) {
      setBlobUrl(null);
      return;
    }
  }, [invoiceCode]);
  return (
    <div className="w-full mx-auto bg-background shadow rounded">
      <div className="flex max-w-md mx-auto gap-2">
        <Input
          type="text"
          placeholder="Nhập mã hóa đơn"
          value={invoiceCode}
          onChange={(e) => setInvoiceCode(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          className="flex-1"
        />
        <Button onClick={handleSearch}>
          <Search className="w-4 h-4 mr-2" />
          Tìm kiếm
        </Button>
      </div>

      {loading && (
        <p className="mt-4 text-sm text-muted-foreground">
          Đang tải hóa đơn...
        </p>
      )}
      {blobUrl && (
        <iframe
          src={blobUrl}
          title="Hóa đơn điện tử"
          width="50%"
          height="800px"
          className="mt-4 border rounded-md mx-auto"
        />
      )}
    </div>
  );
}
