import { createApp } from "vue";
import { VueSignalR } from '@quangdao/vue-signalr';

import "./style.css";
import App from "./App.vue";

const app = createApp(App)
.use(VueSignalR, { url: '/api/signalr' })
  .mount("#app");
