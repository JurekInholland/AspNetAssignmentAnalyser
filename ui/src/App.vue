<script setup lang="ts">
import { getCurrentInstance, onMounted } from 'vue';
import Upload from './pages/Upload.vue';
onMounted(() => {
  const signalr = getCurrentInstance()?.appContext.config.globalProperties.$signalR;
  console.log(signalr);
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
