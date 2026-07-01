import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { createPoll, getPoll, getPollResults, submitVote, closePoll } from '@/api/pollApi.js'

/**
 * NOTE on schema:
 * A Poll has many Questions (List<Question>), each Question has many Options.
 * This app currently only creates polls with ONE question (simpler UX),
 * but the store/views are written generically so multi-question polls
 * from the backend still render correctly.
 */
export const usePollStore = defineStore('poll', () => {
  const currentPoll     = ref(null)   // PollResponse        { id, code, title, status, expiresAt, questions[] }
  const currentResults  = ref(null)   // PollResultResponse  { pollId, code, title, status, questions[] }
  const loading         = ref(false)
  const error           = ref(null)
  const hasVoted         = ref(false)
  const connectionState  = ref('disconnected') // 'connected' | 'reconnecting' | 'disconnected'

  // First (and currently only) question — used by single-question views
  const firstQuestion = computed(() => currentPoll.value?.questions?.[0] ?? null)
  const firstResult   = computed(() => currentResults.value?.questions?.[0] ?? null)

  // ── Fetch ──────────────────────────────────────

  async function fetchPoll(code) {
    loading.value = true
    error.value = null
    try {
      currentPoll.value = normalizePoll(await getPoll(code))
      hasVoted.value = !!sessionStorage.getItem(`voted_${code}`)
      checkExpiry()
    } catch (e) {
      error.value = e.response?.status === 404 ? 'Poll not found' : (e.message || 'Something went wrong')
    } finally {
      loading.value = false
    }
  }

  async function fetchResults(code) {
    loading.value = true
    error.value = null
    try {
      currentResults.value = normalizeResults(await getPollResults(code))
    } catch (e) {
      error.value = e.response?.status === 404 ? 'Results not found' : (e.message || 'Something went wrong')
    } finally {
      loading.value = false
    }
  }

  // ── Vote ───────────────────────────────────────

  /**
   * @param {string} code
   * @param {{ questionId: string, optionId?: string, ratingValue?: number, openTextValue?: string }} answer
   */
  async function vote(code, answer) {
    const voterToken = getOrCreateVoterToken()
    await submitVote(code, { ...answer, voterToken })
    sessionStorage.setItem(`voted_${code}`, '1')
    hasVoted.value = true
    await fetchResults(code)
  }

  // ── Create ─────────────────────────────────────

  /**
   * @param {{ title, expiresAt, questions: Array<{text, type, order, options}> }} payload
   */
  async function create(payload) {
    loading.value = true
    error.value = null
    try {
      const poll = await createPoll(payload)
      return normalizePoll(poll)
    } catch (e) {
      error.value = e.response?.data?.message || e.message || 'Something went wrong'
      return null
    } finally {
      loading.value = false
    }
  }

  // ── Close ──────────────────────────────────────

  async function close(code) {
    await closePoll(code)
    if (currentPoll.value)    currentPoll.value.status    = 'closed'
    if (currentResults.value) currentResults.value.status = 'closed'
  }

  // ── SignalR handlers ───────────────────────────

  /**
   * Backend broadcasts the FULL updated PollResultResponse via
   * hub.Clients.Group(code).SendAsync("ReceivePollUpdate", updatedResults)
   * so we just replace currentResults wholesale — no manual increment needed.
   */
  function applyLiveResultUpdate(data) {
    currentResults.value = normalizeResults(data)
  }

  function applyPollClosed() {
    if (currentPoll.value)    currentPoll.value.status    = 'closed'
    if (currentResults.value) currentResults.value.status = 'closed'
  }

  function setConnectionState(state) {
    connectionState.value = state
  }

  // ── Expiry helper (client-side early warning; backend is source of truth) ──

  function checkExpiry() {
    const poll = currentPoll.value
    if (!poll || poll.status === 'closed' || !poll.expiresAt) return
    if (new Date(poll.expiresAt) < new Date()) {
      poll.status = 'closed'
    }
  }

  // ── Token ──────────────────────────────────────

  function getOrCreateVoterToken() {
    let token = localStorage.getItem('voterToken')
    if (!token) {
      token = crypto.randomUUID()
      localStorage.setItem('voterToken', token)
    }
    return token
  }

  // ── Normalizers: backend uses PascalCase-ish camelCase from System.Text.Json,
  //    but field names differ slightly (Id vs id casing depends on serializer
  //    settings). We lower-case the first letter defensively in case the
  //    backend serializes as PascalCase. ──

  function normalizePoll(p) {
    if (!p) return p
    return {
      id: p.id ?? p.Id,
      code: p.code ?? p.Code,
      title: p.title ?? p.Title,
      status: p.status ?? p.Status,
      createdAt: p.createdAt ?? p.CreatedAt,
      expiresAt: p.expiresAt ?? p.ExpiresAt,
      questions: (p.questions ?? p.Questions ?? []).map(q => ({
        id: q.id ?? q.Id,
        text: q.text ?? q.Text,
        type: q.type ?? q.Type,
        order: q.order ?? q.Order,
        options: (q.options ?? q.Options ?? []).map(o => ({
          id: o.id ?? o.Id,
          text: o.text ?? o.Text,
          order: o.order ?? o.Order
        }))
      }))
    }
  }

  function normalizeResults(r) {
    if (!r) return r
    return {
      pollId: r.pollId ?? r.PollId,
      code: r.code ?? r.Code,
      title: r.title ?? r.Title,
      status: r.status ?? r.Status,
      questions: (r.questions ?? r.Questions ?? []).map(q => ({
        questionId: q.questionId ?? q.QuestionId,
        text: q.text ?? q.Text,
        type: q.type ?? q.Type,
        totalVotes: q.totalVotes ?? q.TotalVotes ?? 0,
        averageRating: q.averageRating ?? q.AverageRating ?? null,
        openTextAnswers: q.openTextAnswers ?? q.OpenTextAnswers ?? [],
        options: (q.options ?? q.Options ?? []).map(o => ({
          optionId: o.optionId ?? o.OptionId,
          text: o.text ?? o.Text,
          voteCount: o.voteCount ?? o.VoteCount ?? 0,
          percentage: o.percentage ?? o.Percentage ?? 0
        }))
      }))
    }
  }

  return {
    currentPoll, currentResults, loading, error, hasVoted, connectionState,
    firstQuestion, firstResult,
    fetchPoll, fetchResults, vote, create, close,
    applyLiveResultUpdate, applyPollClosed, setConnectionState, checkExpiry
  }
})
