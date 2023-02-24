import { createApp } from "vue";
import SignalRPlugin from "./plugins/signalr";

import "./style.css";
import App from "./App.vue";

const app = createApp(App)
  .use(SignalRPlugin, { url: "/api/signalr" })
  .mount("#app");
