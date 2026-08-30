import Link from "next/link";
import LoginForm from "./LoginForm";
import { useTranslations } from "next-intl";
import { useTheme } from "next-themes";

export default function LoginView() {
  const t = useTranslations();
  const { theme } = useTheme();

  return (
    <div className="relative flex w-full h-screen flex-col items-center justify-center md:grid lg:max-w-none lg:grid-cols-2 lg:px-0">
      <div
        className={`relative hidden h-full flex-col p-10 text-white lg:flex ${theme === "dark" ? "bg-blue-900" : "bg-blue-700"}`}
        style={{
          backgroundImage: "url(/img/backround-login.jpeg)",
          backgroundSize: "cover",
          backgroundPosition: "center",
        }}
      >
        <div className="absolute inset-0 bg-primary opacity-50" />
        <div className="relative z-20 flex items-center text-lg font-medium">
          <img
            src={theme === "dark" ? "/logo-dark.png" : "/logo.png"}
            alt="VietWash Logo"
            className="mr-2 h-28 w-28 object-contain"
          />
        </div>
      </div>
      <div className="flex h-full items-center p-4 lg:p-8 bg-gray-50">
        <div className="mx-auto flex w-full flex-col justify-center space-y-6 sm:w-[350px]">
          <div className="flex flex-col space-y-2 text-center">
            <h1 className="text-2xl font-semibold tracking-tight">
              {t("user.login_title")}
            </h1>
          </div>
          <LoginForm />
          <p className="px-8 text-center text-sm text-gray-500">
            {t("user.click_continue")}{" "}
            <Link
              href="/terms"
              className="underline underline-offset-4 hover:text-blue-700"
            >
              {t("user.terms_of_service")}
            </Link>{" "}
            {t("user.and")}{" "}
            <Link
              href="/privacy"
              className="underline underline-offset-4 hover:text-blue-700"
            >
              {t("user.privacy_policy")}
            </Link>
            .
          </p>
        </div>
      </div>
    </div>
  );
}
