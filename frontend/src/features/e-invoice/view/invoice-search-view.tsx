"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import InvoiceSearchForm from "../components/invoice-search-form";

export default function InvoiceSearchView() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center">
      <Card className="w-screen">
        <CardHeader>
          <CardTitle className="text-2xl font-bold text-center">
            VietWash Invoice Search
          </CardTitle>
        </CardHeader>
        <CardContent>
          <InvoiceSearchForm />
        </CardContent>
      </Card>
    </div>
  );
}
