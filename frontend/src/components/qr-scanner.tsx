"use client";

import { useState, useRef, useEffect } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Camera, RotateCcw, Play, Square } from "lucide-react";
import { BrowserMultiFormatReader, NotFoundException } from "@zxing/library";
import { useTranslations } from "next-intl";
import { useIsMobile } from "@/hooks/use-mobile";

interface QRScannerProps {
  onScanSuccess: (value: string) => void;
  onScanError?: (error: string) => void;
  className?: string;
  autoStart?: boolean;
  onStop?: () => void;
}

export function QRScanner({
  onScanSuccess,
  onScanError,
  className = "",
  autoStart = false,
  onStop,
}: QRScannerProps) {
  const [isScanning, setIsScanning] = useState(autoStart);
  const [error, setError] = useState<string>("");
  const [devices, setDevices] = useState<MediaDeviceInfo[]>([]);
  const [selectedDeviceId, setSelectedDeviceId] = useState<string>("");
  const [isInitialized, setIsInitialized] = useState(false);

  const t = useTranslations("qr_code");
  const videoRef = useRef<HTMLVideoElement>(null);
  const codeReader = useRef<BrowserMultiFormatReader | null>(null);
  const isMobile = useIsMobile();

  useEffect(() => {
    codeReader.current = new BrowserMultiFormatReader();

    if (autoStart) {
      initializeCamera();
    }

    return () => {
      if (codeReader.current) {
        codeReader.current.reset();
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [autoStart]);

  const initializeCamera = async () => {
    if (
      !navigator.mediaDevices?.getUserMedia ||
      !navigator.mediaDevices?.enumerateDevices
    ) {
      const msg = t("browserNotSupported");
      setError(msg);
      onScanError?.(msg);
      return;
    }

    try {
      setError("");

      // Request camera permission
      const stream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: "environment",
          width: { ideal: isMobile ? 640 : 1280 },
          height: { ideal: isMobile ? 480 : 720 },
        },
      });
      stream.getTracks().forEach((t) => t.stop());

      // Enumerate video input devices
      const allDevices = await navigator.mediaDevices.enumerateDevices();
      const videoDevices = allDevices.filter(
        (d) => d.kind === "videoinput"
      ) as MediaDeviceInfo[];

      setDevices(videoDevices);
      if (videoDevices.length > 0) {
        setSelectedDeviceId(videoDevices[0].deviceId);
        setIsInitialized(true);

        if (autoStart) {
          setTimeout(() => startScan(), 500);
        }
      } else {
        const msg = t("noCameraFound");
        setError(msg);
        onScanError?.(msg);
      }
    } catch (err) {
      const msg =
        err instanceof DOMException && err.name === "NotAllowedError"
          ? t("cameraPermissionDenied")
          : t("cannotAccessCamera");
      setError(msg);
      onScanError?.(msg);
    }
  };

  const startScan = async () => {
    if (!codeReader.current || !videoRef.current) return;

    try {
      setIsScanning(true);
      setError("");

      await codeReader.current.decodeFromVideoDevice(
        selectedDeviceId || null,
        videoRef.current,
        (result, error) => {
          if (result) {
            const scannedValue = result.getText();
            onScanSuccess(scannedValue);
            stopScan();
          }
          if (error && !(error instanceof NotFoundException)) {
            console.error("Scan error:", error);
          }
        }
      );
    } catch (err) {
      const msg = t("cannotStartCamera");
      setError(msg);
      onScanError?.(msg);
      setIsScanning(false);
    }
  };

  const stopScan = () => {
    if (codeReader.current) {
      codeReader.current.reset();
    }
    setIsScanning(false);
    onStop?.();
  };

  const switchCamera = () => {
    if (devices.length > 1) {
      const currentIndex = devices.findIndex(
        (device) => device.deviceId === selectedDeviceId
      );
      const nextIndex = (currentIndex + 1) % devices.length;
      setSelectedDeviceId(devices[nextIndex].deviceId);

      if (isScanning) {
        stopScan();
        setTimeout(() => startScan(), 100);
      }
    }
  };

  const handleStart = () => {
    if (!isInitialized) {
      initializeCamera();
    } else {
      startScan();
    }
  };

  if (!navigator.mediaDevices?.getUserMedia) {
    return (
      <Card className={className}>
        <CardContent className="p-4">
          <div className="text-center text-red-600 text-sm">
            {t("browserNotSupported")}
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card
      className={`w-full ${isMobile ? "max-w-full" : "max-w-md"} mx-auto ${className}`}
    >
      <CardHeader className={isMobile ? "p-4" : "p-6"}>
        <CardTitle className="flex items-center gap-2 text-lg">
          <Camera className="w-5 h-5" />
          {t("qrCodeScanner")}
        </CardTitle>
        <CardDescription className="text-sm">
          {t("scanInstruction")}
        </CardDescription>
      </CardHeader>
      <CardContent className={`space-y-4 ${isMobile ? "p-4" : "p-6"}`}>
        <div
          className={`relative ${isMobile ? "aspect-[3/4]" : "aspect-[4/3]"} bg-black rounded-lg overflow-hidden`}
        >
          <video
            ref={videoRef}
            className="w-full h-full object-cover"
            playsInline
            muted
          />
          <div className="absolute inset-0 border-2 border-white/20 rounded-lg">
            <div className="absolute inset-4 border-2 border-white rounded-lg">
              <div className="absolute top-0 left-0 w-6 h-6 border-t-2 border-l-2 border-primary"></div>
              <div className="absolute top-0 right-0 w-6 h-6 border-t-2 border-r-2 border-primary"></div>
              <div className="absolute bottom-0 left-0 w-6 h-6 border-b-2 border-l-2 border-primary"></div>
              <div className="absolute bottom-0 right-0 w-6 h-6 border-b-2 border-r-2 border-primary"></div>
            </div>
          </div>
          <div className="absolute top-2 right-2 flex gap-2">
            {devices.length > 1 && (
              <Button
                size="sm"
                variant="secondary"
                onClick={switchCamera}
                className={`h-8 w-8 p-0 ${isMobile ? "h-7 w-7" : ""}`}
              >
                <RotateCcw className="w-4 h-4" />
              </Button>
            )}
          </div>
          {!isInitialized && !isScanning && (
            <div className="absolute inset-0 bg-black/50 flex items-center justify-center">
              <div className="text-white text-center">
                <Camera
                  className={`mx-auto mb-2 opacity-50 ${isMobile ? "w-10 h-10" : "w-12 h-12"}`}
                />
                <p className={`text-sm ${isMobile ? "text-xs" : ""}`}>
                  {t("pressToStartCamera")}
                </p>
              </div>
            </div>
          )}
        </div>
        {error && (
          <div
            className={`p-3 text-sm text-red-600 bg-red-50 rounded-lg border border-red-200 ${isMobile ? "text-xs" : ""}`}
          >
            {error}
          </div>
        )}
        <div className="flex gap-2">
          {!isScanning ? (
            <Button onClick={handleStart} className="flex-1" disabled={!!error}>
              <Play className={`w-4 h-4 mr-2 ${isMobile ? "w-3 h-3" : ""}`} />
              {!isInitialized ? t("startCamera") : t("startScanning")}
            </Button>
          ) : (
            <Button
              onClick={stopScan}
              variant="outline"
              className={`flex-1 bg-transparent ${isMobile ? "text-sm" : ""}`}
            >
              <Square className={`w-4 h-4 mr-2 ${isMobile ? "w-3 h-3" : ""}`} />
              {t("stopScanning")}
            </Button>
          )}
        </div>
        {isScanning && (
          <div className="text-center">
            <div
              className={`inline-flex items-center gap-2 text-sm text-green-600 ${isMobile ? "text-xs" : ""}`}
            >
              <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
              {t("scanning")}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
