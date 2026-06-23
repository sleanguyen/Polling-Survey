<template>
  <main class="page">
    <div class="container">

      <div v-if="store.loading" class="state-box">Loading poll…</div>

      <div v-else-if="store.error" class="state-box">
        <p style="color: var(--color-danger)">{{ store.error }}</p>
      </div>

      <template v-else-if="store.currentPoll">
        <div class="poll-header">
          <span :class="['badge', store.currentPoll.status === 'open' ? 'badge-open' : 'badge-closed']">
            {{ store.currentPoll.status }}
          </span>
          <h1 class="page-title" style="margin-top: 0.75rem">{{ store.currentPoll.question }}</h1>
        </div>

        <!-- Closed state -->
        <div v-if="store.currentPoll.status === 'closed'" class="card state-box">
          <p>This poll is closed. <RouterLink :to="`/results/${code}`">View results →</RouterLink></p>
        </div>

        <!-- Already voted -->
        <div v-else-if="store.hasVoted" class="card state-box">
          <p>You've already voted. <RouterLink :to="`/results/${code}`">See live results →</RouterLink></p>
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
              <span class="option-radio"></span>
              {{ opt }}
            </button>
          </div>

          <button
            class="btn btn-primary"
            :disabled="selected === null || voting"
            @click="submitVote"
            style="margin-top: 1.5rem"
          >
            {{ voting ? 'Submitting…' : 'Submit vote' }}
          </button>
        </div>

        <div class="poll-footer">
          <RouterLink :to="`/results/${code}`" class="link-muted">View results</RouterLink>
        </div>
      </template>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useRouter } from 'vue-router'
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
.poll-header { margin-bottom: 1.5rem; }

.options-list { display: flex; flex-direction: column; gap: 0.65rem; margin-top: 0.5rem; }

.option-btn {
  display: flex;
  align-items: center;
  gap: 0.9rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  padding: 0.85rem 1rem;
  color: var(--color-text);
  font-size: 0.95rem;
  cursor: pointer;
  text-align: left;
  transition: border-color var(--transition), background var(--transition);
}
.option-btn:hover { border-color: var(--color-accent); background: #fdf5dc; }
.option-btn.selected {
  border-color: var(--color-accent);
  background: rgba(107, 14, 30, 0.08);
}

.option-radio {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid var(--color-border);
  flex-shrink: 0;
  transition: border-color var(--transition), background var(--transition);
}
.option-btn.selected .option-radio {
  border-color: var(--color-accent);
  background: var(--color-accent);
}

.poll-footer { margin-top: 1.25rem; text-align: center; }
.link-muted { color: var(--color-muted); font-size: 0.88rem; }
</style>
