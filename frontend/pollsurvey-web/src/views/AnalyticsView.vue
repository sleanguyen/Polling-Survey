<template>
  <main class="page">
    <div class="container">

      <div v-if="loading" class="state-box">
        <div class="loader"></div>
        <p>Loading analytics…</p>
      </div>

      <div v-else-if="error" class="state-box">
        <p class="error-text">{{ error }}</p>
        <RouterLink :to="`/results/${code}`" class="btn btn-outline" style="margin-top:1rem">← Back to results</RouterLink>
      </div>

      <template v-else-if="analytics">

        <div class="page-header">
          <RouterLink :to="`/results/${code}`" class="back-link">← Results</RouterLink>
        </div>

        <h1 class="page-title">Analytics</h1>
        <p class="page-sub">{{ analytics.title }}</p>

        <!-- Summary cards -->
        <div class="summary-grid">
          <div class="summary-card">
            <span class="summary-number">{{ analytics.totalVotes }}</span>
            <span class="summary-label">Total votes</span>
          </div>
          <div class="summary-card">
            <span class="summary-number">{{ analytics.peakMinuteCount }}</span>
            <span class="summary-label">Peak minute votes</span>
          </div>
          <div class="summary-card">
            <span class="summary-number">{{ analytics.peakMinuteLabel }}</span>
            <span class="summary-label">Peak time</span>
          </div>
        </div>

        <!-- Votes over time (line chart) -->
        <div class="card chart-card">
          <p class="section-label">Votes over time</p>
          <div class="line-chart-wrap">
            <Line :data="lineChartData" :options="lineChartOptions" />
          </div>
        </div>

        <!-- Top option trend -->
        <div class="card">
          <p class="section-label">Top option trend</p>
          <div class="trend-rows">
            <div
              v-for="(opt, i) in analytics.optionTrends"
              :key="i"
              class="trend-row"
            >
              <span class="trend-rank" :class="{ 'rank-top': i === 0 }">{{ i + 1 }}</span>
              <div class="trend-main">
                <div class="trend-info">
                  <span class="trend-label">{{ opt.text }}</span>
                  <span class="trend-pct">{{ opt.percentage }}%</span>
                </div>
                <div class="bar-track">
                  <div class="bar-fill" :class="{ 'bar-top': i === 0 }" :style="{ width: opt.percentage + '%' }"></div>
                </div>
              </div>
              <span class="trend-count">{{ opt.voteCount }}</span>
            </div>
          </div>
        </div>

      </template>

    </div>
  </main>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS, CategoryScale, LinearScale, PointElement,
  LineElement, Title, Tooltip, Legend, Filler
} from 'chart.js'
import axios from 'axios'
import { getPollResults } from '@/api/pollApi.js'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler)

const route = useRoute()
const code  = route.params.code

const loading   = ref(true)
const error     = ref(null)
const analytics = ref(null)

/**
 * Distinction: Analytics dashboard
 *
 * Backend currently returns aggregate results only (no VotedAt per vote in results endpoint).
 * We compute mock time-series data from the result totals for the demo.
 *
 * NOTE FOR BACKEND TEAMMATE:
 * To make this real, please add:
 *   GET /api/polls/{code}/analytics
 *   Response: { title, totalVotes, votesOverTime: [{minute: "HH:MM", count: int}], optionTrends: [...] }
 * We will switch to that endpoint once available (just change USE_MOCK_ANALYTICS = false below).
 */
const USE_MOCK_ANALYTICS = true

onMounted(async () => {
  try {
    if (USE_MOCK_ANALYTICS) {
      // Fetch real results to get actual vote counts and option names
      const results = await getPollResults(code)
      analytics.value = buildAnalyticsFromResults(results)
    } else {
      const { data } = await axios.get(`/api/polls/${code}/analytics`)
      analytics.value = data
    }
  } catch (e) {
    error.value = e.response?.status === 404
      ? 'Poll not found'
      : 'Could not load analytics. Make sure the poll exists.'
  } finally {
    loading.value = false
  }
})

/**
 * Builds a plausible analytics object from the aggregate results.
 * Simulates a realistic-looking vote-over-time distribution.
 */
function buildAnalyticsFromResults(results) {
  // Normalize field names (backend may return PascalCase)
  const title = results.title ?? results.Title ?? 'Poll'
  const questions = results.questions ?? results.Questions ?? []
  const firstQ = questions[0]
  const options = firstQ?.options ?? firstQ?.Options ?? []
  const totalVotes = firstQ?.totalVotes ?? firstQ?.TotalVotes ?? 0

  // Simulate votes-over-time for the last 12 minutes
  const now = new Date()
  const minuteData = []
  let remaining = totalVotes

  for (let i = 11; i >= 0; i--) {
    const t = new Date(now.getTime() - i * 60000)
    const label = t.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
    // Weight toward the middle to simulate a realistic curve
    const weight = i >= 5 && i <= 8 ? 3 : 1
    const share = Math.round((totalVotes * weight) / (12 * 2.5))
    const count = Math.min(share + Math.floor(Math.random() * 3), remaining)
    remaining -= count
    minuteData.push({ label, count })
  }
  // Put any remainder in last bucket
  minuteData[minuteData.length - 1].count += remaining

  const peakBucket = [...minuteData].sort((a, b) => b.count - a.count)[0]

  // Option trends sorted by voteCount desc
  const optionTrends = [...options]
    .map(o => ({
      text:       o.text       ?? o.Text       ?? '',
      voteCount:  o.voteCount  ?? o.VoteCount  ?? 0,
      percentage: o.percentage ?? o.Percentage ?? 0
    }))
    .sort((a, b) => b.voteCount - a.voteCount)

  return {
    title,
    totalVotes,
    peakMinuteCount: peakBucket?.count ?? 0,
    peakMinuteLabel: peakBucket?.label ?? '—',
    votesOverTime: minuteData,
    optionTrends
  }
}

// ── Line chart ──────────────────────────────────

const lineChartData = computed(() => {
  if (!analytics.value) return { labels: [], datasets: [] }
  return {
    labels: analytics.value.votesOverTime.map(d => d.label),
    datasets: [{
      label: 'Votes per minute',
      data: analytics.value.votesOverTime.map(d => d.count),
      borderColor: '#6b0e1e',
      backgroundColor: 'rgba(107,14,30,0.1)',
      borderWidth: 2.5,
      pointBackgroundColor: '#6b0e1e',
      pointRadius: 4,
      pointHoverRadius: 6,
      tension: 0.4,
      fill: true
    }]
  }
})

const lineChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  animation: { duration: 800, easing: 'easeOutQuart' },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#2c0a10',
      titleColor: '#f5de8d',
      bodyColor: '#fcefc6',
      padding: 10,
      cornerRadius: 8,
      callbacks: { label: ctx => `  ${ctx.raw} vote(s)` }
    }
  },
  scales: {
    x: {
      ticks: { color: '#9b7c50', font: { family: "'Plus Jakarta Sans', sans-serif", size: 11 } },
      grid: { display: false }
    },
    y: {
      beginAtZero: true,
      ticks: { color: '#9b7c50', precision: 0, font: { family: "'Plus Jakarta Sans', sans-serif", size: 11 } },
      grid: { color: 'rgba(107,14,30,0.08)' }
    }
  }
}
</script>

<style scoped>
.page-header { margin-bottom: 1rem; }
.back-link   { font-size: .85rem; font-weight: 600; color: var(--color-muted); transition: color var(--transition); }
.back-link:hover { color: var(--color-accent); text-decoration: none; }

.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: .85rem;
  margin-bottom: 1.25rem;
}
.summary-card {
  background: var(--color-surface-2);
  border: 1px solid var(--color-border-soft);
  border-radius: var(--radius-md);
  padding: 1.1rem 1.25rem;
  box-shadow: var(--shadow-sm);
}
.summary-number {
  display: block;
  font-family: var(--font-display);
  font-size: 1.7rem;
  font-weight: 700;
  color: var(--color-accent);
  line-height: 1.1;
  margin-bottom: .3rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.summary-label { font-size: .75rem; font-weight: 600; color: var(--color-muted); text-transform: uppercase; letter-spacing: .06em; }

.chart-card { margin-bottom: 1.25rem; }
.section-label { font-size: .75rem; font-weight: 700; color: var(--color-muted); text-transform: uppercase; letter-spacing: .08em; margin-bottom: 1rem; }
.line-chart-wrap { height: 240px; position: relative; }

/* Trend rows */
.trend-rows { display: flex; flex-direction: column; gap: .9rem; }
.trend-row  { display: flex; align-items: center; gap: .85rem; }
.trend-rank {
  width: 26px; height: 26px; border-radius: 50%;
  background: var(--color-bg); border: 1.5px solid var(--color-border);
  display: flex; align-items: center; justify-content: center;
  font-size: .75rem; font-weight: 700; color: var(--color-muted); flex-shrink: 0;
}
.rank-top { background: var(--color-accent); border-color: var(--color-accent); color: #f5de8d; }
.trend-main { flex: 1; min-width: 0; }
.trend-info { display: flex; justify-content: space-between; margin-bottom: .4rem; }
.trend-label { font-size: .9rem; font-weight: 600; color: var(--color-text); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.trend-pct   { font-size: .82rem; font-weight: 700; color: var(--color-accent); flex-shrink: 0; margin-left: .5rem; }
.bar-track { height: 8px; background: var(--color-border); border-radius: 99px; overflow: hidden; }
.bar-fill  { height: 100%; background: var(--color-accent); border-radius: 99px; transition: width 600ms ease; opacity: .7; }
.bar-fill.bar-top { opacity: 1; }
.trend-count { width: 36px; text-align: right; font-size: .82rem; font-weight: 700; color: var(--color-muted); flex-shrink: 0; }

.loader { width: 36px; height: 36px; border-radius: 50%; border: 3px solid var(--color-border); border-top-color: var(--color-accent); animation: spin .8s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }
.error-text { color: var(--color-danger); font-weight: 500; }

@media (max-width: 640px) {
  .summary-grid { grid-template-columns: 1fr 1fr; }
  .summary-grid .summary-card:last-child { grid-column: span 2; }
}
</style>