<template>
  <main class="page">
    <div class="container">

      <div v-if="store.loading && !store.currentResults" class="state-box">
        <div class="loader"></div>
        <p>Loading results…</p>
      </div>

      <template v-else-if="store.currentResults">

        <!-- Expired/Closed banner -->
        <Transition name="banner">
          <div v-if="store.currentResults.status === 'closed'" class="expired-banner">
            <span class="banner-icon">🔒</span>
            <div>
              <p class="banner-title">Đã kết thúc</p>
              <p class="banner-sub">This poll has closed. The results below are final.</p>
            </div>
          </div>
        </Transition>

        <!-- Reconnecting bar -->
        <Transition name="banner">
          <div v-if="store.connectionState === 'reconnecting'" class="reconnect-bar">
            <div class="reconnect-dot"></div>
            Reconnecting to live updates…
          </div>
        </Transition>

        <!-- Header -->
        <div class="results-header">
          <div class="header-top">
            <span :class="['badge', store.currentResults.status === 'open' ? 'badge-open' : 'badge-closed']">
              <span v-if="store.currentResults.status === 'open'" class="live-dot"></span>
              {{ store.currentResults.status === 'open' ? 'Live' : 'Closed' }}
            </span>
            <div class="header-actions">
              <RouterLink
                v-if="store.currentResults.status === 'open'"
                :to="`/analytics/${code}`"
                class="btn btn-outline analytics-btn"
              >
                📊 Analytics
              </RouterLink>
              <button
                v-if="store.currentResults.status === 'open'"
                class="btn btn-outline close-btn"
                @click="handleClose"
              >Close poll</button>
            </div>
          </div>
          <h1 class="page-title">{{ store.currentResults.title }}</h1>
        </div>

        <!-- For each question in the poll -->
        <div
          v-for="(qr, idx) in store.currentResults.questions"
          :key="qr.questionId"
          class="question-block"
        >
          <!-- Question header (shown only if multiple questions) -->
          <p v-if="store.currentResults.questions.length > 1" class="q-label">
            Q{{ idx + 1 }} · {{ qr.text }}
          </p>

          <!-- Stats row -->
          <div class="stats-row">
            <div class="stat-card">
              <span class="stat-number">{{ qr.totalVotes }}</span>
              <span class="stat-label">Total votes</span>
            </div>
            <div class="stat-card" v-if="qr.type === 'rating' && qr.averageRating">
              <span class="stat-number">{{ qr.averageRating }}</span>
              <span class="stat-label">Avg rating</span>
            </div>
            <div class="stat-card" v-else-if="qr.type !== 'open_text'">
              <span class="stat-number top-clamp">{{ topOption(qr) }}</span>
              <span class="stat-label">Leading</span>
            </div>
            <div class="stat-card" v-if="qr.type === 'open_text'">
              <span class="stat-number">{{ qr.openTextAnswers?.length ?? 0 }}</span>
              <span class="stat-label">Responses</span>
            </div>
          </div>

          <!-- open_text -->
          <template v-if="qr.type === 'open_text'">
            <div class="card responses-card">
              <p class="section-label">Responses</p>
              <div v-if="!qr.openTextAnswers?.length" class="empty-responses">No responses yet.</div>
              <div v-else class="response-list">
                <div v-for="(txt, i) in qr.openTextAnswers" :key="i" class="response-item">
                  <span class="response-idx">{{ i + 1 }}</span>
                  <p class="response-text">{{ txt }}</p>
                </div>
              </div>
            </div>
          </template>

          <!-- rating -->
          <template v-else-if="qr.type === 'rating'">
            <div class="card chart-card">
              <p class="section-label">Rating distribution</p>
              <div class="rating-avg">
                <span class="avg-number">{{ qr.averageRating ?? '—' }}</span>
                <span class="avg-stars">{{ starDisplay(qr.averageRating) }}</span>
                <span class="avg-label">average rating</span>
              </div>
              <VoteChart :results="qr" />
            </div>
          </template>

          <!-- multiple_choice / yes_no -->
          <template v-else>
            <div class="card chart-card">
              <p class="section-label">Vote distribution</p>
              <VoteChart :results="qr" />
            </div>
            <div class="card breakdown-card">
              <p class="section-label">Breakdown</p>
              <div class="breakdown-list">
                <div v-for="(opt, rank) in sortedOptions(qr)" :key="opt.optionId" class="breakdown-row">
                  <span class="rank" :class="{ 'rank-top': rank === 0 }">{{ rank + 1 }}</span>
                  <div class="breakdown-main">
                    <div class="breakdown-info">
                      <span class="breakdown-text">{{ opt.text }}</span>
                      <span class="breakdown-votes">{{ opt.voteCount }} votes</span>
                    </div>
                    <div class="bar-track">
                      <div
                        class="bar-fill"
                        :class="{ 'bar-top': rank === 0 }"
                        :style="{ width: opt.percentage + '%' }"
                      ></div>
                    </div>
                  </div>
                  <span class="breakdown-pct">{{ opt.percentage }}%</span>
                </div>
              </div>
            </div>
          </template>

        </div>

        <!-- Footer -->
        <div class="results-footer">
          <div v-if="store.currentResults.status === 'open'" class="live-indicator">
            <span class="live-dot"></span>
            <span>Updates live via SignalR</span>
          </div>
          <div v-else class="final-indicator"><span>🔒</span><span>Final results</span></div>
          <RouterLink :to="`/poll/${code}`" class="btn btn-outline">← Vote again</RouterLink>
        </div>

      </template>

      <div v-else class="state-box">
        <p>Results not found.</p>
        <RouterLink to="/" class="btn btn-outline" style="margin-top:1rem">← Back home</RouterLink>
      </div>

    </div>
  </main>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { usePollStore } from '@/stores/pollStore.js'
import VoteChart from '@/components/VoteChart.vue'
import { connectToPoll, disconnect, onEvent, offEvent } from '@/services/signalr.js'

const route = useRoute()
const store = usePollStore()
const code  = route.params.code

function sortedOptions(qr) {
  return [...(qr.options ?? [])].sort((a, b) => b.voteCount - a.voteCount)
}

function topOption(qr) {
  return sortedOptions(qr)[0]?.text ?? '—'
}

function starDisplay(avg) {
  if (!avg) return ''
  const rounded = Math.round(avg)
  return '★'.repeat(rounded) + '☆'.repeat(5 - rounded)
}

function handleClose() { store.close(code) }

// SignalR handlers
function handleResultUpdate(data) { store.applyLiveResultUpdate(data) }
function handleConnectionState(data) { store.setConnectionState(data.state) }

onMounted(async () => {
  await store.fetchResults(code)
  await connectToPoll(code)
  onEvent('ReceivePollUpdate', handleResultUpdate)
  onEvent('ConnectionState',   handleConnectionState)
})
onUnmounted(() => {
  offEvent('ReceivePollUpdate', handleResultUpdate)
  offEvent('ConnectionState',   handleConnectionState)
  disconnect(code)
})
</script>

<style scoped>
.expired-banner { display:flex; align-items:center; gap:1rem; background:#fff8e8; border:1px solid rgba(107,14,30,.2); border-radius:var(--radius-md); padding:1rem 1.25rem; margin-bottom:1.25rem; }
.banner-icon  { font-size:1.6rem; flex-shrink:0; }
.banner-title { font-family:var(--font-display); font-size:1rem; font-weight:700; color:var(--color-text); }
.banner-sub   { font-size:.85rem; color:var(--color-muted); margin-top:.15rem; }

.reconnect-bar { display:flex; align-items:center; gap:.6rem; background:#fef9c3; border:1px solid #fde047; border-radius:var(--radius-sm); padding:.6rem 1rem; font-size:.85rem; font-weight:600; color:#854d0e; margin-bottom:1rem; }
.reconnect-dot { width:8px; height:8px; border-radius:50%; background:#eab308; animation:pulse .8s ease-in-out infinite; }

.results-header { margin-bottom:1.5rem; }
.header-top { display:flex; align-items:center; justify-content:space-between; margin-bottom:.85rem; }
.header-actions { display:flex; gap:.5rem; }
.close-btn { font-size:.82rem; padding:.4rem .9rem; color:var(--color-danger); border-color:rgba(185,28,28,.25); }
.close-btn:hover { background:var(--color-danger-bg); border-color:var(--color-danger); }
.analytics-btn { font-size:.82rem; padding:.4rem .9rem; }

.question-block { margin-bottom:2rem; }
.q-label { font-size:.82rem; font-weight:700; color:var(--color-muted); text-transform:uppercase; letter-spacing:.06em; margin-bottom:.85rem; }

.stats-row { display:grid; grid-template-columns:repeat(3,1fr); gap:.85rem; margin-bottom:1.25rem; }
.stat-card { background:var(--color-surface-2); border:1px solid var(--color-border-soft); border-radius:var(--radius-md); padding:1.1rem 1.25rem; box-shadow:var(--shadow-sm); }
.stat-number { display:block; font-family:var(--font-display); font-size:1.7rem; font-weight:700; color:var(--color-accent); line-height:1.1; margin-bottom:.3rem; }
.top-clamp { font-size:1rem; line-height:1.4; overflow:hidden; display:-webkit-box; -webkit-line-clamp:2; -webkit-box-orient:vertical; }
.stat-label { font-size:.75rem; font-weight:600; color:var(--color-muted); text-transform:uppercase; letter-spacing:.06em; }

.section-label { font-size:.75rem; font-weight:700; color:var(--color-muted); text-transform:uppercase; letter-spacing:.08em; margin-bottom:1rem; }

.chart-card, .breakdown-card { margin-bottom:1.25rem; }

.rating-avg { display:flex; align-items:center; gap:.75rem; margin-bottom:1.25rem; flex-wrap:wrap; }
.avg-number { font-family:var(--font-display); font-size:2.2rem; font-weight:700; color:var(--color-accent); }
.avg-stars  { font-size:1.4rem; color:var(--color-accent); letter-spacing:.05em; }
.avg-label  { font-size:.82rem; color:var(--color-muted); font-weight:600; }

.responses-card { margin-bottom:1.25rem; }
.empty-responses { color:var(--color-muted); font-size:.9rem; text-align:center; padding:1.5rem 0; }
.response-list { display:flex; flex-direction:column; gap:.75rem; }
.response-item { display:flex; gap:.75rem; align-items:flex-start; }
.response-idx { width:24px; height:24px; border-radius:50%; background:var(--color-bg); border:1.5px solid var(--color-border); display:flex; align-items:center; justify-content:center; font-size:.72rem; font-weight:700; color:var(--color-muted); flex-shrink:0; margin-top:2px; }
.response-text { font-size:.92rem; line-height:1.55; color:var(--color-text); flex:1; }

.breakdown-list { display:flex; flex-direction:column; gap:1rem; }
.breakdown-row { display:flex; align-items:center; gap:.85rem; }
.rank { width:26px; height:26px; border-radius:50%; background:var(--color-bg); border:1.5px solid var(--color-border); display:flex; align-items:center; justify-content:center; font-size:.75rem; font-weight:700; color:var(--color-muted); flex-shrink:0; }
.rank-top { background:var(--color-accent); border-color:var(--color-accent); color:#f5de8d; }
.breakdown-main { flex:1; min-width:0; }
.breakdown-info { display:flex; justify-content:space-between; align-items:baseline; margin-bottom:.4rem; }
.breakdown-text  { font-size:.9rem; font-weight:600; color:var(--color-text); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
.breakdown-votes { font-size:.78rem; color:var(--color-muted); flex-shrink:0; margin-left:.5rem; }
.bar-track { height:8px; background:var(--color-border); border-radius:99px; overflow:hidden; }
.bar-fill  { height:100%; background:var(--color-accent); border-radius:99px; transition:width 600ms cubic-bezier(.4,0,.2,1); opacity:.7; }
.bar-fill.bar-top { opacity:1; }
.breakdown-pct { width:42px; text-align:right; font-size:.85rem; font-weight:700; color:var(--color-accent); flex-shrink:0; }

.results-footer { display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:1rem; }
.live-indicator  { display:flex; align-items:center; gap:.5rem; font-size:.82rem; font-weight:600; color:var(--color-success); }
.final-indicator { display:flex; align-items:center; gap:.5rem; font-size:.82rem; font-weight:600; color:var(--color-muted); }

.loader { width:36px; height:36px; border-radius:50%; border:3px solid var(--color-border); border-top-color:var(--color-accent); animation:spin .8s linear infinite; margin:0 auto 1rem; }
@keyframes spin { to { transform:rotate(360deg); } }
.live-dot { width:7px; height:7px; border-radius:50%; background:var(--color-success); display:inline-block; animation:pulse 1.6s ease-in-out infinite; }
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.3} }

.banner-enter-active { transition:all .35s ease; }
.banner-enter-from   { opacity:0; transform:translateY(-10px); }
.banner-leave-active { transition:all .25s ease; }
.banner-leave-to     { opacity:0; transform:translateY(-10px); }
</style>
