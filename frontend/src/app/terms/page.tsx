// pages/dich-vu.tsx
import { useTranslations } from "next-intl";
import Head from "next/head";

const Page = () => {
  const t = useTranslations("ServicePage");

  return (
    <>
      <Head>
        <title>{t("title")}</title>
      </Head>
      <div className="container mx-auto p-4">
        <h1 className="text-3xl font-bold text-center">{t("title")}</h1>
        <p className="mt-4 text-lg">{t("description")}</p>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
          <div className="bg-card p-6 rounded-lg shadow-md">
            <h2 className="text-xl font-semibold">{t("service1.title")}</h2>
            <p>{t("service1.description")}</p>
          </div>
          <div className="bg-card p-6 rounded-lg shadow-md">
            <h2 className="text-xl font-semibold">{t("service2.title")}</h2>
            <p>{t("service2.description")}</p>
          </div>
        </div>
      </div>
    </>
  );
};

export default Page;
