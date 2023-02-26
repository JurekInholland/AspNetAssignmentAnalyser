<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { Icon } from '@iconify/vue';
import { useSignalR } from '@quangdao/vue-signalr';
import { IStatusMessage, ITestResult } from '../models/models';
import TestResult from '../components/TestResult.vue';

const signalr = useSignalR();

const headline = "Snake assignment hand-in";
const infoText = "Please upload your .zip file containing your Snake Assignment code below. Please ensure that all required files are included in the zip file, but keep in mind that the maximum file size allowed is 200 kB. ";
const feedback = ref<string | null>(null);
const file = ref<File | null>(null);
const dragging = ref(false);
const inProgress = ref(false);

const currentStatus = ref("");

const testResults = ref<ITestResult[]>(new Array<ITestResult>() as ITestResult[]);
const passedTests = computed(() => testResults.value.filter(t => t.passed).length);

const testGrade = computed(() => {
    if (testResults.value.length === 0) {
        return 0;
    }
    return passedTests.value / testResults.value.length
})

onMounted(() => {
    signalr.on('StatusMessage', receiveStatusUpdate);

})

onBeforeUnmount(() => {
    signalr.off('StatusMessage', receiveStatusUpdate);
})

const receiveStatusUpdate = (message: IStatusMessage) => {
    console.log(message)
    if (!message.success || message.status === "done") {
        inProgress.value = false;
    }
    if (!message.success && message.status !== "done") {
        feedback.value = message.status;
    }
    currentStatus.value = message.status
    if (message.testResult != null) {
        testResults.value.push(message.testResult);
    }

}


const onChange = (e: Event) => {
    const target = e.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
        file.value = target.files[0];
    }
};
const removeFile = () => {
    file.value = null;
    feedback.value = null;
};
const upload = async () => {

    inProgress.value = true;
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
            console.log("GOT RESPONSE")
            currentStatus.value = `You passed ${passedTests.value}/${testResults.value.length} tests!`;
            console.log("RATING: " + passedTests.value / testResults.value.length)

            if (passedTests.value / testResults.value.length >= 0.7) {
                currentStatus.value = currentStatus.value.replace("You", "Congratulations, you")
            }
            file.value = null;
        }
    }
};
function reset() {
    testResults.value = new Array<ITestResult>() as ITestResult[];
    feedback.value = null;
    currentStatus.value = "";
    file.value = null;
}
</script>

<template>
    <div :style="{ 'padding-bottom': file === null ? '0' : '4rem' }">

        <article>
            <Icon icon="fxemoji:snake" />
            <h1>
                {{ headline }}
            </h1>
        </article>
        <p>{{ infoText }}</p>

        <p class="feedback" v-if="feedback">{{ feedback }}</p>

        <div v-if="currentStatus === ''" @dragenter="dragging = true" @dragleave="dragging = false" class="file-upload">
            <div class="upload-content">

                <div class="dragdrop" v-if="!file" :class="dragging ? 'dragged' : ''">
                    <h4>Drop your zip file here!</h4>
                    <input v-if="!file" type="file" accept=".zip" @change="onChange">

                </div>
                <div v-else class="file-info">
                    <Icon style="font-size: 104px;" icon="ph:file-zip-light"></Icon>
                    <aside>
                        <p style="color: black">{{ file?.name }}</p>
                        <p>{{ Math.ceil(file?.size / 1024) }} kB</p>
                        <button class="btn1" @click.prevent="removeFile">Remove</button>
                    </aside>
                </div>
            </div>
            <button class="btn2" v-if="file != null" @click="upload">
                Submit
            </button>
        </div>

        <div v-else>
            <article>
                <span v-if="inProgress" class="loader"></span>
                <h2>{{ currentStatus }}</h2>
            </article>
            <ul v-auto-animate>
                <li v-for=" res of testResults">
                    <TestResult :testResult="res" />
                </li>
            </ul>
            <button v-if="testGrade < 1 && !inProgress" class="btn2" @click="reset">
                Try Again
            </button>
        </div>
    </div>
</template>

<style scoped>
ul {
    list-style-type: none;
    margin: 0;
    padding: 0;
}

li {
    margin: 16px 0;
}

article {
    display: flex;
    align-items: center;
    gap: 1rem;
    flex-wrap: wrap;
}

article>svg {
    min-width: 3rem;
    min-height: 3rem;
}

</style>
