import axios from 'axios'

/**
 * In development: Vite proxy forwards /api → http://localhost:5000/api
 * In production (Vercel): VITE_API_BASE_URL points to Railway backend
 *   e.g. https://your-backend.railway.app
 */
const BASE_URL = import.meta.env.VITE_API_BASE_URL
  ? `${import.meta.env.VITE_API_BASE_URL}/api`
  : '/api'

const http = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
})

// ── Polls ──────────────────────────────────────────────────────────────────

/**
 * POST /api/polls
 * Body: CreatePollRequest { title, expiresAt?, questions: [{ text, type, order, options: [{text, order}] }] }
 * Returns: PollResponse
 */
export async function createPoll(payload) {
  const { data } = await http.post('/polls', payload)
  return data
}

/**
 * GET /api/polls/{code}
 * Returns: PollResponse { id, code, title, status, createdAt, expiresAt, questions[] }
 */
export async function getPoll(code) {
  const { data } = await http.get(`/polls/${code}`)
  return data
}

/**
 * GET /api/polls/{code}/results
 * Returns: PollResultResponse { pollId, code, title, status, questions[] }
 *   questions[].options[]: { optionId, text, voteCount, percentage }
 *   questions[].averageRating: number | null
 *   questions[].openTextAnswers: string[]
 */
export async function getPollResults(code) {
  const { data } = await http.get(`/polls/${code}/results`)
  return data
}

// ── Votes ──────────────────────────────────────────────────────────────────

/**
 * POST /api/polls/{code}/vote
 * Body: SubmitVoteRequest { questionId, optionId?, ratingValue?, openTextValue?, voterToken }
 */
export async function submitVote(code, payload) {
  const { data } = await http.post(`/polls/${code}/vote`, payload)
  return data
}

// ── Creator ────────────────────────────────────────────────────────────────

/**
 * PATCH /api/polls/{code}/close
 */
export async function closePoll(code) {
  const { data } = await http.patch(`/polls/${code}/close`)
  return data
}
