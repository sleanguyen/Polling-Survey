import axios from 'axios'

/**
 * Trong môi trường dev: Vite proxy sẽ chuyển hướng từ /api → http://localhost:5139/api (hoặc cổng .NET của bạn)
 * Trong môi trường production: Sử dụng biến môi trường VITE_API_BASE_URL
 */
const BASE_URL = import.meta.env.VITE_API_BASE_URL
  ? `${import.meta.env.VITE_API_BASE_URL}/api`
  : '/api'

const http = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
})

// ── 📊 Polls ──────────────────────────────────────────────────────────────────

/**
 * POST /api/polls
 */
export async function createPoll(payload) {
  const { data } = await http.post('/polls', payload)
  return data
}

/**
 * GET /api/polls/{code}
 */
export async function getPoll(code) {
  const { data } = await http.get(`/polls/${code}`)
  return data
}

/**
 * GET /api/polls/{code}/results
 */
export async function getPollResults(code) {
  const { data } = await http.get(`/polls/${code}/results`)
  return data
}

// ── 🗳️ Votes ──────────────────────────────────────────────────────────────────

/**
 * POST /api/polls/{code}/vote
 */
export async function submitVote(code, payload) {
  const { data } = await http.post(`/polls/${code}/vote`, payload)
  return data
}

// ── 👑 Creator ────────────────────────────────────────────────────────────────

/**
 * PATCH /api/polls/{code}/close
 */
export async function closePoll(code) {
  const { data } = await http.patch(`/polls/${code}/close`)
  return data
}

// ── 🔐 Authentication ───────────────────────────────────────────────────────

/**
 * POST /api/auth/login
 */
export async function login(credentials) {
  const { data } = await http.post('/auth/login', credentials)
  return data
}

/**
 * POST /api/auth/register
 */
export async function register(credentials) {
  const { data } = await http.post('/auth/register', credentials)
  return data
}

/**
 * Lưu token vào LocalStorage và đính kèm vào header của mọi request
 */
export function setAuthToken(token) {
  if (token) {
    http.defaults.headers.common['Authorization'] = `Bearer ${token}`
    localStorage.setItem('poll_token', token)
  } else {
    delete http.defaults.headers.common['Authorization']
    localStorage.removeItem('poll_token')
  }
}

/**
 * Khôi phục lại token từ LocalStorage khi tải lại trang (F5)
 */
export function restoreAuthToken() {
  const token = localStorage.getItem('poll_token')
  if (token) {
    http.defaults.headers.common['Authorization'] = `Bearer ${token}`
  }
  return token
}

// ── 📷 QR Code (Giải quyết lỗi hiển thị QR trong CreatePollView.vue) ─────────

/**
 * Lấy link ảnh QR Code trực tiếp từ Backend
 */
export function getQrCodeUrl(code) {
  return `${BASE_URL}/polls/${code}/qrcode`
}