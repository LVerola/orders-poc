import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const connection = new HubConnectionBuilder()
  .withUrl(import.meta.env.VITE_API_URL + "/ordersHub", { withCredentials: true })
  .withAutomaticReconnect()
  .configureLogging(LogLevel.Information)
  .build();

  // connection.onclose(error => {
  //   console.error("SignalR desconectado!", error);
  // });
  // connection.onreconnecting(error => {
  //   console.warn("SignalR tentando reconectar...", error);
  // });
  // connection.onreconnected(connectionId => {
  //   console.log("SignalR reconectado!", connectionId);
  // });

export default connection;