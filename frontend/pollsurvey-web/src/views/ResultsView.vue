<template>
  <main class="page">
    <div class="container">

      <div v-if="store.loading && !store.currentResults" class="state-box">
        Loading results…
      </div>

      <template v-else-if="store.currentResults">
        <div class="results-header">
          <span :class="['badge', store.currentResults.status === 'open' ? 'badge-open' : 'badge-closed']">
            {{ store.currentResults.status }}
          </span>
          <h1 class="page-title" style="margin-top: 0.75rem">{{ store.currentResults.question }}</h1>
          <p class="page-sub">
            {{ store.currentResults.totalVotes }} total votes
            <span v-if="store.currentResults.status === 'open'" class="live-badge">● LIVE</span>
          </p>
        </div>

        <!-- Chart -->
        <div class="card">
          <VoteChart :results="store.currentResults" />
        </div>

        <!-- Options breakdown -->
        <div class="breakdown">
          <div
            v-for="opt in sorted"
            :key="opt.index"
            class="breakdown-row"
          >
            <div class="breakdown-label">
              <span>{{ opt.text }}</span>
              <span class="breakdown-count">{{ opt.votes }}</span>
            </div>
            <div class="breakdown-bar-bg">
              <div
                class="breakdown-bar"
                :style="{ width: pct(opt.votes) + '%' }"
              ></div>
            </div>
            <span class="breakdown-pct">{{ pct(opt.votes) }}%</span>
          </div>
        </div>

        <div class="actions">
          <RouterLink :to="`/poll/${code}`" class="btn btn-outline">← Vote again</RouterLink>
        </div>
      </template>

      <div v-else class="state-box">
        Results not found.
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

function pct(votes) {
  const total = store.currentResults?.totalVotes || 0
  if (!total) return 0
  return Math.round((votes / total) * 100)
}

function handleVoteUpdate(data) {
  store.applyLiveVoteUpdate(data)
}

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
.results-header { margin-bottom: 1.5rem; }

.live-badge {
  color: var(--color-success);
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  margin-left: 0.75rem;
  animation: pulse 1.6s ease-in-out infinite;
  background: #c8e6d5;
  padding: 0.1rem 0.5rem;
  border-radius: 99px;
}
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }

.breakdown {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  margin-top: 1.5rem;
}
.breakdown-row { display: flex; align-items: center; gap: 0.75rem; }
.breakdown-label {
  display: flex;
  justify-content: space-between;
  width: 220px;
  flex-shrink: 0;
  font-size: 0.88rem;
}
.breakdown-count { color: var(--color-muted); }
.breakdown-bar-bg {
  flex: 1;
  height: 8px;
  background: var(--color-border);
  border-radius: 99px;
  overflow: hidden;
}
.breakdown-bar {
  height: 100%;
  background: var(--color-accent);
  border-radius: 99px;
  transition: width 500ms ease;
}
.breakdown-pct {
  width: 40px;
  text-align: right;
  font-size: 0.82rem;
  color: var(--color-muted);
}

.actions { margin-top: 2rem; }
</style>
