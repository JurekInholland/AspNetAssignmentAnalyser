<script setup lang="ts">
import { onMounted, ref } from 'vue';
// import { HubConnectionBuilder } from '@microsoft/signalr';
import { useSignalR } from '@quangdao/vue-signalr';

import Upload from './pages/Upload.vue';
const signalr = useSignalR();
onMounted(async () => {
  signalr.getConnectionStatus();
  console.log(signalr);

  signalr.on('ReceiveMessage', (message: string) => {
    console.log("m:" + message);
  });
});

</script>

<template>
  <Upload />
</template>

<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}

.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}

.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>
