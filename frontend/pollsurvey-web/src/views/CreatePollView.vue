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

        <!-- Question type (Merit) -->
        <div class="field">
          <label class="label">Question type</label>
          <div class="type-grid">
            <button
              v-for="t in questionTypes" :key="t.value"
              :class="['type-btn', { active: questionType === t.value }]"
              @click="questionType = t.value"
            >
              <span class="type-icon">{{ t.icon }}</span>
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
            <span class="yn-pill yes">👍 Yes</span>
            <span class="yn-pill no">👎 No</span>
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
            <span>💬</span>
            <p>Respondents will type a free-text answer. Responses are stored but not voted on.</p>
          </div>
        </div>

        <!-- Expiry (Merit) -->
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
              {{ copied ? '✓ Copied!' : 'Copy link' }}
            </button>
          </div>
          <RouterLink :to="`/poll/${createdPoll.code}`" class="btn btn-primary full-btn">
            Open voting page →
          </RouterLink>
        </div>
      </Transition>

    </div>
  </main>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { usePollStore } from '@/stores/pollStore.js'

const store       = usePollStore()
const loading     = ref(false)
const question    = ref('')
const options     = ref(['', ''])
const expiry      = ref('')
const questionType = ref('multiple_choice')
const errors      = reactive({})
const createdPoll = ref(null)
const copied      = ref(false)

const questionTypes = [
  { value: 'multiple_choice', icon: '☑️', label: 'Multiple choice' },
  { value: 'yes_no',          icon: '👍', label: 'Yes / No' },
  { value: 'rating',          icon: '⭐', label: 'Rating 1–5' },
  { value: 'open_text',       icon: '💬', label: 'Open text' }
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

function getOptionsForType() {
  if (questionType.value === 'yes_no')   return ['Yes', 'No']
  if (questionType.value === 'rating')   return ['1', '2', '3', '4', '5']
  if (questionType.value === 'open_text') return []
  return options.value.filter(o => o.trim())
}

async function submit() {
  if (!validate()) return
  loading.value = true
  const poll = await store.create({
    question:      question.value.trim(),
    question_type: questionType.value,
    options:       getOptionsForType(),
    expiresAt:     expiry.value ? computeExpiry(expiry.value) : null
  })
  loading.value = false
  if (poll) createdPoll.value = poll
  else errors.submit = 'Something went wrong. Please try again.'
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

/* Question type grid */
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
.type-icon  { font-size:1.25rem; line-height:1; }
.type-label { font-size:.72rem; font-weight:600; color:var(--color-text-soft); text-align:center; }

/* Options */
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

/* Yes/No preview */
.yn-preview   { display:flex; gap:.75rem; }
.yn-pill      { padding:.5rem 1.25rem; border-radius:99px; font-weight:700; font-size:.9rem; }
.yn-pill.yes  { background:#dcfce7; color:#166534; }
.yn-pill.no   { background:#fee2e2; color:#b91c1c; }

/* Rating preview */
.rating-preview { display:flex; gap:.4rem; }
.star-preview   { font-size:1.6rem; color:var(--color-accent); opacity:.6; }

/* Info box */
.info-box {
  display:flex; gap:.75rem; align-items:flex-start;
  background:rgba(107,14,30,.05); border:1px solid rgba(107,14,30,.12);
  border-radius:var(--radius-sm); padding:.85rem 1rem;
  font-size:.88rem; color:var(--color-text-soft); line-height:1.5;
}

.select-input { cursor:pointer; }
.form-footer  { margin-top:.5rem; }
.field-error  { font-size:.82rem; color:var(--color-danger); margin-top:.4rem; }
.spin { animation:spin .8s linear infinite; }
@keyframes spin { to { transform:rotate(360deg); } }

/* Success */
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

/* Transitions */
.opt-enter-active, .opt-leave-active { transition:all .2s ease; }
.opt-enter-from, .opt-leave-to { opacity:0; transform:translateY(-8px); }
.fade-up-enter-active { transition:all .3s ease; }
.fade-up-enter-from   { opacity:0; transform:translateY(12px); }
</style>