export const useRoute = () => {
  function push({
    path,
    params,
  }: {
    path: string;
    params: Record<string, string>;
  }) {
    let fullPath = path;

    Object.keys(params).forEach((key) => {
      fullPath = fullPath.replace(`[${key}]`, params[key]);
    });

    return fullPath;
  }
  return {
    push,
  };
};
