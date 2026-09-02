export class CustomerSyncPendingError extends Error {}

// A timeout retains the Auth ID. Retrying must only repeat lookup, never CreateCustomer.
export async function waitForCustomer<T>(
  id: number,
  lookup: (id: number) => Promise<T>,
  options: {
    attempts?: number;
    delayMs?: number;
    sleep?: (ms: number) => Promise<void>;
  } = {},
): Promise<T> {
  const {
    attempts = 6,
    delayMs = 700,
    sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms)),
  } = options;
  for (let attempt = 0; attempt < attempts; attempt++) {
    try {
      return await lookup(id);
    } catch (error) {
      // Only a not-yet-propagated customer is retried. Auth/network/server errors are not 404s.
      if (
        (error as { response?: { status?: number } }).response?.status !== 404
      )
        throw error;
      if (attempt + 1 < attempts) await sleep(delayMs);
    }
  }
  throw new CustomerSyncPendingError(
    "Customer was created successfully but is still synchronizing. Please retry in a moment.",
  );
}

export async function synchronizeCustomer<T>(
  id: number,
  lookup: (id: number) => Promise<T>,
  ready: (customer: T) => Promise<void>,
  options?: Parameters<typeof waitForCustomer>[2],
) {
  const customer = await waitForCustomer(id, lookup, options);
  await ready(customer);
  return customer;
}
