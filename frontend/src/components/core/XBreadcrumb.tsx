import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { useTranslations, useMessages } from "next-intl";
import { usePathname } from "next/navigation";
import { Fragment } from "react";
import "./styles.scss";

function routePatternToRegex(pattern: string): RegExp {
  const regexStr = pattern.replace(/\[.*?\]/g, "[^/]+");
  return new RegExp(`^${regexStr}$`);
}

export function XBreadcrumb() {
  const pathname = usePathname();
  const t = useTranslations("route");
  const messages = useMessages();
  const routeKeys = Object.keys(messages["route"] ?? {});

  const pathParts = pathname.split("/").filter(Boolean);
  const pathVariants = pathParts.map(
    (_, i) => "/" + pathParts.slice(0, i + 1).join("/")
  );

  const matchedBreadcrumbs = pathVariants
    .map((segment) => {
      const matched = routeKeys.find((pattern) =>
        routePatternToRegex(pattern).test(segment)
      );
      return matched ? { name: t(matched), href: segment } : null;
    })
    .filter(Boolean) as { name: string; href: string }[];

  return (
    <Breadcrumb>
      <BreadcrumbList>
        {matchedBreadcrumbs.map((item, index) => (
          <Fragment key={item.href}>
            {index > 0 && <BreadcrumbSeparator />}
            <BreadcrumbItem>
              <BreadcrumbLink href={index > 0 ? item.href : "#"}>
                <span className="font-bold breadbrumb-span">{item.name}</span>
              </BreadcrumbLink>
            </BreadcrumbItem>
          </Fragment>
        ))}
      </BreadcrumbList>
    </Breadcrumb>
  );
}
