<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { Icon } from '@iconify/vue';
import { useSignalR } from '@quangdao/vue-signalr';

const headline = "Snake assignment hand-in";
const infoText = "Upload your .zip file here. Make sure that all required files are included but keep the maximum file size of 250 kB in mind.";
const feedback = ref<string | null>(null);
const file = ref<File | null>(null);
const dragging = ref(false);
const signalr = useSignalR();
const onChange = (e: Event) => {
    const target = e.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
        file.value = target.files[0];
    }
};
const removeFile = () => {
    file.value = null;
};
const upload = async () => {
    const conId = signalr.connection.connectionId ?? "";

    if (file.value) {
        const formData = new FormData();
        formData.append('file', file.value);
        formData.append('connectionId', conId);
        const response = await fetch('/api/upload', {
            method: 'POST',
            body: formData
        });
        if (response.ok) {
            feedback.value = 'File uploaded successfully!';
            file.value = null;
        } else {
            console.log(response.statusText)
            feedback.value = 'Something went wrong!';
        }
    }
};


</script>

<template>
    <div id="app">
        <h1>{{ headline }}</h1>
        <p>{{ infoText }}</p>

        <p class="feedback" v-if="feedback">{{ feedback }}</p>

        <div @dragenter="dragging = true" @dragleave="dragging = false" class="file-upload">
            <div class="upload-content">

                <div class="dragdrop" v-if="!file" :class="dragging ? 'dragged' : ''">
                    <h4>Drop your zip file here!</h4>
                    <input v-if="!file" type="file" accept=".zip" @change="onChange">

                </div>
                <div v-else class="file-info">
                    <Icon style="font-size: 104px;" icon="ph:file-zip-light"></Icon>
                    <aside>
                        <p style="color: black">{{ file?.name }}</p>
                        <p>{{ Math.ceil(file?.size! / 1024) }} kB</p>
                        <button class="btn1" @click.prevent="removeFile">Remove</button>
                    </aside>
                </div>
            </div>
        </div>

        <button class="btn2" v-if="file != null" @click="upload">
            Submit
        </button>
    </div>
</template>