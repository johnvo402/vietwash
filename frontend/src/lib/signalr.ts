import * as signalR from "@microsoft/signalr";

const connections = new Map<string, signalR.HubConnection>();

const BASE_URL = process.env.NEXT_PUBLIC_API_URL?.replace(/\/+$/, "") ?? "";
const publicClientId = process.env.NEXT_PUBLIC_CLIENT_ID;
const platform = process.env.NEXT_PUBLIC_PLATFORM;

const defaultHeaders: Record<string, string> = {};

if (publicClientId) defaultHeaders["X-Api-Key"] = publicClientId;
if (platform) defaultHeaders.Platform = platform;

interface ConnectionOptions<TMessage> {
  accessToken: string;
  onReceiveMessage: (message: TMessage) => void;
}

export const startSignalRConnection = async <TMessage>({
  accessToken,
  onReceiveMessage,
}: ConnectionOptions<TMessage>) => {
  const hubUrl = `${BASE_URL}/notification/hub?access_token=${accessToken}`;

  // Kiểm tra kết nối hiện có
  if (connections.has(hubUrl)) {
    const existingConnection = connections.get(hubUrl)!;
    if (existingConnection.state === signalR.HubConnectionState.Connected) {
      console.log("Connection already established for:", hubUrl);
      return existingConnection;
    }
    await stopSignalRConnection(existingConnection);
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, { headers: defaultHeaders })
    .configureLogging(signalR.LogLevel.None)
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build();

  connection.on("ReceiveNotification", onReceiveMessage);

  connection.onreconnecting(() => console.warn("🔄 Reconnecting"));
  connection.onreconnected(() => console.log("✅ Reconnected"));
  connection.onclose((error) =>
    console.error("❌ Connection closed for", error),
  );

  try {
    await connection.start();
    console.log("✅ SignalR connected");
    connections.set(hubUrl, connection);
    return connection;
  } catch (error: unknown) {
    const connectionError =
      error instanceof Error
        ? error
        : new Error("Unable to establish the SignalR connection.");
    console.error("🚫 SignalR connection error:", connectionError.message);
    throw connectionError;
  }
};

export const stopSignalRConnection = async (
  connection: signalR.HubConnection,
) => {
  try {
    await connection.stop();
    const hubUrl = connection.baseUrl;
    connections.delete(hubUrl);
    console.log("🛑 SignalR connection stopped");
  } catch (err) {
    console.error("Error stopping SignalR connection:", err);
  }
};
