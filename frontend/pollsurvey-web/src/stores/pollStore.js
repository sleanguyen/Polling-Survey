import { defineStore } from 'pinia'
import { ref } from 'vue'
import { createPoll, getPoll, getPollResults, submitVote, closePoll } from '@/api/pollApi.js'

export const usePollStore = defineStore('poll', () => {
  const currentPoll    = ref(null)
  const currentResults = ref(null)
  const loading        = ref(false)
  const error          = ref(null)
  const hasVoted       = ref(false)
  const connectionState = ref('disconnected') // 'connected' | 'reconnecting' | 'disconnected'

  // ── Fetch ──────────────────────────────────────

  async function fetchPoll(code) {
    loading.value = true
    error.value = null
    try {
      currentPoll.value = await getPoll(code)
      hasVoted.value = !!sessionStorage.getItem(`voted_${code}`)
      // Check expiry on load
      checkExpiry()
    } catch (e) {
      error.value = e.message
    } finally {
      loading.value = false
    }
  }

  async function fetchResults(code) {
    loading.value = true
    error.value = null
    try {
      currentResults.value = await getPollResults(code)
      checkExpiry()
    } catch (e) {
      error.value = e.message
    } finally {
      loading.value = false
    }
  }

  // ── Vote ───────────────────────────────────────

  async function vote(code, optionIndex) {
    const voterToken = getOrCreateVoterToken()
    await submitVote(code, { optionIndex, voterToken })
    sessionStorage.setItem(`voted_${code}`, '1')
    hasVoted.value = true
    await fetchResults(code)
  }

  // ── Create ─────────────────────────────────────

  async function create(payload) {
    loading.value = true
    error.value = null
    try {
      return await createPoll(payload)
    } catch (e) {
      error.value = e.message
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

  // Called when a new vote arrives via SignalR (real or mock)
  function applyLiveVoteUpdate(data) {
    if (!currentResults.value) return
    if (currentResults.value.pollCode !== data.pollCode) return
    const option = currentResults.value.options[data.optionIndex]
    if (option) option.votes += 1
    currentResults.value.totalVotes += 1
  }

  // Called when PollClosed event arrives via SignalR
  function applyPollClosed(data) {
    if (currentPoll.value    && currentPoll.value.code    === data.pollCode)
      currentPoll.value.status    = 'closed'
    if (currentResults.value && currentResults.value.pollCode === data.pollCode)
      currentResults.value.status = 'closed'
  }

  function setConnectionState(state) {
    connectionState.value = state
  }

  // ── Expiry helper ──────────────────────────────

  function checkExpiry() {
    const poll = currentPoll.value || currentResults.value
    if (!poll || poll.status === 'closed') return
    if (!poll.expiresAt) return
    if (new Date(poll.expiresAt) < new Date()) {
      if (currentPoll.value)    currentPoll.value.status    = 'closed'
      if (currentResults.value) currentResults.value.status = 'closed'
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

  return {
    currentPoll, currentResults, loading, error, hasVoted, connectionState,
    fetchPoll, fetchResults, vote, create, close,
    applyLiveVoteUpdate, applyPollClosed, setConnectionState, checkExpiry
  }
})
