// pages/chinh-sach-bao-mat.tsx
import { useTranslations } from "next-intl";
import Head from "next/head";

const Page = () => {
  const t = useTranslations("PrivacyPolicyPage");

  return (
    <>
      <Head>
        <title>{t("title")}</title>
      </Head>
      <div className="container mx-auto p-4">
        <h1 className="text-3xl font-bold text-center">{t("title")}</h1>
        <p className="mt-4 text-lg">{t("introduction")}</p>

        <div className="mt-6">
          <h2 className="text-2xl font-semibold">{t("section1.title")}</h2>
          <p>{t("section1.content")}</p>
        </div>
        <div className="mt-6">
          <h2 className="text-2xl font-semibold">{t("section2.title")}</h2>
          <p>{t("section2.content")}</p>
        </div>
      </div>
    </>
  );
};

export default Page;
