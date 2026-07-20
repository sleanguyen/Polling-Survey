<template>
  <main class="page">
    <div class="container">
      <h1 class="page-title">Create a poll</h1>
      <p class="page-sub">Fill in your question and up to 6 options, then share the link.</p>

      <div class="card">

        <!-- Question -->
        <div class="field">
          <label class="label">Question</label>
          <input v-model="question" class="input"
            placeholder="e.g. What's your favourite framework?"
            maxlength="200" />
          <p v-if="errors.question" class="field-error">{{ errors.question }}</p>
        </div>

        <!-- Question type -->
        <div class="field">
          <label class="label">Question type</label>
          <div class="type-grid">
            <button
              v-for="t in questionTypes" :key="t.value"
              :class="['type-btn', { active: questionType === t.value }]"
              @click="questionType = t.value"
            >
              <span class="type-icon" v-html="t.icon"></span>
              <span class="type-label">{{ t.label }}</span>
            </button>
          </div>
        </div>

        <!-- Options — only for multiple_choice -->
        <div v-if="questionType === 'multiple_choice'" class="field">
          <label class="label">
            Options
            <span class="label-hint">min 2 · max 6</span>
          </label>
          <TransitionGroup name="opt" tag="div" class="options-wrap">
            <div v-for="(opt, i) in options" :key="i" class="option-row">
              <span class="opt-index">{{ i + 1 }}</span>
              <input v-model="options[i]" class="input"
                :placeholder="`Option ${i + 1}`" maxlength="100" />
              <button v-if="options.length > 2" class="remove-btn"
                @click="removeOption(i)" title="Remove">
                <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                  <path d="M2 2l10 10M12 2L2 12" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
                </svg>
              </button>
            </div>
          </TransitionGroup>
          <button v-if="options.length < 6" class="add-btn" @click="addOption">
            <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
              <path d="M7 1v12M1 7h12" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
            </svg>
            Add option
          </button>
          <p v-if="errors.options" class="field-error">{{ errors.options }}</p>
        </div>

        <!-- Yes/No preview -->
        <div v-if="questionType === 'yes_no'" class="field">
          <label class="label">Options</label>
          <div class="yn-preview">
            <span class="yn-pill yes">Yes</span>
            <span class="yn-pill no">No</span>
          </div>
        </div>

        <!-- Rating preview -->
        <div v-if="questionType === 'rating'" class="field">
          <label class="label">Rating scale 1–5</label>
          <div class="rating-preview">
            <span v-for="n in 5" :key="n" class="star-preview">★</span>
          </div>
        </div>

        <!-- Open text note -->
        <div v-if="questionType === 'open_text'" class="field">
          <div class="info-box">
            <svg class="info-icon" width="16" height="16" viewBox="0 0 16 16" fill="none">
              <path d="M2 3.5h12v7H6.5L3.5 13v-2.5H2v-7z" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round"/>
            </svg>
            <p>Respondents will type a free-text answer. Responses are stored but not voted on.</p>
          </div>
        </div>

        <!-- Expiry -->
        <div class="field">
          <label class="label">
            Expiry
            <span class="label-hint">optional</span>
          </label>
          <select v-model="expiry" class="input select-input">
            <option value="">No expiry</option>
            <option value="1h">1 hour</option>
            <option value="24h">24 hours</option>
            <option value="7d">7 days</option>
          </select>
        </div>

        <div class="form-footer">
          <button class="btn btn-primary" :disabled="loading" @click="submit">
            <svg v-if="loading" class="spin" width="16" height="16" viewBox="0 0 16 16" fill="none">
              <circle cx="8" cy="8" r="6" stroke="currentColor" stroke-width="2" stroke-dasharray="20" stroke-dashoffset="10"/>
            </svg>
            {{ loading ? 'Creating…' : 'Create poll' }}
          </button>
          <p v-if="errors.submit" class="field-error">{{ errors.submit }}</p>
        </div>
      </div>

      <!-- Success card -->
      <Transition name="fade-up">
        <div v-if="createdPoll" class="card success-card">
          <div class="success-icon">✓</div>
          <p class="success-label">Poll created!</p>

          <div class="share-row">
            <p class="share-link">{{ shareUrl() }}</p>
            <button class="btn btn-outline copy-btn" @click="copyLink">
              {{ copied ? 'Copied!' : 'Copy link' }}
            </button>
          </div>

          <!-- QR Code — fetched directly from backend as PNG -->
          <div class="qr-section">
            <p class="qr-label">Scan to vote</p>
            <div class="qr-wrapper">
              <img
                v-if="!USE_MOCK"
                :src="qrUrl"
                alt="QR Code for this poll"
                class="qr-img"
              />
              <!-- Mock mode placeholder -->
              <div v-else class="qr-placeholder">
                <span class="qr-icon">▦</span>
                <p class="qr-mock-note">QR available when backend is connected</p>
              </div>
            </div>
          </div>

          <div class="success-actions">
            <RouterLink :to="`/poll/${createdPoll.code}`" class="btn btn-primary">
              Open voting page →
            </RouterLink>
            <RouterLink :to="`/results/${createdPoll.code}`" class="btn btn-outline">
              View results
            </RouterLink>
          </div>
        </div>
      </Transition>

    </div>
  </main>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { usePollStore } from '@/stores/pollStore.js'
import { getQrCodeUrl } from '@/api/pollApi.js'

const store        = usePollStore()
const loading      = ref(false)
const question     = ref('')
const options      = ref(['', ''])
const expiry       = ref('')
const questionType = ref('multiple_choice')
const errors       = reactive({})
const createdPoll  = ref(null)
const copied       = ref(false)
const USE_MOCK     = false   // matches pollApi.js
const qrUrl        = computed(() =>
  createdPoll.value ? getQrCodeUrl(createdPoll.value.code) : ''
)

const icon = {
  list: '<svg width="18" height="18" viewBox="0 0 18 18" fill="none"><rect x="2" y="2.5" width="4.5" height="4.5" rx="1" stroke="currentColor" stroke-width="1.3"/><path d="M9.5 4.75h6.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/><rect x="2" y="11" width="4.5" height="4.5" rx="1" stroke="currentColor" stroke-width="1.3"/><path d="M9.5 13.25h6.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/></svg>',
  thumb: '<svg width="18" height="18" viewBox="0 0 18 18" fill="none"><path d="M6 8v7H3.5A1.5 1.5 0 0 1 2 13.5v-4A1.5 1.5 0 0 1 3.5 8H6zm0 0l3-6a2 2 0 0 1 2 2v3h3a1.5 1.5 0 0 1 1.4 2.05l-1.8 4.5A1.5 1.5 0 0 1 12.2 15H8a2 2 0 0 1-2-2V8z" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round"/></svg>',
  star: '<svg width="18" height="18" viewBox="0 0 18 18" fill="none"><path d="M9 1.5l2.3 4.9 5.2.6-3.8 3.6 1 5.2L9 13.2 4.3 15.8l1-5.2L1.5 7l5.2-.6L9 1.5z" stroke="currentColor" stroke-width="1.2" stroke-linejoin="round"/></svg>',
  chat: '<svg width="18" height="18" viewBox="0 0 18 18" fill="none"><path d="M2 3.5h14v8H7.5L4 15v-3.5H2v-8z" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round"/></svg>'
}

const questionTypes = [
  { value: 'multiple_choice', icon: icon.list,  label: 'Multiple choice' },
  { value: 'yes_no',          icon: icon.thumb, label: 'Yes / No' },
  { value: 'rating',          icon: icon.star,  label: 'Rating 1–5' },
  { value: 'open_text',       icon: icon.chat,  label: 'Open text' }
]

const shareUrl = () => createdPoll.value
  ? `${location.origin}/poll/${createdPoll.value.code}` : ''

function addOption()     { if (options.value.length < 6) options.value.push('') }
function removeOption(i) { options.value.splice(i, 1) }

function validate() {
  delete errors.question; delete errors.options; delete errors.submit
  let ok = true
  if (!question.value.trim()) { errors.question = 'Question is required.'; ok = false }
  if (questionType.value === 'multiple_choice' &&
      options.value.filter(o => o.trim()).length < 2) {
    errors.options = 'Add at least 2 options.'; ok = false
  }
  return ok
}

/**
 * Build the option list for the chosen question type.
 * Backend expects: List<CreateOptionRequest> { text, order }
 */
function getOptionsForType() {
  let texts = []
  if (questionType.value === 'yes_no')    texts = ['Yes', 'No']
  else if (questionType.value === 'rating')    texts = []  // rating has no Options, uses RatingValue 1-5
  else if (questionType.value === 'open_text') texts = []  // open_text has no Options
  else texts = options.value.filter(o => o.trim())

  return texts.map((text, i) => ({ text, order: i }))
}

async function submit() {
  if (!validate()) return
  loading.value = true

  // Backend shape: CreatePollRequest { title, expiresAt, questions: [{ text, type, order, options }] }
  const poll = await store.create({
    title: question.value.trim(), // poll-level title; we reuse the question text
    expiresAt: expiry.value ? computeExpiry(expiry.value) : null,
    questions: [
      {
        text: question.value.trim(),
        type: questionType.value,
        order: 0,
        options: getOptionsForType()
      }
    ]
  })

  loading.value = false
  if (poll) createdPoll.value = poll
  else errors.submit = store.error || 'Something went wrong. Please try again.'
}

async function copyLink() {
  await navigator.clipboard.writeText(shareUrl())
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}

function computeExpiry(val) {
  const now = new Date()
  if (val === '1h')  now.setHours(now.getHours() + 1)
  if (val === '24h') now.setHours(now.getHours() + 24)
  if (val === '7d')  now.setDate(now.getDate() + 7)
  return now.toISOString()
}
</script>

<style scoped>
.field { margin-bottom: 1.6rem; }
.label-hint { font-size:.72rem; font-weight:500; color:var(--color-muted); text-transform:none; letter-spacing:0; margin-left:.4rem; }

.type-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:.55rem; }
.type-btn {
  display:flex; flex-direction:column; align-items:center; gap:.35rem;
  padding:.65rem .5rem;
  background:var(--color-bg); border:1.5px solid var(--color-border);
  border-radius:var(--radius-sm); cursor:pointer; font-family:var(--font-body);
  transition:border-color var(--transition), background var(--transition);
}
.type-btn:hover   { border-color:var(--color-accent); background:rgba(107,14,30,.04); }
.type-btn.active  { border-color:var(--color-accent); background:rgba(107,14,30,.07); }
.type-icon  { display:flex; line-height:1; color:var(--color-text-soft); }
.type-btn.active .type-icon { color:var(--color-accent); }
.type-label { font-size:.72rem; font-weight:600; color:var(--color-text-soft); text-align:center; }

.options-wrap { display:flex; flex-direction:column; gap:.55rem; }
.option-row   { display:flex; align-items:center; gap:.6rem; }
.opt-index {
  width:26px; height:26px; border-radius:50%;
  background:var(--color-accent); color:#f5de8d;
  font-size:.75rem; font-weight:700;
  display:flex; align-items:center; justify-content:center; flex-shrink:0;
}
.remove-btn {
  width:32px; height:32px; display:flex; align-items:center; justify-content:center;
  flex-shrink:0; background:transparent; border:1.5px solid var(--color-border);
  border-radius:var(--radius-xs); color:var(--color-muted); cursor:pointer;
  transition:color var(--transition), border-color var(--transition), background var(--transition);
}
.remove-btn:hover { color:var(--color-danger); border-color:var(--color-danger); background:var(--color-danger-bg); }
.add-btn {
  display:inline-flex; align-items:center; gap:.5rem; margin-top:.65rem;
  font-family:var(--font-body); font-size:.88rem; font-weight:600;
  color:var(--color-accent); background:transparent;
  border:1.5px dashed rgba(107,14,30,.3); border-radius:var(--radius-sm);
  padding:.55rem 1rem; width:100%; justify-content:center; cursor:pointer;
  transition:border-color var(--transition), background var(--transition);
}
.add-btn:hover { border-color:var(--color-accent); background:rgba(107,14,30,.04); }

.yn-preview   { display:flex; gap:.75rem; }
.yn-pill      { padding:.5rem 1.25rem; border-radius:99px; font-weight:700; font-size:.9rem; }
.yn-pill.yes  { background:#dcfce7; color:#166534; }
.yn-pill.no   { background:#fee2e2; color:#b91c1c; }

.rating-preview { display:flex; gap:.4rem; }
.star-preview   { font-size:1.6rem; color:var(--color-accent); opacity:.6; }

.info-box {
  display:flex; gap:.75rem; align-items:flex-start;
  background:rgba(107,14,30,.05); border:1px solid rgba(107,14,30,.12);
  border-radius:var(--radius-sm); padding:.85rem 1rem;
  font-size:.88rem; color:var(--color-text-soft); line-height:1.5;
}
.info-icon { flex-shrink:0; margin-top:.15rem; color:var(--color-accent); }

.select-input { cursor:pointer; }
.form-footer  { margin-top:.5rem; }
.field-error  { font-size:.82rem; color:var(--color-danger); margin-top:.4rem; }
.spin { animation:spin .8s linear infinite; }
@keyframes spin { to { transform:rotate(360deg); } }

.success-card { margin-top:1.25rem; text-align:center; }
.success-icon {
  width:44px; height:44px; border-radius:50%;
  background:var(--color-success-bg); color:var(--color-success);
  font-size:1.2rem; font-weight:700;
  display:flex; align-items:center; justify-content:center;
  margin:0 auto .75rem;
}
.success-label { font-family:var(--font-display); font-size:1.2rem; font-weight:700; color:var(--color-text); margin-bottom:1rem; }
.share-row {
  display:flex; align-items:center; gap:.75rem;
  background:var(--color-bg); border:1.5px solid var(--color-border);
  border-radius:var(--radius-sm); padding:.6rem .85rem; margin-bottom:1rem; text-align:left;
}
.share-link { font-family:'Courier New',monospace; font-size:.85rem; color:var(--color-text-soft); flex:1; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.copy-btn { white-space:nowrap; flex-shrink:0; }
.full-btn { width:100%; }

.opt-enter-active, .opt-leave-active { transition:all .2s ease; }
.opt-enter-from, .opt-leave-to { opacity:0; transform:translateY(-8px); }
.fade-up-enter-active { transition:all .3s ease; }
.fade-up-enter-from   { opacity:0; transform:translateY(12px); }

/* QR Code */
.qr-section { margin: 1.25rem 0; }
.qr-label {
  font-size: .75rem; font-weight: 700; color: var(--color-muted);
  text-transform: uppercase; letter-spacing: .08em; margin-bottom: .75rem;
}
.qr-wrapper {
  display: flex; justify-content: center;
}
.qr-img {
  width: 160px; height: 160px;
  border-radius: var(--radius-sm);
  border: 4px solid var(--color-surface-2);
  box-shadow: var(--shadow-md);
}
.qr-placeholder {
  width: 160px; height: 160px;
  background: var(--color-bg);
  border: 2px dashed var(--color-border);
  border-radius: var(--radius-sm);
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  gap: .5rem;
}
.qr-icon { font-size: 2.5rem; color: var(--color-muted); opacity: .4; }
.qr-mock-note { font-size: .72rem; color: var(--color-muted); text-align: center; padding: 0 .5rem; }

.success-actions {
  display: flex; gap: .75rem; flex-wrap: wrap;
  margin-top: .5rem;
}
.success-actions .btn { flex: 1; min-width: 130px; }

</style>
