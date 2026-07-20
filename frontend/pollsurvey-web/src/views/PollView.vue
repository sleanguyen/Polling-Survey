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

      <template v-else-if="store.currentPoll && q">

        <div class="poll-meta-row">
          <span :class="['badge', store.currentPoll.status === 'open' ? 'badge-open' : 'badge-closed']">
            <span v-if="store.currentPoll.status === 'open'" class="live-dot"></span>
            {{ store.currentPoll.status }}
          </span>
          <span class="poll-code">poll/{{ code }}</span>
        </div>

        <!-- Poll title as heading, question text as sub -->
        <h1 class="page-title">{{ store.currentPoll.title }}</h1>
        <p v-if="q.text !== store.currentPoll.title" class="page-sub" style="margin-bottom:1.25rem">
          {{ q.text }}
        </p>

        <!-- Expiry countdown -->
        <div v-if="store.currentPoll.expiresAt && store.currentPoll.status === 'open'" class="expiry-bar">
          <svg width="13" height="13" viewBox="0 0 14 14" fill="none">
            <circle cx="7" cy="7" r="6" stroke="currentColor" stroke-width="1.4"/>
            <path d="M7 3.5V7l2.5 1.5" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <span>Closes {{ expiryLabel }}</span>
        </div>

        <!-- CLOSED -->
        <div v-if="store.currentPoll.status === 'closed'" class="notice-card closed-notice">
          <div class="notice-icon">
            <svg width="26" height="26" viewBox="0 0 20 20" fill="none">
              <rect x="4" y="9" width="12" height="8" rx="1.5" stroke="currentColor" stroke-width="1.5"/>
              <path d="M6.5 9V6.5a3.5 3.5 0 0 1 7 0V9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
          </div>
          <p class="notice-title">Đã kết thúc</p>
          <p class="notice-sub">This poll is now closed and no longer accepting votes.</p>
          <RouterLink :to="`/results/${code}`" class="btn btn-primary" style="margin-top:1.25rem">
            View final results →
          </RouterLink>
        </div>

        <!-- ALREADY VOTED -->
        <div v-else-if="store.hasVoted" class="notice-card voted-notice">
          <div class="notice-icon">✓</div>
          <p class="notice-title">Vote submitted!</p>
          <p class="notice-sub">You've already cast your vote for this poll.</p>
          <RouterLink :to="`/results/${code}`" class="btn btn-primary" style="margin-top:1.25rem">
            See live results →
          </RouterLink>
        </div>

        <!-- VOTING FORM -->
        <div v-else class="card">

          <!-- multiple_choice -->
          <template v-if="q.type === 'multiple_choice'">
            <p class="label">Choose one option</p>
            <div class="options-list">
              <button
                v-for="opt in q.options" :key="opt.id"
                :class="['option-btn', { selected: selectedOptionId === opt.id }]"
                @click="selectedOptionId = opt.id"
              >
                <span class="option-radio">
                  <span v-if="selectedOptionId === opt.id" class="radio-fill"></span>
                </span>
                <span class="option-text">{{ opt.text }}</span>
              </button>
            </div>
          </template>

          <!-- yes_no -->
          <template v-else-if="q.type === 'yes_no'">
            <p class="label">Your answer</p>
            <div class="yn-row">
              <button
                v-for="opt in q.options" :key="opt.id"
                :class="['yn-btn', opt.text === 'Yes' ? 'yes-btn' : 'no-btn',
                         { selected: selectedOptionId === opt.id }]"
                @click="selectedOptionId = opt.id"
              >
                <span>{{ opt.text }}</span>
              </button>
            </div>
          </template>

          <!-- rating -->
          <template v-else-if="q.type === 'rating'">
            <p class="label">Your rating</p>
            <div class="rating-row">
              <button
                v-for="n in 5" :key="n"
                :class="['star-btn', { active: ratingValue !== null && n <= ratingValue }]"
                @click="ratingValue = n"
              >★</button>
            </div>
            <p v-if="ratingValue" class="rating-label">{{ ratingLabels[ratingValue - 1] }}</p>
          </template>

          <!-- open_text -->
          <template v-else-if="q.type === 'open_text'">
            <p class="label">Your answer</p>
            <textarea
              v-model="openText"
              class="input open-textarea"
              placeholder="Type your answer here…"
              maxlength="500"
            ></textarea>
            <p class="char-count">{{ openText.length }} / 500</p>
          </template>

          <!-- Submit -->
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

const route  = useRoute()
const router = useRouter()
const store  = usePollStore()
const code   = route.params.code

// The first (and usually only) question
const q = computed(() => store.firstQuestion)

// Per-type answer state
const selectedOptionId = ref(null)   // for multiple_choice / yes_no  — stores Option.id (GUID)
const ratingValue      = ref(null)   // for rating                     — int 1..5
const openText         = ref('')     // for open_text

const ratingLabels = ['Poor', 'Fair', 'Good', 'Great', 'Excellent']

const canSubmit = computed(() => {
  if (!q.value) return false
  if (q.value.type === 'open_text') return openText.value.trim().length > 0
  if (q.value.type === 'rating')    return ratingValue.value !== null
  return selectedOptionId.value !== null
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
  if (!canSubmit.value || !q.value) return
  const voting_ = ref(true)

  // Build payload matching SubmitVoteRequest DTO
  const answer = {
    questionId:    q.value.id,
    optionId:      selectedOptionId.value ?? undefined,
    ratingValue:   ratingValue.value ?? undefined,
    openTextValue: openText.value.trim() || undefined
  }

  await store.vote(code, answer)
  router.push(`/results/${code}`)
}

const voting = ref(false)
async function submitVoteHandler() {
  voting.value = true
  await submitVote()
  voting.value = false
}
</script>

<style scoped>
.poll-meta-row { display:flex; align-items:center; justify-content:space-between; margin-bottom:.85rem; }
.poll-code { font-size:.78rem; color:var(--color-muted); font-family:'Courier New',monospace; }

.expiry-bar {
  display:inline-flex; align-items:center; gap:.45rem;
  font-size:.82rem; font-weight:600; color:var(--color-text-soft);
  background:rgba(107,14,30,.06); border:1px solid rgba(107,14,30,.12);
  border-radius:99px; padding:.3rem .85rem; margin-bottom:1.25rem;
}

.notice-card { border-radius:var(--radius-lg); padding:2.75rem 2rem; text-align:center; box-shadow:var(--shadow-sm); }
.closed-notice { background:#fff8e8; border:1px solid rgba(107,14,30,.15); }
.voted-notice  { background:var(--color-success-bg); border:1px solid rgba(22,101,52,.2); }
.notice-icon  { font-size:2rem; margin-bottom:.75rem; }
.notice-title { font-family:var(--font-display); font-size:1.35rem; font-weight:700; color:var(--color-text); margin-bottom:.4rem; }
.notice-sub   { font-size:.92rem; color:var(--color-muted); line-height:1.5; }

/* multiple_choice */
.options-list { display:flex; flex-direction:column; gap:.7rem; margin-top:.75rem; }
.option-btn {
  display:flex; align-items:center; gap:1rem; width:100%; text-align:left;
  background:var(--color-bg); border:1.5px solid var(--color-border);
  border-radius:var(--radius-md); padding:1rem 1.2rem;
  font-family:var(--font-body); font-size:.97rem; font-weight:500;
  color:var(--color-text); cursor:pointer;
  transition:border-color var(--transition), background var(--transition),
              box-shadow var(--transition), transform var(--transition);
}
.option-btn:hover    { border-color:var(--color-accent); background:rgba(107,14,30,.04); transform:translateX(3px); }
.option-btn.selected { border-color:var(--color-accent); background:rgba(107,14,30,.06); box-shadow:0 0 0 3px rgba(107,14,30,.1); }
.option-radio {
  width:20px; height:20px; border-radius:50%; border:2px solid var(--color-border);
  display:flex; align-items:center; justify-content:center; flex-shrink:0;
  transition:border-color var(--transition);
}
.option-btn.selected .option-radio { border-color:var(--color-accent); }
.radio-fill { width:10px; height:10px; border-radius:50%; background:var(--color-accent); }
.option-text { flex:1; }

/* yes_no */
.yn-row { display:grid; grid-template-columns:1fr 1fr; gap:1rem; margin-top:.75rem; }
.yn-btn {
  display:flex; align-items:center; justify-content:center;
  padding:1.4rem 1rem; border-radius:var(--radius-md);
  font-family:var(--font-body); font-size:1.05rem; font-weight:700;
  cursor:pointer; border:2px solid var(--color-border); background:var(--color-bg);
  transition:all var(--transition);
}
.yes-btn:hover, .yes-btn.selected { border-color:#166534; background:#dcfce7; color:#166534; }
.no-btn:hover,  .no-btn.selected  { border-color:#b91c1c; background:#fee2e2; color:#b91c1c; }

/* rating */
.rating-row { display:flex; gap:.5rem; margin-top:.75rem; }
.star-btn {
  font-size:2.2rem; background:transparent; border:none;
  cursor:pointer; color:var(--color-border);
  transition:color var(--transition), transform var(--transition); line-height:1;
}
.star-btn:hover, .star-btn.active { color:var(--color-accent); transform:scale(1.15); }
.rating-label { margin-top:.6rem; font-size:.9rem; font-weight:600; color:var(--color-accent); }

/* open_text */
.open-textarea { height:120px; resize:vertical; margin-top:.5rem; line-height:1.6; }
.char-count { text-align:right; font-size:.78rem; color:var(--color-muted); margin-top:.35rem; }

.vote-footer { display:flex; align-items:center; gap:1.2rem; margin-top:1.75rem; flex-wrap:wrap; }
.vote-btn { min-width:140px; }
.results-link { font-size:.88rem; font-weight:600; color:var(--color-muted); transition:color var(--transition); }
.results-link:hover { color:var(--color-accent); text-decoration:none; }

.loader { width:36px; height:36px; border-radius:50%; border:3px solid var(--color-border); border-top-color:var(--color-accent); animation:spin .8s linear infinite; margin:0 auto 1rem; }
@keyframes spin { to { transform:rotate(360deg); } }
.error-text { color:var(--color-danger); font-weight:500; }
.live-dot { width:6px; height:6px; border-radius:50%; background:var(--color-success); display:inline-block; animation:pulse 1.6s ease-in-out infinite; }
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.3} }
</style>
