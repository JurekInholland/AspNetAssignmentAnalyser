<script setup lang="ts">
import { onMounted, ref } from "vue";
import http from "../../http";

const result = ref("not loaded");

async function test() {
    const res = await http.get("test");
    result.value = res;
    console.log(res);

}
onMounted(test);
</script>
<template>
    <div>
        <h1>Upload</h1>
    </div>
</template>

