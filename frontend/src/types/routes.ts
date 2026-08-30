const routes = {
  home: "/",
  about: "/about",
  contact: "/contact",
  blog: {
    index: "/blog",
    detail: (slug: string) => `/blog/${slug}`, // Dynamic route
  },
  products: {
    index: "/products",
    detail: (id: string) => `/products/${id}`, // Dynamic route
  },
};

export default routes;
