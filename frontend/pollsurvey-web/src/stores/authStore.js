import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login, register, setAuthToken, restoreAuthToken } from '@/api/pollApi.js'

export const useAuthStore = defineStore('auth', () => {
  const token    = ref(null)
  const username = ref(null)
  const email    = ref(null)
  const expiresAt = ref(null)
  const loading  = ref(false)
  const error    = ref(null)

  const isLoggedIn = computed(() => !!token.value)

  function init() {
    const saved = restoreAuthToken()
    if (saved) {
      token.value    = saved
      username.value = localStorage.getItem('authUsername') ?? null
      email.value    = localStorage.getItem('authEmail')    ?? null
    }
  }

  /**
   * LoginRequest: { usernameOrEmail, password }
   * LoginResponse: { Token, Username, Email, ExpiresAt }
   */
  async function loginUser({ usernameOrEmail, password }) {
    loading.value = true
    error.value   = null
    try {
      const data = await login({ usernameOrEmail, password })
      // Backend returns PascalCase: Token, Username, Email, ExpiresAt
      const jwt = data.Token ?? data.token
      if (!jwt) throw new Error('No token returned')
      token.value     = jwt
      username.value  = data.Username  ?? data.username  ?? usernameOrEmail
      email.value     = data.Email     ?? data.email     ?? ''
      expiresAt.value = data.ExpiresAt ?? data.expiresAt ?? null
      setAuthToken(jwt)
      localStorage.setItem('authUsername', username.value)
      localStorage.setItem('authEmail',    email.value)
      return true
    } catch (e) {
      error.value = e.response?.data?.message ?? e.message ?? 'Invalid credentials.'
      return false
    } finally {
      loading.value = false
    }
  }

  /**
   * RegisterRequest: { username, email, password }
   * RegisterResponse: { id, username, email, createdAt }  ← no token, must login after
   */
  async function registerUser({ username: u, email: em, password }) {
    loading.value = true
    error.value   = null
    try {
      await register({ username: u, email: em, password })
      // Backend returns 201 with user info (no token) → auto-login
      const ok = await loginUser({ usernameOrEmail: u, password })
      return ok
    } catch (e) {
      error.value = e.response?.data?.message ?? e.message ?? 'Registration failed.'
      return false
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value     = null
    username.value  = null
    email.value     = null
    expiresAt.value = null
    setAuthToken(null)
    localStorage.removeItem('authUsername')
    localStorage.removeItem('authEmail')
  }

  return {
    token, username, email, expiresAt, loading, error, isLoggedIn,
    init, loginUser, registerUser, logout
  }
})
