import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_BASE_URL
  ? `${import.meta.env.VITE_API_BASE_URL}/api`
  : '/api'

const http = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
})

// ── Auth token ─────────────────────────────────────────────────────────────

export function setAuthToken(token) {
  if (token) {
    http.defaults.headers.common['Authorization'] = `Bearer ${token}`
    localStorage.setItem('authToken', token)
  } else {
    delete http.defaults.headers.common['Authorization']
    localStorage.removeItem('authToken')
  }
}

export function restoreAuthToken() {
  const token = localStorage.getItem('authToken')
  if (token) setAuthToken(token)
  return token
}

// ── Auth endpoints ─────────────────────────────────────────────────────────

/**
 * POST /api/auth/register
 * Body: RegisterRequest { username, email, password }
 * Returns: RegisterResponse { id, username, email, createdAt }
 */
export async function register(payload) {
  const { data } = await http.post('/auth/register', {
    username: payload.username,
    email:    payload.email,
    password: payload.password
  })
  return data
}

/**
 * POST /api/auth/login
 * Body: LoginRequest { usernameOrEmail, password }
 * Returns: LoginResponse { Token, Username, Email, ExpiresAt }
 */
export async function login(payload) {
  const { data } = await http.post('/auth/login', {
    usernameOrEmail: payload.usernameOrEmail,
    password:        payload.password
  })
  return data
}

// ── Polls ──────────────────────────────────────────────────────────────────

export async function createPoll(payload) {
  const { data } = await http.post('/polls', payload)
  return data
}

export async function getPoll(code) {
  const { data } = await http.get(`/polls/${code}`)
  return data
}

export async function getPollResults(code) {
  const { data } = await http.get(`/polls/${code}/results`)
  return data
}

export function getQrCodeUrl(code) {
  const base = import.meta.env.VITE_API_BASE_URL
    ? `${import.meta.env.VITE_API_BASE_URL}/api`
    : '/api'
  return `${base}/polls/${code}/qrcode`
}

// ── Votes ──────────────────────────────────────────────────────────────────

export async function submitVote(code, payload) {
  const { data } = await http.post(`/polls/${code}/vote`, payload)
  return data
}

// ── Creator ────────────────────────────────────────────────────────────────

export async function closePoll(code) {
  const { data } = await http.patch(`/polls/${code}/close`)
  return data
}
