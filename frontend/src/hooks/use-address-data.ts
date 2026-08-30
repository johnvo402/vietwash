"use client";

import { useState, useEffect, useCallback } from "react";

interface AddressItem {
  id: string;
  full_name: string;
}

interface ApiResponse {
  error: number;
  data: AddressItem[];
}

export function useAddressApi() {
  const [provinces, setProvinces] = useState<AddressItem[]>([]);
  const [districts, setDistricts] = useState<AddressItem[]>([]);
  const [communes, setCommunes] = useState<AddressItem[]>([]);
  const [loading, setLoading] = useState({
    provinces: false,
    districts: false,
    communes: false,
  });

  // Load provinces on mount
  useEffect(() => {
    loadProvinces();
  }, []);

  const loadProvinces = async () => {
    setLoading((prev) => ({ ...prev, provinces: true }));
    try {
      const response = await fetch("https://esgoo.net/api-tinhthanh/1/0.htm");
      const data: ApiResponse = await response.json();
      if (data.error === 0) {
        setProvinces(data.data);
      }
    } catch (error) {
      console.error("Error loading provinces:", error);
    } finally {
      setLoading((prev) => ({ ...prev, provinces: false }));
    }
  };

  const loadDistricts = useCallback(async (provinceId: string) => {
    if (!provinceId || provinceId === "0") {
      setDistricts([]);
      setCommunes([]);
      return;
    }

    setLoading((prev) => ({ ...prev, districts: true }));
    try {
      const response = await fetch(
        `https://esgoo.net/api-tinhthanh/2/${provinceId}.htm`
      );
      const data: ApiResponse = await response.json();
      if (data.error === 0) {
        setDistricts(data.data);
        setCommunes([]); // Reset communes when province changes
      }
    } catch (error) {
      console.error("Error loading districts:", error);
    } finally {
      setLoading((prev) => ({ ...prev, districts: false }));
    }
  }, []);

  const loadCommunes = useCallback(async (districtId: string) => {
    if (!districtId || districtId === "0") {
      setCommunes([]);
      return;
    }

    setLoading((prev) => ({ ...prev, communes: true }));
    try {
      const response = await fetch(
        `https://esgoo.net/api-tinhthanh/3/${districtId}.htm`
      );
      const data: ApiResponse = await response.json();
      if (data.error === 0) {
        setCommunes(data.data);
      }
    } catch (error) {
      console.error("Error loading communes:", error);
    } finally {
      setLoading((prev) => ({ ...prev, communes: false }));
    }
  }, []);

  return {
    provinces,
    districts,
    communes,
    loading,
    loadDistricts,
    loadCommunes,
  };
}
