<template>
  <main class="page">
    <div class="container">

      <div v-if="store.loading && !store.currentResults" class="state-box">
        <div class="loader"></div>
        <p>Loading results…</p>
      </div>

      <template v-else-if="store.currentResults">

        <!-- Header -->
        <div class="results-header">
          <div class="header-top">
            <span :class="['badge', store.currentResults.status === 'open' ? 'badge-open' : 'badge-closed']">
              <span v-if="store.currentResults.status === 'open'" class="live-dot"></span>
              {{ store.currentResults.status === 'open' ? 'Live' : 'Closed' }}
            </span>
            <button
              v-if="store.currentResults.status === 'open'"
              class="btn btn-outline close-btn"
              @click="handleClose"
            >
              Close poll
            </button>
          </div>
          <h1 class="page-title">{{ store.currentResults.question }}</h1>
        </div>

        <!-- Stats row -->
        <div class="stats-row">
          <div class="stat-card">
            <span class="stat-number">{{ store.currentResults.totalVotes }}</span>
            <span class="stat-label">Total votes</span>
          </div>
          <div class="stat-card">
            <span class="stat-number">{{ store.currentResults.options.length }}</span>
            <span class="stat-label">Options</span>
          </div>
          <div class="stat-card">
            <span class="stat-number">{{ topOption }}</span>
            <span class="stat-label">Leading option</span>
          </div>
        </div>

        <!-- Chart -->
        <div class="card chart-card">
          <p class="section-label">Vote distribution</p>
          <VoteChart :results="store.currentResults" />
        </div>

        <!-- Breakdown table -->
        <div class="card breakdown-card">
          <p class="section-label">Breakdown</p>
          <div class="breakdown-list">
            <div
              v-for="(opt, rank) in sorted"
              :key="opt.index"
              class="breakdown-row"
            >
              <span class="rank" :class="{ 'rank-top': rank === 0 }">
                {{ rank + 1 }}
              </span>
              <div class="breakdown-main">
                <div class="breakdown-info">
                  <span class="breakdown-text">{{ opt.text }}</span>
                  <span class="breakdown-votes">{{ opt.votes }} votes</span>
                </div>
                <div class="bar-track">
                  <div
                    class="bar-fill"
                    :class="{ 'bar-top': rank === 0 }"
                    :style="{ width: pct(opt.votes) + '%' }"
                  ></div>
                </div>
              </div>
              <span class="breakdown-pct">{{ pct(opt.votes) }}%</span>
            </div>
          </div>
        </div>

        <!-- Live indicator + actions -->
        <div class="results-footer">
          <div v-if="store.currentResults.status === 'open'" class="live-indicator">
            <span class="live-dot"></span>
            <span>Updates live via SignalR</span>
          </div>
          <RouterLink :to="`/poll/${code}`" class="btn btn-outline">
            ← Vote again
          </RouterLink>
        </div>

      </template>

      <div v-else class="state-box">
        <p>Results not found.</p>
        <RouterLink to="/" class="btn btn-outline" style="margin-top:1rem">← Back home</RouterLink>
      </div>

    </div>
  </main>
</template>

<script setup>
import { computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { usePollStore } from '@/stores/pollStore.js'
import VoteChart from '@/components/VoteChart.vue'
import { connectToPoll, disconnect, onEvent, offEvent } from '@/services/signalr.js'

const route = useRoute()
const store = usePollStore()
const code  = route.params.code

const sorted = computed(() => {
  if (!store.currentResults) return []
  return [...store.currentResults.options].sort((a, b) => b.votes - a.votes)
})

const topOption = computed(() => sorted.value[0]?.text ?? '—')

function pct(votes) {
  const total = store.currentResults?.totalVotes || 0
  return total ? Math.round((votes / total) * 100) : 0
}

function handleClose() { store.close(code) }
function handleVoteUpdate(data) { store.applyLiveVoteUpdate(data) }

onMounted(async () => {
  await store.fetchResults(code)
  await connectToPoll(code)
  onEvent('VoteUpdated', handleVoteUpdate)
})
onUnmounted(() => {
  offEvent('VoteUpdated', handleVoteUpdate)
  disconnect()
})
</script>

<style scoped>
/* Header */
.results-header { margin-bottom: 1.5rem; }
.header-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.85rem;
}
.close-btn {
  font-size: 0.82rem;
  padding: 0.4rem 0.9rem;
  color: var(--color-danger);
  border-color: rgba(185,28,28,0.25);
}
.close-btn:hover {
  background: var(--color-danger-bg);
  border-color: var(--color-danger);
}

/* Stats */
.stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.85rem;
  margin-bottom: 1.25rem;
}
.stat-card {
  background: var(--color-surface-2);
  border: 1px solid var(--color-border-soft);
  border-radius: var(--radius-md);
  padding: 1.1rem 1.25rem;
  box-shadow: var(--shadow-sm);
}
.stat-number {
  display: block;
  font-family: var(--font-display);
  font-size: 1.7rem;
  font-weight: 700;
  color: var(--color-accent);
  line-height: 1.1;
  margin-bottom: 0.3rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.stat-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-muted);
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

/* Chart card */
.chart-card { margin-bottom: 1.25rem; }
.section-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--color-muted);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: 1rem;
}

/* Breakdown */
.breakdown-card { margin-bottom: 1.5rem; }
.breakdown-list { display: flex; flex-direction: column; gap: 1rem; }

.breakdown-row {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}
.rank {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: var(--color-bg);
  border: 1.5px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--color-muted);
  flex-shrink: 0;
}
.rank-top {
  background: var(--color-accent);
  border-color: var(--color-accent);
  color: #f5de8d;
}

.breakdown-main { flex: 1; min-width: 0; }
.breakdown-info {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 0.4rem;
}
.breakdown-text {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--color-text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.breakdown-votes {
  font-size: 0.78rem;
  color: var(--color-muted);
  flex-shrink: 0;
  margin-left: 0.5rem;
}
.bar-track {
  height: 8px;
  background: var(--color-border);
  border-radius: 99px;
  overflow: hidden;
}
.bar-fill {
  height: 100%;
  background: var(--color-accent);
  border-radius: 99px;
  transition: width 600ms cubic-bezier(0.4, 0, 0.2, 1);
  opacity: 0.75;
}
.bar-fill.bar-top { opacity: 1; }

.breakdown-pct {
  width: 42px;
  text-align: right;
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--color-accent);
  flex-shrink: 0;
}

/* Footer */
.results-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}
.live-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--color-success);
}

/* Loader */
.loader {
  width: 36px; height: 36px;
  border-radius: 50%;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-accent);
  animation: spin 0.8s linear infinite;
  margin: 0 auto 1rem;
}
@keyframes spin { to { transform: rotate(360deg); } }

.live-dot {
  width: 7px; height: 7px;
  border-radius: 50%;
  background: var(--color-success);
  display: inline-block;
  animation: pulse 1.6s ease-in-out infinite;
}
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:0.3} }
</style>
