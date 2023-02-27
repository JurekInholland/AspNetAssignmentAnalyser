<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { Icon } from '@iconify/vue';
import { useSignalR } from '@quangdao/vue-signalr';
import { IStatusMessage, ITestResult, Status } from '../models/models';
import TestResult from '../components/TestResult.vue';

const signalr = useSignalR();

const state = ref(Status.Idle);

const descriptionTexts = {
    [Status.Idle]: "Welcome! Please upload your Snake Assignment code as a .zip file below. Please make sure all required files are included and that the file is no larger than 200 kB.",
    [Status.Running]: "Your code is currently being automatically tested. This may take some time, so please wait while your submission is checked.",
    [Status.Completed]: "Your code has been tested! Please see the results below. Please get in touch if you think there is a problem with the test results.",
    [Status.Error]: "Oops, there was an error while testing your code. Please see the error message below and try again."
}

const headline = "Snake assignment hand-in";
// const infoText = "Please upload your .zip file containing your Snake Assignment code below. Please ensure that all required files are included in the zip file, but keep in mind that the maximum file size allowed is 200 kB. ";
const feedback = ref<string | null>(null);
const file = ref<File | null>(null);
const dragging = ref(false);
const inProgress = ref(false);

const currentStatus = ref("");

const testResults = ref<Array<ITestResult>>(new Array<ITestResult>());
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
    console.log("M: ", message);
    if (message.status === "done") {
        console.log("Done!")
        // inProgress.value = false;
        state.value = Status.Completed;
        currentStatus.value = `You passed ${passedTests.value}/${testResults.value.length} tests!`;
        if (passedTests.value / testResults.value.length >= 0.7) {
            currentStatus.value = currentStatus.value.replace("You", "Congratulations, you")
        }
        file.value = null;
        return;
    }

    if (!message.success) {
        // inProgress.value = false;
        state.value = Status.Error;
        currentStatus.value = "Problem encountered";
        feedback.value = message.status;
        return;
    }

    currentStatus.value = message.status
    if (message.testResult != null) {
        testResults.value.push(message.testResult);
    }
}

const onChange = (e: Event) => {
    const target = e.target as HTMLInputElement;
    feedback.value = null;

    if (validateFile(target.files!)) {
        file.value = target.files!.item(0);
    }
};

const validateFile = (files: FileList | null): boolean => {
    dragging.value = false;

    if (!files) {
        return false;
    }
    if (files.length > 1) {
        feedback.value = "Too many files. Please ensure that you only upload one file.";
        return false;
    }
    const fileToValidate = files.item(0);
    if (fileToValidate == null) {
        return false;
    }
    if (fileToValidate.type !== "application/zip" && fileToValidate.type !== "application/x-zip-compressed") {
        feedback.value = "Invalid file type. Please ensure that your file is a .zip file.";
        return false;
    }
    if (fileToValidate.size > 200000) {
        feedback.value = "File size too large. Please ensure that your file is smaller than 200 kB.";
        return false;
    }
    if (fileToValidate.size < 50000) {
        feedback.value = "File size too small. It does not look like you included all files.";
        return false;
    }
    return true;
}

const removeFile = () => {
    file.value = null;
    feedback.value = null;
};
const upload = async () => {

    // inProgress.value = true;
    const conId = signalr.connection.connectionId ?? "";
    console.log("CONNECTION ID: " + conId)
    state.value = Status.Running;

    if (file.value) {
        const formData = new FormData();
        formData.append('file', file.value);
        formData.append('connectionId', conId);
        const response = await fetch('/api/upload', {
            method: 'POST',
            body: formData
        });
        // if (response.ok) {
        // }
    }
};

const reset = () => {
    testResults.value = new Array<ITestResult>() as ITestResult[];
    feedback.value = null;
    currentStatus.value = "";
    file.value = null;
    state.value = Status.Idle;
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
        <h2 v-if="currentStatus">{{ currentStatus }}</h2>
        <p>{{ descriptionTexts[state] }}</p>

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
                <span v-if="state === Status.Running" class="loader"></span>
            </article>
            <ul v-auto-animate>
                <TestResult v-for="res in testResults" :key="res.name" :testResult="res" />
            </ul>
            <button v-if="testGrade < 1 && state !== Status.Running" class="btn2" @click="reset">
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
    position: unset;
}

li {
    margin: 16px 0;
}

article {
    display: flex;
    align-items: center;
    gap: 1rem;
}

article>svg {
    width: 3.5rem;
    height: 3.5rem;
    flex-shrink: 0;
    gap: 1rem;
}
</style>
