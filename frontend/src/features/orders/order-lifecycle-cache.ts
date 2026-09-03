import type { QueryClient } from "@tanstack/react-query";

export const invalidateOrderEquipment = (client: QueryClient) =>
  Promise.all([
    client.invalidateQueries({ queryKey: ["form-equipments"] }),
    client.invalidateQueries({ queryKey: ["equipments"] }),
  ]);

export const invalidateOrderLifecycle = (client: QueryClient) =>
  Promise.all([
    client.invalidateQueries({ queryKey: ["orders"] }),
    client.invalidateQueries({ queryKey: ["order"] }),
    invalidateOrderEquipment(client),
  ]);
