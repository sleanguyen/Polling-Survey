<template>
  <div class="chart-wrap">
    <Bar :data="chartData" :options="chartOptions" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS, Title, Tooltip, Legend,
  BarElement, CategoryScale, LinearScale
} from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

/**
 * Accepts a QuestionResultResponse:
 * { type, totalVotes, options: [{ optionId, text, voteCount, percentage }],
 *   averageRating, openTextAnswers }
 */
const props = defineProps({
  results: { type: Object, required: true }
})

const chartData = computed(() => {
  const opts = props.results.options ?? []
  return {
    labels: opts.map(o => o.text),
    datasets: [{
      label: 'Votes',
      data: opts.map(o => o.voteCount ?? 0),
      backgroundColor: opts.map((_, i) =>
        i === 0 ? '#6b0e1e' : 'rgba(107,14,30,0.35)'
      ),
      hoverBackgroundColor: opts.map(() => '#850f23'),
      borderRadius: 8,
      borderSkipped: false
    }]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 700, easing: 'easeOutQuart' },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#2c0a10',
      titleColor: '#f5de8d',
      bodyColor: '#fcefc6',
      padding: 10,
      cornerRadius: 8,
      callbacks: {
        label: ctx => `  ${ctx.raw} votes`
      }
    }
  },
  scales: {
    x: {
      ticks: { color: '#9b7c50', font: { family: "'Plus Jakarta Sans', sans-serif", size: 12, weight: '500' } },
      grid: { display: false },
      border: { color: '#dfc96a' }
    },
    y: {
      beginAtZero: true,
      ticks: { color: '#9b7c50', precision: 0, font: { family: "'Plus Jakarta Sans', sans-serif", size: 11 } },
      grid: { color: 'rgba(107,14,30,0.08)' },
      border: { dash: [4, 4], color: 'transparent' }
    }
  }
}
</script>

<style scoped>
.chart-wrap { position:relative; height:260px; width:100%; }
</style>
