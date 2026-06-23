<template>
  <main class="page">
    <div class="container">
      <div class="hero">
        <h1 class="hero-title">Real-time polls,<br>no account needed.</h1>
        <p class="hero-sub">Create a poll, share the link, watch votes roll in live.</p>
        <RouterLink to="/create" class="btn btn-primary btn-lg">Create a poll</RouterLink>
      </div>

      <section class="demos">
        <p class="label">Try a demo poll</p>
        <div class="poll-grid">
          <RouterLink
            v-for="poll in demoPolls"
            :key="poll.code"
            :to="`/poll/${poll.code}`"
            class="poll-card"
          >
            <span :class="['badge', poll.status === 'open' ? 'badge-open' : 'badge-closed']">
              {{ poll.status }}
            </span>
            <p class="poll-question">{{ poll.question }}</p>
            <p class="poll-meta">{{ poll.totalVotes }} votes · {{ poll.options.length }} options</p>
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
.hero {
  text-align: center;
  padding: 4rem 0 3rem;
}
.hero-title {
  font-size: clamp(2rem, 5vw, 3rem);
  font-weight: 800;
  line-height: 1.15;
  margin-bottom: 1rem;
}
.accent { color: var(--color-accent); }
.hero-sub {
  color: var(--color-muted);
  font-size: 1.1rem;
  margin-bottom: 2rem;
}
.btn-lg { padding: 0.85rem 2rem; font-size: 1rem; }

.demos { margin-top: 1rem; }
.poll-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 1rem;
  margin-top: 0.75rem;
}
.poll-card {
  display: block;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 1.25rem;
  text-decoration: none;
  transition: border-color var(--transition), transform var(--transition);
}
.poll-card:hover {
  border-color: var(--color-accent);
  background: #fdf5dc;
  transform: translateY(-2px);
  text-decoration: none;
}
.poll-question {
  font-size: 0.95rem;
  font-weight: 500;
  color: var(--color-text);
  margin: 0.6rem 0 0.4rem;
  line-height: 1.4;
}
.poll-meta { font-size: 0.8rem; color: var(--color-muted); }
</style>
