import { createApp } from "vue";
import { VueSignalR } from '@quangdao/vue-signalr';
import { autoAnimatePlugin } from '@formkit/auto-animate/vue'

import "./style.css";
import App from "./App.vue";

const app = createApp(App)
.use(VueSignalR, { url: '/api/signalr' })
.use(autoAnimatePlugin)
  .mount("#app");
