import { useCallback, useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Button } from "@/components/ui/button";
import { Upload } from "lucide-react";
import { UseFormReturn } from "react-hook-form";
import { FormValues } from "./create-material-dialog";
import Image from "next/image";

const MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB
const ALLOWED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/gif"];

interface ImageUploadFieldProps {
  form: UseFormReturn<FormValues>;
  initialImage?: string | null;
}

export function ImageUploadField({
  form,
  initialImage,
}: ImageUploadFieldProps) {
  const t = useTranslations();
  const [imagePreview, setImagePreview] = useState<string | null>(
    initialImage || null
  );

  const handleImageChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      if (e.target.files && e.target.files[0]) {
        const file = e.target.files[0];

        // Validate file size
        if (file.size > MAX_FILE_SIZE) {
          form.setError("image", {
            type: "manual",
            message: t("product.imageSizeValidation", { max: "50MB" }),
          });
          return;
        }

        // Validate file type
        if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
          form.setError("image", {
            type: "manual",
            message: t("product.imageTypeValidation", {
              allowed: "JPEG, PNG, GIF",
            }),
          });
          return;
        }

        // Set image file in form
        form.setValue("image", file, { shouldValidate: true });

        // Generate preview URL
        const previewUrl = URL.createObjectURL(file);
        setImagePreview(previewUrl);

        // Clean up preview URL when component unmounts or new file is selected
        return () => URL.revokeObjectURL(previewUrl);
      }
    },
    [form, t]
  );

  // Clean up preview URL on unmount
  useEffect(() => {
    return () => {
      if (imagePreview && (!initialImage || typeof initialImage !== "string")) {
        URL.revokeObjectURL(imagePreview);
      }
    };
  }, [imagePreview, initialImage]);

  return (
    <FormField
      control={form.control}
      name="image"
      render={({ field }) => (
        <FormItem>
          <FormLabel>
            {t("image.name", {
              Entity: t("common.product").replace(/^./, (c) => c.toUpperCase()),
            })}
          </FormLabel>
          <FormControl>
            <div className="flex flex-col items-center justify-center border-2 border-dashed border-gray-300 rounded-lg p-4 max-w-[300px] h-[150px]">
              {imagePreview ? (
                <div className="relative w-full h-full">
                  <Image
                    src={imagePreview}
                    alt={t("image.alt", { entity: t("common.product") })}
                    className="object-contain"
                    fill
                    onError={() =>
                      console.error("Failed to load image:", imagePreview)
                    }
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="absolute bottom-2 right-2"
                    onClick={() => {
                      setImagePreview(null);
                      form.setValue("image", undefined, {
                        shouldValidate: true,
                      });
                    }}
                  >
                    {t("common.delete")}
                  </Button>
                </div>
              ) : (
                <label
                  htmlFor="image-upload"
                  className="flex flex-col items-center justify-center cursor-pointer w-full h-full"
                >
                  <Upload className="h-8 w-8 text-gray-400 mb-2" />
                  <span className="text-sm text-gray-500">
                    {t("common.upload")}
                  </span>
                  <input
                    id="image-upload"
                    type="file"
                    className="hidden"
                    accept={ALLOWED_IMAGE_TYPES.join(",")}
                    onChange={handleImageChange}
                  />
                </label>
              )}
            </div>
          </FormControl>
          <FormDescription>
            {t("image.description", { entity: t("common.product") })}
          </FormDescription>
          <FormMessage />
        </FormItem>
      )}
    />
  );
}
