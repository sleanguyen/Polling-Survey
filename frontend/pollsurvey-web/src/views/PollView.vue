<template>
  <main class="page">
    <div class="container">

      <div v-if="store.loading" class="state-box">
        <div class="loader"></div>
        <p>Loading poll…</p>
      </div>

      <div v-else-if="store.error" class="state-box">
        <p class="error-text">{{ store.error }}</p>
        <RouterLink to="/" class="btn btn-outline" style="margin-top:1rem">← Back home</RouterLink>
      </div>

      <template v-else-if="store.currentPoll">

        <div class="poll-meta-row">
          <span :class="['badge', store.currentPoll.status === 'open' ? 'badge-open' : 'badge-closed']">
            <span v-if="store.currentPoll.status === 'open'" class="live-dot"></span>
            {{ store.currentPoll.status }}
          </span>
          <span class="poll-code">poll/{{ code }}</span>
        </div>

        <h1 class="page-title">{{ store.currentPoll.question }}</h1>

        <!-- Closed -->
        <div v-if="store.currentPoll.status === 'closed'" class="notice-card">
          <p>This poll is now closed.</p>
          <RouterLink :to="`/results/${code}`" class="btn btn-primary" style="margin-top:1rem">
            View final results →
          </RouterLink>
        </div>

        <!-- Already voted -->
        <div v-else-if="store.hasVoted" class="notice-card">
          <p>You've already submitted your vote.</p>
          <RouterLink :to="`/results/${code}`" class="btn btn-primary" style="margin-top:1rem">
            See live results →
          </RouterLink>
        </div>

        <!-- Voting form -->
        <div v-else class="card">
          <p class="label">Choose one option</p>

          <div class="options-list">
            <button
              v-for="(opt, i) in store.currentPoll.options"
              :key="i"
              :class="['option-btn', { selected: selected === i }]"
              @click="selected = i"
            >
              <span class="option-radio">
                <span v-if="selected === i" class="radio-fill"></span>
              </span>
              <span class="option-text">{{ opt }}</span>
            </button>
          </div>

          <div class="vote-footer">
            <button
              class="btn btn-primary vote-btn"
              :disabled="selected === null || voting"
              @click="submitVote"
            >
              {{ voting ? 'Submitting…' : 'Submit vote' }}
            </button>
            <RouterLink :to="`/results/${code}`" class="results-link">
              View results →
            </RouterLink>
          </div>
        </div>

      </template>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePollStore } from '@/stores/pollStore.js'

const route  = useRoute()
const router = useRouter()
const store  = usePollStore()
const code   = route.params.code
const selected = ref(null)
const voting   = ref(false)

onMounted(() => store.fetchPoll(code))

async function submitVote() {
  if (selected.value === null) return
  voting.value = true
  await store.vote(code, selected.value)
  voting.value = false
  router.push(`/results/${code}`)
}
</script>

<style scoped>
.poll-meta-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.85rem;
}
.poll-code {
  font-size: 0.78rem;
  color: var(--color-muted);
  font-family: 'Courier New', monospace;
}

/* Notice card (closed / voted) */
.notice-card {
  background: var(--color-surface-2);
  border: 1px solid var(--color-border-soft);
  border-radius: var(--radius-lg);
  padding: 2.5rem 2rem;
  text-align: center;
  color: var(--color-text-soft);
  font-size: 0.97rem;
  box-shadow: var(--shadow-sm);
}

/* Options */
.options-list {
  display: flex;
  flex-direction: column;
  gap: 0.7rem;
  margin-top: 0.75rem;
}

.option-btn {
  display: flex;
  align-items: center;
  gap: 1rem;
  width: 100%;
  text-align: left;
  background: var(--color-bg);
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 1rem 1.2rem;
  font-family: var(--font-body);
  font-size: 0.97rem;
  font-weight: 500;
  color: var(--color-text);
  cursor: pointer;
  transition: border-color var(--transition), background var(--transition),
              box-shadow var(--transition), transform var(--transition);
}
.option-btn:hover {
  border-color: var(--color-accent);
  background: rgba(107,14,30,0.04);
  transform: translateX(3px);
}
.option-btn.selected {
  border-color: var(--color-accent);
  background: rgba(107,14,30,0.06);
  box-shadow: 0 0 0 3px rgba(107,14,30,0.1);
}

.option-radio {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 2px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: border-color var(--transition);
}
.option-btn.selected .option-radio { border-color: var(--color-accent); }

.radio-fill {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--color-accent);
}

.option-text { flex: 1; }

/* Footer */
.vote-footer {
  display: flex;
  align-items: center;
  gap: 1.2rem;
  margin-top: 1.75rem;
  flex-wrap: wrap;
}
.vote-btn { min-width: 160px; }
.results-link {
  font-size: 0.88rem;
  font-weight: 600;
  color: var(--color-muted);
  transition: color var(--transition);
}
.results-link:hover { color: var(--color-accent); text-decoration: none; }

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

.error-text { color: var(--color-danger); font-weight: 500; }

.live-dot {
  width: 6px; height: 6px;
  border-radius: 50%;
  background: var(--color-success);
  display: inline-block;
  animation: pulse 1.6s ease-in-out infinite;
}
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:0.3} }
</style>
