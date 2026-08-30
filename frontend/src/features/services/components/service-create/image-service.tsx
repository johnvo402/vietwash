import { useCallback, useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from "@/components/ui/form";
import { Button } from "@/components/ui/button";
import { Upload } from "lucide-react";
import Image from "next/image";

interface ImageUploadFieldProps {
  form: any; // UseFormReturn<FormValues>
  image: (data?: File) => Promise<void>;
  initialImage?: string | null;
}

export function ImageUploadField({
  form,
  image,
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
        image(file);
        const reader = new FileReader();
        reader.onload = () => {
          const result = reader.result as string;
          setImagePreview(result);
          form.setValue("image", result, { shouldValidate: true });
        };
        reader.readAsDataURL(file);
      }
    },
    [image, form]
  );

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
              Entity: t("common.service").replace(/^./, (c) => c.toUpperCase()),
            })}
          </FormLabel>
          <FormControl>
            <div className="flex flex-col items-center justify-center border-2 border-dashed border-gray-300 rounded-lg p-4 max-w-[300px] h-[150px]">
              {imagePreview ? (
                <div className="relative w-full h-full">
                  <Image
                    src={imagePreview}
                    alt={t("image.alt", { entity: t("common.service") })}
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
                      image(undefined);
                      setImagePreview(null);
                      form.setValue("image", "", { shouldValidate: true });
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
                    accept="image/*"
                    onChange={handleImageChange}
                  />
                </label>
              )}
            </div>
          </FormControl>
          <FormDescription>
            {t("image.description", { entity: t("common.service") })}
          </FormDescription>
          <FormMessage />
        </FormItem>
      )}
    />
  );
}
