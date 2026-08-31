"use client";
import { apiClient } from "@/api/client";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { isValidCredentials, useAuth } from "@/hooks/use-auth";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useTransition } from "react";
import { useForm } from "react-hook-form";
import * as z from "zod";
import PasswordInput from "./PasswordInput";
import { useTranslations } from "next-intl";
import { Checkbox } from "@/components/ui/checkbox"; // Import Checkbox component
import { toast } from "react-toastify";

// Updated form schema to include rememberMe
const formSchema = z.object({
  email: z.string().nonempty({ message: "Please enter email" }),
  password: z.string().nonempty({ message: "Please enter password" }),
  rememberMe: z.boolean().default(false), // Add rememberMe field
});

type UserFormValue = z.infer<typeof formSchema>;

export default function LoginForm() {
  const [loading, startTransition] = useTransition();
  const { login } = useAuth();

  // Initialize form with default values
  const form = useForm<UserFormValue>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      email: "",
      password: "",
      rememberMe: false, // Default value for rememberMe
    },
  });

  const mutationLogin = useMutation({
    mutationFn: (data: UserFormValue) =>
      apiClient.authApiAccountsLoginPost(data),
    onSuccess: (data) => {
      const credentials = {
        accessTokenExpiredIn: data.data.results?.accessTokenExpiredIn,
        refresh: data.data.results?.refresh,
        token: data.data.results?.token,
      };

      if (!isValidCredentials(credentials)) {
        toast.error("Login response did not contain valid credentials");
        return;
      }

      login(credentials);
      toast("Login success");
    },
    onError: () => {
      toast.error("Login failed");
    },
  });

  const onSubmit = (data: UserFormValue) => {
    startTransition(() => {
      mutationLogin.mutateAsync({
        email: data.email,
        password: data.password,
        rememberMe: data.rememberMe, // Include rememberMe in the mutation if needed
      });
    });
  };

  const t = useTranslations();

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="w-full space-y-2">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>{t("user.email.title")}</FormLabel>
              <FormControl>
                <Input
                  type="email" // Use type="email" for better browser support
                  placeholder={t("user.placeholder", {
                    entity: t("user.email.title"),
                  })}
                  disabled={loading || mutationLogin.isPending}
                  autoComplete="email" // Enable autocomplete
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>{t("user.password.title")}</FormLabel>
              <FormControl>
                <PasswordInput
                  placeholder={t("user.placeholder", {
                    entity: t("user.password.title"),
                  })}
                  disabled={loading || mutationLogin.isPending}
                  autoComplete="current-password" // Enable autocomplete
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="rememberMe"
          render={({ field }) => (
            <FormItem>
              <label className="flex items-center space-x-2 cursor-pointer">
                <span className="text-sm">
                  {t("user.rememberMe.title") || "Remember me"}
                </span>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                  disabled={loading || mutationLogin.isPending}
                  id="rememberMe"
                />
              </label>
            </FormItem>
          )}
        />
        <Button
          disabled={loading || mutationLogin.isPending}
          className="ml-auto w-full"
          type="submit"
        >
          {mutationLogin.isPending ? "Logging in..." : "Login"}
        </Button>
      </form>
    </Form>
  );
}
