<template>
  <div class="chart-wrap">
    <Bar :data="chartData" :options="chartOptions" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS,
  Title, Tooltip, Legend,
  BarElement, CategoryScale, LinearScale
} from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

const props = defineProps({
  results: { type: Object, required: true }
})

const COLORS = [
  '#6b0e1e', '#9b1a2e', '#b84a5a', '#c97a55',
  '#4a7a5a', '#7a4a9b', '#c9a020'
]

const chartData = computed(() => ({
  labels: props.results.options.map(o => o.text),
  datasets: [{
    label: 'Votes',
    data: props.results.options.map(o => o.votes),
    backgroundColor: props.results.options.map((_, i) => COLORS[i % COLORS.length]),
    borderRadius: 8,
    borderSkipped: false
  }]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  animation: {
    duration: 600,
    easing: 'easeOutQuart'
  },
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: ctx => ` ${ctx.raw} votes`
      }
    }
  },
  scales: {
    x: {
      ticks: { color: '#9b7c50', font: { size: 13 } },
      grid: { display: false }
    },
    y: {
      beginAtZero: true,
      ticks: { color: '#9b7c50', precision: 0 },
      grid: { color: '#e0c870' }
    }
  }
}
</script>

<style scoped>
.chart-wrap {
  position: relative;
  height: 300px;
  width: 100%;
}
</style>
