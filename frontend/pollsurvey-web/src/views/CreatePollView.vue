<template>
  <main class="page">
    <div class="container">
      <h1 class="page-title">Create a poll</h1>
      <p class="page-sub">Fill in your question and up to 6 options, then share the link.</p>

      <div class="card">
        <!-- Question -->
        <div class="field">
          <label class="label">Question</label>
          <input
            v-model="question"
            class="input"
            placeholder="e.g. What's your favourite language?"
            maxlength="200"
          />
          <p v-if="errors.question" class="field-error">{{ errors.question }}</p>
        </div>

        <!-- Options -->
        <div class="field">
          <label class="label">Options <span class="muted">(min 2, max 6)</span></label>
          <div
            v-for="(opt, i) in options"
            :key="i"
            class="option-row"
          >
            <input
              v-model="options[i]"
              class="input"
              :placeholder="`Option ${i + 1}`"
              maxlength="100"
            />
            <button
              v-if="options.length > 2"
              class="remove-btn"
              @click="removeOption(i)"
              title="Remove option"
            >✕</button>
          </div>
          <button
            v-if="options.length < 6"
            class="btn btn-outline add-btn"
            @click="addOption"
          >+ Add option</button>
          <p v-if="errors.options" class="field-error">{{ errors.options }}</p>
        </div>

        <!-- Expiry (Merit feature — wired up but optional) -->
        <div class="field">
          <label class="label">Expiry <span class="muted">(optional)</span></label>
          <select v-model="expiry" class="input">
            <option value="">No expiry</option>
            <option value="1h">1 hour</option>
            <option value="24h">24 hours</option>
            <option value="7d">7 days</option>
          </select>
        </div>

        <button
          class="btn btn-primary"
          :disabled="loading"
          @click="submit"
        >
          {{ loading ? 'Creating…' : 'Create poll' }}
        </button>

        <p v-if="errors.submit" class="field-error">{{ errors.submit }}</p>
      </div>

      <!-- Success state -->
      <div v-if="createdPoll" class="card success-card">
        <p class="success-label">Poll created!</p>
        <p class="share-link">{{ shareUrl }}</p>
        <div class="success-actions">
          <button class="btn btn-outline" @click="copyLink">
            {{ copied ? '✓ Copied!' : 'Copy link' }}
          </button>
          <RouterLink :to="`/poll/${createdPoll.code}`" class="btn btn-primary">
            Open voting page →
          </RouterLink>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { usePollStore } from '@/stores/pollStore.js'

const store   = usePollStore()
const loading = ref(false)
const question = ref('')
const options  = ref(['', ''])
const expiry   = ref('')
const errors   = reactive({})
const createdPoll = ref(null)
const copied = ref(false)

const shareUrl = () => createdPoll.value
  ? `${location.origin}/poll/${createdPoll.value.code}`
  : ''

function addOption() {
  if (options.value.length < 6) options.value.push('')
}

function removeOption(i) {
  options.value.splice(i, 1)
}

function validate() {
  delete errors.question
  delete errors.options
  delete errors.submit
  let ok = true

  if (!question.value.trim()) {
    errors.question = 'Question is required.'
    ok = false
  }

  const filled = options.value.filter(o => o.trim())
  if (filled.length < 2) {
    errors.options = 'Add at least 2 options.'
    ok = false
  }

  return ok
}

async function submit() {
  if (!validate()) return
  loading.value = true

  const expiresAt = expiry.value ? computeExpiry(expiry.value) : null
  const poll = await store.create({
    question: question.value.trim(),
    options: options.value.filter(o => o.trim()),
    expiresAt
  })

  loading.value = false
  if (poll) {
    createdPoll.value = poll
  } else {
    errors.submit = 'Something went wrong. Please try again.'
  }
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
.field { margin-bottom: 1.5rem; }

.option-row {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.remove-btn {
  background: transparent;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  color: var(--color-muted);
  padding: 0 0.7rem;
  cursor: pointer;
  transition: color var(--transition);
}
.remove-btn:hover { color: var(--color-danger); }

.add-btn { margin-top: 0.25rem; font-size: 0.85rem; }

.field-error { font-size: 0.82rem; color: var(--color-danger); margin-top: 0.35rem; }
.muted { color: var(--color-muted); font-size: 0.82rem; font-weight: 400; }

.success-card { margin-top: 1.5rem; }
.success-label { font-size: 0.82rem; font-weight: 700; color: var(--color-success); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.5rem; }
.share-link { font-family: monospace; background: var(--color-bg); padding: 0.6rem 0.9rem; border-radius: var(--radius-sm); font-size: 0.9rem; margin-bottom: 1rem; word-break: break-all; }
.success-actions { display: flex; gap: 0.75rem; flex-wrap: wrap; }
</style>
