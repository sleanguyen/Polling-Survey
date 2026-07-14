<template>
  <main class="page">
    <div class="container">
      <div class="login-wrap">

        <!-- Logo -->
        <div class="login-brand">
          <div class="brand-mark">P</div>
          <span class="brand-name">PollSurvey</span>
        </div>

        <!-- Tab switcher -->
        <div class="tab-row">
          <button
            :class="['tab-btn', { active: mode === 'login' }]"
            @click="mode = 'login'; error = null"
          >Sign in</button>
          <button
            :class="['tab-btn', { active: mode === 'register' }]"
            @click="mode = 'register'; error = null"
          >Create account</button>
        </div>

        <div class="card login-card">

          <!-- Sign in form -->
          <template v-if="mode === 'login'">
            <h1 class="login-title">Welcome back</h1>
            <p class="login-sub">Sign in to create and manage polls.</p>

            <div class="field">
              <label class="label">Username</label>
              <input
                v-model="username"
                class="input"
                placeholder="Enter your username"
                autocomplete="username"
                @keyup.enter="submit"
              />
            </div>
            <div class="field">
              <label class="label">Password</label>
              <div class="pw-wrap">
                <input
                  v-model="password"
                  :type="showPw ? 'text' : 'password'"
                  class="input"
                  placeholder="Enter your password"
                  autocomplete="current-password"
                  @keyup.enter="submit"
                />
                <button class="pw-toggle" @click="showPw = !showPw">
                  {{ showPw ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>

            <div v-if="error" class="error-box">{{ error }}</div>

            <!-- Note: Auth not yet implemented in backend -->
            <div class="info-note">
              <span>ℹ️</span>
              <p>Authentication is not yet available. You can still <RouterLink to="/create">create polls</RouterLink> without an account.</p>
            </div>

            <button class="btn btn-primary full-btn" :disabled="loading || !canSubmit" @click="submit">
              <svg v-if="loading" class="spin" width="16" height="16" viewBox="0 0 16 16" fill="none">
                <circle cx="8" cy="8" r="6" stroke="currentColor" stroke-width="2" stroke-dasharray="20" stroke-dashoffset="10"/>
              </svg>
              {{ loading ? 'Signing in…' : 'Sign in' }}
            </button>
          </template>

          <!-- Register form -->
          <template v-else>
            <h1 class="login-title">Create account</h1>
            <p class="login-sub">Start creating polls in seconds.</p>

            <div class="field">
              <label class="label">Username</label>
              <input
                v-model="username"
                class="input"
                placeholder="Choose a username"
                autocomplete="username"
              />
            </div>
            <div class="field">
              <label class="label">Password</label>
              <div class="pw-wrap">
                <input
                  v-model="password"
                  :type="showPw ? 'text' : 'password'"
                  class="input"
                  placeholder="Choose a password"
                  autocomplete="new-password"
                />
                <button class="pw-toggle" @click="showPw = !showPw">
                  {{ showPw ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>
            <div class="field">
              <label class="label">Confirm password</label>
              <input
                v-model="confirmPassword"
                type="password"
                class="input"
                placeholder="Repeat your password"
                autocomplete="new-password"
              />
              <p v-if="passwordMismatch" class="field-error">Passwords do not match.</p>
            </div>

            <div v-if="error" class="error-box">{{ error }}</div>

            <div class="info-note">
              <span>ℹ️</span>
              <p>Authentication is not yet available. You can still <RouterLink to="/create">create polls</RouterLink> without an account.</p>
            </div>

            <button class="btn btn-primary full-btn" :disabled="loading || !canSubmit" @click="submit">
              <svg v-if="loading" class="spin" width="16" height="16" viewBox="0 0 16 16" fill="none">
                <circle cx="8" cy="8" r="6" stroke="currentColor" stroke-width="2" stroke-dasharray="20" stroke-dashoffset="10"/>
              </svg>
              {{ loading ? 'Creating account…' : 'Create account' }}
            </button>
          </template>

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
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore.js'

const router = useRouter()
const auth   = useAuthStore()

const mode            = ref('login')
const username        = ref('')
const password        = ref('')
const confirmPassword = ref('')
const showPw          = ref(false)
const error           = ref(null)
const loading         = ref(false)

const passwordMismatch = computed(() =>
  mode.value === 'register' &&
  confirmPassword.value.length > 0 &&
  password.value !== confirmPassword.value
)

const canSubmit = computed(() => {
  if (!username.value.trim() || !password.value) return false
  if (mode.value === 'register' && password.value !== confirmPassword.value) return false
  return true
})

async function submit() {
  if (!canSubmit.value) return
  loading.value = true
  error.value   = null

  const ok = mode.value === 'login'
    ? await auth.loginUser({ username: username.value, password: password.value })
    : await auth.registerUser({ username: username.value, password: password.value })

  loading.value = false
  if (ok) router.push('/create')
  else error.value = auth.error
}
</script>

<style scoped>
.login-wrap {
  max-width: 420px;
  margin: 0 auto;
}

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
  padding: .75rem; font-family: var(--font-body); font-size: .88rem;
  font-weight: 600; background: transparent; border: none;
  color: var(--color-muted); cursor: pointer;
  transition: all var(--transition);
}
.tab-btn.active {
  background: var(--color-accent); color: #f5de8d;
}

.login-card {
  border-radius: 0 0 var(--radius-lg) var(--radius-lg);
  border-top: none;
}
.login-title {
  font-family: var(--font-display); font-size: 1.5rem;
  font-weight: 700; color: var(--color-text);
  margin-bottom: .35rem;
}
.login-sub { font-size: .9rem; color: var(--color-muted); margin-bottom: 1.5rem; }

.field { margin-bottom: 1.25rem; }
.field-error { font-size: .8rem; color: var(--color-danger); margin-top: .35rem; }

.pw-wrap { position: relative; }
.pw-wrap .input { padding-right: 2.8rem; }
.pw-toggle {
  position: absolute; right: .75rem; top: 50%;
  transform: translateY(-50%);
  background: none; border: none; cursor: pointer; font-size: 1rem;
  padding: 0; line-height: 1;
}

.error-box {
  background: var(--color-danger-bg); border: 1px solid rgba(185,28,28,.2);
  border-radius: var(--radius-sm); padding: .65rem .9rem;
  font-size: .85rem; color: var(--color-danger);
  margin-bottom: 1rem;
}

.info-note {
  display: flex; gap: .65rem; align-items: flex-start;
  background: rgba(107,14,30,.05); border: 1px solid rgba(107,14,30,.12);
  border-radius: var(--radius-sm); padding: .75rem .9rem;
  font-size: .83rem; color: var(--color-text-soft); line-height: 1.5;
  margin-bottom: 1.25rem;
}

.full-btn { width: 100%; justify-content: center; }

.divider {
  display: flex; align-items: center; gap: .75rem;
  margin: 1rem 0; color: var(--color-muted); font-size: .8rem;
}
.divider::before, .divider::after {
  content: ''; flex: 1; height: 1px; background: var(--color-border);
}

.spin { animation: spin .8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
