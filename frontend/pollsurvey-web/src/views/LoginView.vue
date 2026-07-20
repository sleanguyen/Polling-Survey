<template>
  <main class="page">
    <div class="container">
      <div class="login-wrap">

        <div class="login-brand">
          <div class="brand-mark">P</div>
          <span class="brand-name">PollSurvey</span>
        </div>

        <div class="tab-row">
          <button :class="['tab-btn', { active: mode === 'login' }]"
            @click="switchMode('login')">Sign in</button>
          <button :class="['tab-btn', { active: mode === 'register' }]"
            @click="switchMode('register')">Create account</button>
        </div>

        <div class="card login-card">

          <template v-if="mode === 'login'">
            <h1 class="login-title">Welcome back</h1>
            <p class="login-sub">Sign in with your username or email.</p>

            <!-- Username or Email -->
            <div class="field">
              <label class="label">Username or Email</label>
              <input v-model="usernameOrEmail" class="input"
                placeholder="Enter username or email"
                autocomplete="username"
                @keyup.enter="submit" />
            </div>

            <!-- Password -->
            <div class="field">
              <label class="label">Password</label>
              <div class="pw-wrap">
                <input v-model="password" :type="showPw ? 'text' : 'password'"
                  class="input" placeholder="Enter your password"
                  autocomplete="current-password" @keyup.enter="submit" />
                <button class="pw-toggle" type="button" @click="showPw = !showPw" :aria-label="showPw ? 'Hide password' : 'Show password'">
                  <svg v-if="showPw" width="16" height="16" viewBox="0 0 16 16" fill="none"><path d="M1 8s2.5-5 7-5c1.4 0 2.6.4 3.6 1M15 8s-1 2-3 3.4M8 13c-4.5 0-7-5-7-5a12 12 0 0 1 2.7-3.4" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"/><path d="M1 1l14 14" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/></svg>
                  <svg v-else width="16" height="16" viewBox="0 0 16 16" fill="none"><path d="M1 8s2.5-5 7-5 7 5 7 5-2.5 5-7 5-7-5-7-5z" stroke="currentColor" stroke-width="1.4" stroke-linejoin="round"/><circle cx="8" cy="8" r="2" stroke="currentColor" stroke-width="1.4"/></svg>
                </button>
              </div>
            </div>
          </template>

          <template v-else>
            <h1 class="login-title">Create account</h1>
            <p class="login-sub">Start creating polls in seconds.</p>

            <!-- Username -->
            <div class="field">
              <label class="label">Username</label>
              <input v-model="username" class="input"
                placeholder="Choose a username"
                autocomplete="username" />
            </div>

            <!-- Email -->
            <div class="field">
              <label class="label">Email</label>
              <input v-model="emailInput" class="input" type="email"
                placeholder="Enter your email"
                autocomplete="email" />
            </div>

            <!-- Password -->
            <div class="field">
              <label class="label">Password</label>
              <div class="pw-wrap">
                <input v-model="password" :type="showPw ? 'text' : 'password'"
                  class="input" placeholder="Choose a password"
                  autocomplete="new-password" />
                <button class="pw-toggle" type="button" @click="showPw = !showPw" :aria-label="showPw ? 'Hide password' : 'Show password'">
                  <svg v-if="showPw" width="16" height="16" viewBox="0 0 16 16" fill="none"><path d="M1 8s2.5-5 7-5c1.4 0 2.6.4 3.6 1M15 8s-1 2-3 3.4M8 13c-4.5 0-7-5-7-5a12 12 0 0 1 2.7-3.4" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"/><path d="M1 1l14 14" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/></svg>
                  <svg v-else width="16" height="16" viewBox="0 0 16 16" fill="none"><path d="M1 8s2.5-5 7-5 7 5 7 5-2.5 5-7 5-7-5-7-5z" stroke="currentColor" stroke-width="1.4" stroke-linejoin="round"/><circle cx="8" cy="8" r="2" stroke="currentColor" stroke-width="1.4"/></svg>
                </button>
              </div>
            </div>

            <!-- Confirm password -->
            <div class="field">
              <label class="label">Confirm password</label>
              <input v-model="confirmPassword" type="password" class="input"
                placeholder="Repeat your password"
                autocomplete="new-password" @keyup.enter="submit" />
              <p v-if="passwordMismatch" class="field-error">Passwords do not match.</p>
            </div>
          </template>

          <!-- Error -->
          <div v-if="error" class="error-box">
            <svg class="error-icon" width="15" height="15" viewBox="0 0 16 16" fill="none">
              <path d="M8 1.5l7 12.5H1L8 1.5z" stroke="currentColor" stroke-width="1.4" stroke-linejoin="round"/>
              <path d="M8 6.5v3.5" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/>
              <circle cx="8" cy="12" r=".8" fill="currentColor"/>
            </svg>
            {{ error }}
          </div>

          <!-- Submit -->
          <button class="btn btn-primary full-btn"
            :disabled="auth.loading || !canSubmit" @click="submit">
            <svg v-if="auth.loading" class="spin" width="16" height="16" viewBox="0 0 16 16" fill="none">
              <circle cx="8" cy="8" r="6" stroke="currentColor" stroke-width="2"
                stroke-dasharray="20" stroke-dashoffset="10"/>
            </svg>
            {{ auth.loading
              ? (mode === 'login' ? 'Signing in…' : 'Creating account…')
              : (mode === 'login' ? 'Sign in' : 'Create account') }}
          </button>

          <div class="divider"><span>or</span></div>

          <RouterLink to="/create" class="btn btn-outline full-btn">
            Continue without account →
          </RouterLink>

        </div>
      </div>
    </div>
  </main>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore.js'

const router = useRouter()
const route  = useRoute()
const auth   = useAuthStore()

const mode            = ref('login')
const usernameOrEmail = ref('')   // login only
const username        = ref('')   // register only
const emailInput      = ref('')   // register only
const password        = ref('')
const confirmPassword = ref('')
const showPw          = ref(false)
const error           = ref(null)

function switchMode(m) {
  mode.value = m
  error.value = null
  password.value = ''
  confirmPassword.value = ''
}

const passwordMismatch = computed(() =>
  mode.value === 'register' &&
  confirmPassword.value.length > 0 &&
  password.value !== confirmPassword.value
)

const canSubmit = computed(() => {
  if (!password.value) return false
  if (mode.value === 'login') return !!usernameOrEmail.value.trim()
  // register
  if (!username.value.trim() || !emailInput.value.trim()) return false
  if (password.value !== confirmPassword.value) return false
  return true
})

async function submit() {
  if (!canSubmit.value || auth.loading) return
  error.value = null

  let ok = false
  if (mode.value === 'login') {
    ok = await auth.loginUser({
      usernameOrEmail: usernameOrEmail.value.trim(),
      password: password.value
    })
  } else {
    ok = await auth.registerUser({
      username: username.value.trim(),
      email:    emailInput.value.trim(),
      password: password.value
    })
  }

  if (ok) {
    const redirect = route.query.redirect ?? '/create'
    router.push(redirect)
  } else {
    error.value = auth.error
  }
}
</script>

<style scoped>
.login-wrap { max-width: 420px; margin: 0 auto; }

.login-brand {
  display: flex; align-items: center; gap: .65rem;
  justify-content: center; margin-bottom: 2rem;
}
.brand-mark {
  width: 36px; height: 36px; border-radius: 10px;
  background: var(--color-accent); color: #f5de8d;
  font-family: var(--font-display); font-size: 1.1rem; font-weight: 700;
  display: flex; align-items: center; justify-content: center;
}
.brand-name {
  font-family: var(--font-display); font-size: 1.3rem;
  font-weight: 700; color: var(--color-text);
}

.tab-row {
  display: grid; grid-template-columns: 1fr 1fr;
  background: var(--color-surface-2);
  border: 1px solid var(--color-border-soft);
  border-radius: var(--radius-md) var(--radius-md) 0 0;
  overflow: hidden;
}
.tab-btn {
  padding: .75rem; font-family: var(--font-body);
  font-size: .88rem; font-weight: 600;
  background: transparent; border: none;
  color: var(--color-muted); cursor: pointer;
  transition: all var(--transition);
}
.tab-btn.active { background: var(--color-accent); color: #f5de8d; }

.login-card {
  border-radius: 0 0 var(--radius-lg) var(--radius-lg);
  border-top: none;
}
.login-title {
  font-family: var(--font-display); font-size: 1.5rem;
  font-weight: 700; color: var(--color-text); margin-bottom: .35rem;
}
.login-sub { font-size: .9rem; color: var(--color-muted); margin-bottom: 1.5rem; }

.field { margin-bottom: 1.25rem; }
.field-error { font-size: .8rem; color: var(--color-danger); margin-top: .35rem; }

.pw-wrap { position: relative; }
.pw-wrap .input { padding-right: 2.8rem; }
.pw-toggle {
  position: absolute; right: .75rem; top: 50%; transform: translateY(-50%);
  background: none; border: none; cursor: pointer;
  padding: 0; line-height: 1; color: var(--color-muted);
  display: flex; align-items: center;
}
.pw-toggle:hover { color: var(--color-text-soft); }

.error-box {
  display: flex; align-items: center; gap: .5rem;
  background: var(--color-danger-bg); border: 1px solid rgba(185,28,28,.2);
  border-radius: var(--radius-sm); padding: .65rem .9rem;
  font-size: .85rem; color: var(--color-danger); margin-bottom: 1rem;
}
.error-icon { flex-shrink: 0; }

.full-btn { width: 100%; justify-content: center; margin-bottom: .5rem; }

.divider {
  display: flex; align-items: center; gap: .75rem;
  margin: .75rem 0; color: var(--color-muted); font-size: .8rem;
}
.divider::before, .divider::after {
  content: ''; flex: 1; height: 1px; background: var(--color-border);
}

.spin { animation: spin .8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
