import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login, register, setAuthToken, restoreAuthToken } from '@/api/pollApi.js'

export const useAuthStore = defineStore('auth', () => {
  const token   = ref(null)
  const user    = ref(null)  // { username } when backend returns user info
  const loading = ref(false)
  const error   = ref(null)

  const isLoggedIn = computed(() => !!token.value)

  // Restore token on app boot
  function init() {
    const saved = restoreAuthToken()
    if (saved) token.value = saved
  }

  async function loginUser(credentials) {
    loading.value = true
    error.value   = null
    try {
      const data = await login(credentials)
      token.value = data.token
      setAuthToken(data.token)
      return true
    } catch (e) {
      error.value = e.response?.data?.message || 'Invalid username or password.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function registerUser(credentials) {
    loading.value = true
    error.value   = null
    try {
      const data = await register(credentials)
      token.value = data.token
      setAuthToken(data.token)
      return true
    } catch (e) {
      error.value = e.response?.data?.message || 'Registration failed.'
      return false
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value = null
    user.value  = null
    setAuthToken(null)
  }

  return { token, user, loading, error, isLoggedIn, init, loginUser, registerUser, logout }
})
