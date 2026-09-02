# PayOS setup

VietWash uses PayOS only for card/online payments. Cash completion remains a normal staff action. A card payment is completed only by a verified PayOS webhook; return-page query parameters are display hints and never change an order.

## Required configuration

Set these environment variables for the Ecommerce service:

```dotenv
PayOsSetting__IsEnabled=true
PayOsSetting__ClientId=<client-id-from-PayOS>
PayOsSetting__ApiKey=<api-key-from-PayOS>
PayOsSetting__ChecksumKey=<checksum-key-from-PayOS>
PayOsSetting__ReturnUrl=https://app.example.com/payment/payos-return
PayOsSetting__CancelUrl=https://app.example.com/payment/payos-return
PayOsSetting__WebhookUrl=https://api.example.com/Webhook/api/CompletedOrder
```

Do not commit real credentials. Use deployment secrets or an untracked local `.env` file. When `PayOsSetting__IsEnabled=true`, all three credentials and all three URLs are required; each URL must be an absolute HTTP or HTTPS URL. The Ecommerce service fails at startup with a setting-specific message if the configuration is incomplete. With `PayOsSetting__IsEnabled=false`, the service starts without credentials and payment-link requests return a controlled business error.

Production callback and webhook URLs must be publicly reachable over HTTPS. `ReturnUrl` and `CancelUrl` normally point to the same Next.js route shown above.

## One-time PayOS dashboard setup

1. Create or open the payment channel at [my.payos.vn](https://my.payos.vn/).
2. Copy the channel Client ID, API key, and checksum key into the deployment secret store.
3. Set the channel webhook URL to the exact `PayOsSetting__WebhookUrl` value.
4. Let PayOS send its signed sample payload. VietWash verifies that signature and acknowledges only the documented sample without looking up or updating a real order.
5. Restart the Ecommerce service after changing environment variables.

The application deliberately does not call `confirm-webhook` during startup. Dashboard registration is a one-time operator action and cannot unexpectedly overwrite another environment's webhook URL.

## Verification

Run the automated checks from the repository root:

```powershell
dotnet restore Micro.sln --configfile NuGet.Config
dotnet build Micro.sln -c Release --no-restore
dotnet test tests/UnitTest/EcommerceService.Tests/EcommerceService.Tests.csproj -c Release --no-build
dotnet test tests/UnitTest/FinanceService.Tests/FinanceService.Tests.csproj -c Release --no-build
```

Then run the frontend checks from `frontend`:

```powershell
npm run generate
npm run typecheck
npm run lint
npm run build:test
```

## Manual end-to-end test

1. Use an order in `Processed` status at a branch available to the signed-in staff account.
2. Choose Card. Confirm that the browser opens a PayOS checkout URL returned by `POST /Ecommerce/api/Orders/{id}/payment-link`.
3. Repeat the action before paying. Confirm that the existing link is reused and no second PayOS link is created.
4. Pay the exact integer VND `Order.Total` amount. The return page may show “Payment received. Confirming order…” while it polls the local order.
5. Confirm that the verified webhook changes the order to `Completed` with payment method `Card`, and that duplicate delivery creates no duplicate Fund, points, VoucherUsage, or e-invoice event.
6. Start with another `Processed` order, open its link, and cancel on PayOS. Confirm that the return page reports only the online-payment cancellation and the business order remains `Processed`; staff can then choose Cash or explicitly cancel the order with a reason.
7. Tamper with the webhook signature or amount in a mock request. Confirm the endpoint returns non-2xx and the order remains unchanged.

No real PayOS account is required for the automated tests; provider calls and signature verification are mock-based. A real account is needed only for the manual checkout test.
