<script setup lang="ts">
import { ITestResult } from '../models/models';
import { Icon } from '@iconify/vue';

const props = defineProps<{
    testResult: ITestResult;
}>();
</script>

<template>
    <section>
        <p class="nr">{{ props.testResult.number }}</p>
        <p>{{ props.testResult.name }}</p>
        <Icon :style="{ 'color': props.testResult.passed ? '#00BC74' : '#FF4856' }" style="font-size: 2.25rem;"
            :icon="props.testResult.passed ? 'carbon:checkmark-filled' : 'tabler:circle-x-filled'" />
    </section>
</template>

<style scoped>
section {
    display: grid;
    align-items: center;
    grid-template-columns: 2rem 1fr 2rem;
    background: #f5f5f5;
    padding-right: 1rem;
    /* margin: 1rem; */
}

p {
    margin: 0;
    padding: 0 .5rem;

}

.nr {
    padding: 0;
    font-size: 1.25rem;
    text-align: end;
    font-weight: 600;
    width: 32px;
}
</style>
