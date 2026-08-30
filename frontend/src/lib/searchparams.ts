import {
  createParser,
  parseAsInteger,
  parseAsString,
  createSearchParamsCache,
  createSerializer,
} from "nuqs/server";

export const searchParams = {
  page: parseAsInteger.withDefault(1),
  pageSize: parseAsInteger.withDefault(10),
  search: parseAsString,
};

export const searchParamsCache = createSearchParamsCache(searchParams);
export const serialize = createSerializer(searchParams);
