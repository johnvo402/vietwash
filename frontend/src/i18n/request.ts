import { getRequestConfig } from "next-intl/server";
import { cookies } from "next/headers";

export default getRequestConfig(async () => {
  // Provide a static locale, fetch a user setting,
  // read from `cookies()`, `headers()`, etc.

  const requested = (await cookies()).get("NEXT_LOCALE")?.value;
  const locale = requested === "en" ? "en" : "vi";

  return {
    locale,
    messages: (await import(`../../messages/${locale}.ts`)).default,
  };
});
