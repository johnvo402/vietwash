import qs from "qs";

type Operator =
  | "$eq"
  | "$eqi"
  | "$ne"
  | "$nei"
  | "$in"
  | "$notin"
  | "$lt"
  | "$lte"
  | "$gt"
  | "$gte"
  | "$between"
  | "$contains"
  | "$containsi"
  | "$notcontains"
  | "$notcontainsi"
  | "$startswith"
  | "$endswith";

type FilterCondition = {
  [field: string]: {
    [op in Operator]?: any;
  };
};

type AndOrCondition = {
  $and?: (FilterCondition | AndOrCondition)[];
  $or?: (FilterCondition | AndOrCondition)[];
};

type FilterInput = FilterCondition | AndOrCondition;

/**
 * Build query string for backend filter
 * @param filterObject Complex filter object
 * @returns Query string
 */
export function buildFilterQuery(filterObject: FilterInput): string {
  return qs.stringify({ filter: filterObject }, { encode: false });
}
export const useQueryFilter = () => {
  function flattenQueryObject(obj: any): { [key: string]: string } {
    // First stringify the object with the qs library to get the URL format
    const filter = {
      filter: obj,
    };
    const queryString = qs.stringify(filter, { encode: false });

    // Then parse it back, but as a flat object
    const result: { [key: string]: string } = {};

    // Split by & to get each key=value pair
    const pairs = queryString.split("&");

    for (const pair of pairs) {
      const [key, value] = pair.split("=");
      // Decode the URI components to get the original characters
      result[key] = value || "";
    }

    return result;
  }

  function prepareApiParams<T extends readonly string[]>(
    paramKeys: T,
    params: Partial<Record<T[number], any>>,
    defaults: Partial<Record<T[number], any>> = {}
  ): any[] {
    return paramKeys.map(
      (key: T[number]) => params[key] ?? defaults[key] ?? undefined
    );
  }

  return {
    flattenQueryObject,
    prepareApiParams,
  };
};
