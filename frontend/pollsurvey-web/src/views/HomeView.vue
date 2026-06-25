<template>
  <main class="page">
    <div class="container">

      <!-- Hero -->
      <section class="hero">
        <div class="hero-eyebrow">
          <span class="live-dot"></span>
          No account needed
        </div>
        <h1 class="hero-title">
          Real-time polls,<br>
          <em>beautifully simple.</em>
        </h1>
        <p class="hero-sub">
          Create a poll in seconds, share the link, and watch votes
          roll in live — no signup, no friction.
        </p>
        <div class="hero-actions">
          <RouterLink to="/create" class="btn btn-primary btn-lg">
            Create a poll →
          </RouterLink>
          <span class="hero-hint">Free &amp; instant</span>
        </div>
      </section>

      <!-- Demo polls -->
      <section class="demos">
        <p class="label">Try a demo poll</p>
        <div class="poll-grid">
          <RouterLink
            v-for="poll in demoPolls"
            :key="poll.code"
            :to="`/poll/${poll.code}`"
            class="poll-card"
          >
            <div class="poll-card-top">
              <span :class="['badge', poll.status === 'open' ? 'badge-open' : 'badge-closed']">
                <span v-if="poll.status === 'open'" class="live-dot small"></span>
                {{ poll.status }}
              </span>
              <span class="poll-votes">{{ poll.totalVotes }} votes</span>
            </div>
            <p class="poll-question">{{ poll.question }}</p>
            <div class="poll-card-footer">
              <span class="poll-opts">{{ poll.options.length }} options</span>
              <span class="poll-arrow">Vote →</span>
            </div>
          </RouterLink>
        </div>
      </section>

    </div>
  </main>
</template>

<script setup>
import { mockPolls } from '@/api/mockData.js'
const demoPolls = mockPolls
</script>

<style scoped>
/* ── Hero ───────────────────────────────────── */
.hero {
  text-align: center;
  padding: 4rem 1rem 3.5rem;
}
.hero-eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  font-size: 0.8rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--color-accent);
  background: rgba(107,14,30,0.07);
  padding: 0.35rem 0.9rem;
  border-radius: 99px;
  margin-bottom: 1.5rem;
  border: 1px solid rgba(107,14,30,0.12);
}
.hero-title {
  font-family: var(--font-display);
  font-size: clamp(2.2rem, 5vw, 3.2rem);
  font-weight: 700;
  line-height: 1.15;
  color: var(--color-text);
  margin-bottom: 1.1rem;
}
.hero-title em {
  font-style: italic;
  color: var(--color-accent);
}
.hero-sub {
  color: var(--color-muted);
  font-size: 1.05rem;
  line-height: 1.7;
  max-width: 480px;
  margin: 0 auto 2rem;
}
.hero-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  flex-wrap: wrap;
}
.btn-lg { padding: 0.85rem 2rem; font-size: 1rem; }
.hero-hint {
  font-size: 0.82rem;
  color: var(--color-muted);
  font-weight: 500;
}

/* ── Live dot ───────────────────────────────── */
.live-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  background: var(--color-success);
  display: inline-block;
  animation: pulse 1.6s ease-in-out infinite;
}
.live-dot.small { width: 6px; height: 6px; }
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:0.3} }

/* ── Demo grid ──────────────────────────────── */
.demos { margin-top: 0.5rem; }

.poll-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1rem;
  margin-top: 0.75rem;
}
.poll-card {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  background: var(--color-surface-2);
  border: 1px solid var(--color-border-soft);
  border-radius: var(--radius-md);
  padding: 1.25rem 1.35rem;
  text-decoration: none;
  box-shadow: var(--shadow-sm);
  transition: border-color var(--transition), transform var(--transition-slow),
              box-shadow var(--transition-slow);
}
.poll-card:hover {
  border-color: var(--color-accent);
  transform: translateY(-4px);
  box-shadow: var(--shadow-lift);
  text-decoration: none;
}
.poll-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.poll-votes {
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--color-muted);
}
.poll-question {
  font-family: var(--font-body);
  font-size: 0.93rem;
  font-weight: 600;
  color: var(--color-text);
  line-height: 1.45;
  flex: 1;
}
.poll-card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 0.2rem;
}
.poll-opts { font-size: 0.78rem; color: var(--color-muted); }
.poll-arrow {
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--color-accent);
  opacity: 0;
  transition: opacity var(--transition);
}
.poll-card:hover .poll-arrow { opacity: 1; }
</style>
