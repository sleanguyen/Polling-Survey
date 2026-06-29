<template>
  <main class="page">
    <div class="container">

      <!-- Loading -->
      <div v-if="store.loading" class="state-box">
        <div class="loader"></div>
        <p>Loading poll…</p>
      </div>

      <!-- Error -->
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

        <!-- Expiry countdown (Merit) -->
        <div v-if="store.currentPoll.expiresAt && store.currentPoll.status === 'open'" class="expiry-bar">
          <span class="expiry-icon">⏳</span>
          <span>Closes {{ expiryLabel }}</span>
        </div>

        <!-- ── CLOSED ── -->
        <div v-if="store.currentPoll.status === 'closed'" class="notice-card closed-notice">
          <div class="notice-icon">🔒</div>
          <p class="notice-title">Đã kết thúc</p>
          <p class="notice-sub">This poll is now closed and no longer accepting votes.</p>
          <RouterLink :to="`/results/${code}`" class="btn btn-primary" style="margin-top:1.25rem">
            View final results →
          </RouterLink>
        </div>

        <!-- ── ALREADY VOTED ── -->
        <div v-else-if="store.hasVoted" class="notice-card voted-notice">
          <div class="notice-icon">✓</div>
          <p class="notice-title">Vote submitted!</p>
          <p class="notice-sub">You've already cast your vote for this poll.</p>
          <RouterLink :to="`/results/${code}`" class="btn btn-primary" style="margin-top:1.25rem">
            See live results →
          </RouterLink>
        </div>

        <!-- ── VOTING FORM ── -->
        <div v-else class="card">

          <!-- multiple_choice -->
          <template v-if="pollType === 'multiple_choice'">
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
          </template>

          <!-- yes_no -->
          <template v-else-if="pollType === 'yes_no'">
            <p class="label">Your answer</p>
            <div class="yn-row">
              <button
                :class="['yn-btn yes-btn', { selected: selected === 0 }]"
                @click="selected = 0"
              >
                <span class="yn-emoji">👍</span>
                <span>Yes</span>
              </button>
              <button
                :class="['yn-btn no-btn', { selected: selected === 1 }]"
                @click="selected = 1"
              >
                <span class="yn-emoji">👎</span>
                <span>No</span>
              </button>
            </div>
          </template>

          <!-- rating -->
          <template v-else-if="pollType === 'rating'">
            <p class="label">Your rating</p>
            <div class="rating-row">
              <button
                v-for="n in 5"
                :key="n"
                :class="['star-btn', { active: selected !== null && n <= selected + 1 }]"
                @click="selected = n - 1"
              >★</button>
            </div>
            <p v-if="selected !== null" class="rating-label">
              {{ ratingLabels[selected] }}
            </p>
          </template>

          <!-- open_text -->
          <template v-else-if="pollType === 'open_text'">
            <p class="label">Your answer</p>
            <textarea
              v-model="openText"
              class="input open-textarea"
              placeholder="Type your answer here…"
              maxlength="500"
            ></textarea>
            <p class="char-count">{{ openText.length }} / 500</p>
          </template>

          <!-- Submit footer -->
          <div class="vote-footer">
            <button
              class="btn btn-primary vote-btn"
              :disabled="!canSubmit || voting"
              @click="submitVote"
            >
              {{ voting ? 'Submitting…' : 'Submit' }}
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
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePollStore } from '@/stores/pollStore.js'
import { submitOpenText } from '@/api/pollApi.js'

const route  = useRoute()
const router = useRouter()
const store  = usePollStore()
const code   = route.params.code

const selected = ref(null)
const openText = ref('')
const voting   = ref(false)

const ratingLabels = ['Poor', 'Fair', 'Good', 'Great', 'Excellent']

const pollType = computed(() => store.currentPoll?.question_type || 'multiple_choice')

const canSubmit = computed(() => {
  if (pollType.value === 'open_text') return openText.value.trim().length > 0
  return selected.value !== null
})

const expiryLabel = computed(() => {
  const exp = store.currentPoll?.expiresAt
  if (!exp) return ''
  const diff = new Date(exp) - new Date()
  if (diff <= 0) return 'soon'
  const h = Math.floor(diff / 3600000)
  const m = Math.floor((diff % 3600000) / 60000)
  if (h > 24) return `in ${Math.floor(h / 24)} day(s)`
  if (h > 0)  return `in ${h}h ${m}m`
  return `in ${m} minute(s)`
})

onMounted(() => store.fetchPoll(code))

async function submitVote() {
  voting.value = true
  try {
    if (pollType.value === 'open_text') {
      await submitOpenText(code, { text: openText.value.trim() })
      sessionStorage.setItem(`voted_${code}`, '1')
      store.hasVoted = true
    } else {
      await store.vote(code, selected.value)
    }
    router.push(`/results/${code}`)
  } finally {
    voting.value = false
  }
}
</script>

<style scoped>
.poll-meta-row {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 0.85rem;
}
.poll-code { font-size: 0.78rem; color: var(--color-muted); font-family: 'Courier New', monospace; }

/* Expiry bar */
.expiry-bar {
  display: inline-flex; align-items: center; gap: 0.45rem;
  font-size: 0.82rem; font-weight: 600; color: var(--color-text-soft);
  background: rgba(107,14,30,0.06); border: 1px solid rgba(107,14,30,0.12);
  border-radius: 99px; padding: 0.3rem 0.85rem; margin-bottom: 1.25rem;
}
.expiry-icon { font-size: 0.9rem; }

/* Notice cards */
.notice-card {
  border-radius: var(--radius-lg); padding: 2.75rem 2rem;
  text-align: center; box-shadow: var(--shadow-sm);
}
.closed-notice { background: #fff8e8; border: 1px solid rgba(107,14,30,0.15); }
.voted-notice  { background: var(--color-success-bg); border: 1px solid rgba(22,101,52,0.2); }
.notice-icon { font-size: 2rem; margin-bottom: 0.75rem; }
.notice-title {
  font-family: var(--font-display); font-size: 1.35rem; font-weight: 700;
  color: var(--color-text); margin-bottom: 0.4rem;
}
.notice-sub { font-size: 0.92rem; color: var(--color-muted); line-height: 1.5; }

/* multiple_choice options */
.options-list { display: flex; flex-direction: column; gap: 0.7rem; margin-top: 0.75rem; }
.option-btn {
  display: flex; align-items: center; gap: 1rem; width: 100%; text-align: left;
  background: var(--color-bg); border: 1.5px solid var(--color-border);
  border-radius: var(--radius-md); padding: 1rem 1.2rem;
  font-family: var(--font-body); font-size: 0.97rem; font-weight: 500;
  color: var(--color-text); cursor: pointer;
  transition: border-color var(--transition), background var(--transition),
              box-shadow var(--transition), transform var(--transition);
}
.option-btn:hover    { border-color: var(--color-accent); background: rgba(107,14,30,0.04); transform: translateX(3px); }
.option-btn.selected { border-color: var(--color-accent); background: rgba(107,14,30,0.06); box-shadow: 0 0 0 3px rgba(107,14,30,0.1); }
.option-radio {
  width: 20px; height: 20px; border-radius: 50%; border: 2px solid var(--color-border);
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
  transition: border-color var(--transition);
}
.option-btn.selected .option-radio { border-color: var(--color-accent); }
.radio-fill { width: 10px; height: 10px; border-radius: 50%; background: var(--color-accent); }
.option-text { flex: 1; }

/* yes_no */
.yn-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-top: 0.75rem; }
.yn-btn {
  display: flex; flex-direction: column; align-items: center; gap: 0.5rem;
  padding: 1.5rem 1rem; border-radius: var(--radius-md);
  font-family: var(--font-body); font-size: 1rem; font-weight: 700;
  cursor: pointer; border: 2px solid var(--color-border);
  background: var(--color-bg);
  transition: all var(--transition);
}
.yn-emoji { font-size: 2rem; }
.yes-btn:hover, .yes-btn.selected { border-color: #166534; background: #dcfce7; color: #166534; }
.no-btn:hover,  .no-btn.selected  { border-color: #b91c1c; background: #fee2e2; color: #b91c1c; }

/* rating */
.rating-row { display: flex; gap: 0.5rem; margin-top: 0.75rem; }
.star-btn {
  font-size: 2.2rem; background: transparent; border: none;
  cursor: pointer; color: var(--color-border);
  transition: color var(--transition), transform var(--transition);
  line-height: 1;
}
.star-btn:hover, .star-btn.active { color: var(--color-accent); transform: scale(1.15); }
.rating-label {
  margin-top: 0.6rem; font-size: 0.9rem; font-weight: 600;
  color: var(--color-accent);
}

/* open_text */
.open-textarea {
  height: 120px; resize: vertical;
  margin-top: 0.5rem; line-height: 1.6;
}
.char-count {
  text-align: right; font-size: 0.78rem;
  color: var(--color-muted); margin-top: 0.35rem;
}

/* Footer */
.vote-footer {
  display: flex; align-items: center; gap: 1.2rem;
  margin-top: 1.75rem; flex-wrap: wrap;
}
.vote-btn { min-width: 140px; }
.results-link {
  font-size: 0.88rem; font-weight: 600; color: var(--color-muted);
  transition: color var(--transition);
}
.results-link:hover { color: var(--color-accent); text-decoration: none; }

/* Loader */
.loader {
  width: 36px; height: 36px; border-radius: 50%;
  border: 3px solid var(--color-border); border-top-color: var(--color-accent);
  animation: spin 0.8s linear infinite; margin: 0 auto 1rem;
}
@keyframes spin { to { transform: rotate(360deg); } }
.error-text { color: var(--color-danger); font-weight: 500; }

.live-dot {
  width: 6px; height: 6px; border-radius: 50%;
  background: var(--color-success); display: inline-block;
  animation: pulse 1.6s ease-in-out infinite;
}
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:0.3} }
</style>
