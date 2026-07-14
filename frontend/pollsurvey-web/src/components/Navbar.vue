<template>
  <nav class="navbar">
    <div class="nav-inner container">
      <RouterLink to="/" class="nav-logo">
        <div class="logo-mark">P</div>
        <span class="logo-text">PollSurvey</span>
      </RouterLink>

      <div class="nav-right">
        <!-- Logged in -->
        <template v-if="auth.isLoggedIn">
          <span class="nav-user">👤 {{ auth.user?.username ?? 'Account' }}</span>
          <button class="nav-btn-outline" @click="auth.logout()">Sign out</button>
        </template>

        <!-- Not logged in -->
        <template v-else>
          <RouterLink to="/login" class="nav-btn-outline">Sign in</RouterLink>
        </template>

        <RouterLink to="/create" class="nav-cta">+ New Poll</RouterLink>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { useAuthStore } from '@/stores/authStore.js'
const auth = useAuthStore()
</script>

<style scoped>
.navbar {
  position: sticky; top: 0; z-index: 50;
  background: var(--color-accent);
  border-bottom: 1px solid rgba(0,0,0,0.18);
  box-shadow: 0 2px 12px rgba(107,14,30,0.2);
}
.nav-inner {
  display: flex; align-items: center;
  justify-content: space-between; height: 58px;
}
.nav-logo {
  display: flex; align-items: center; gap: .6rem; text-decoration: none;
}
.logo-mark {
  width: 30px; height: 30px; border-radius: 8px;
  background: rgba(245,222,141,0.18); border: 1px solid rgba(245,222,141,0.35);
  display: flex; align-items: center; justify-content: center;
  font-family: var(--font-display); font-size: 1rem; font-weight: 700; color: #f5de8d;
}
.logo-text {
  font-family: var(--font-display); font-size: 1.15rem;
  font-weight: 600; color: #f5de8d; letter-spacing: .01em;
}
.nav-right {
  display: flex; align-items: center; gap: .6rem;
}
.nav-user {
  font-size: .82rem; color: rgba(245,222,141,.8); font-weight: 500;
}
.nav-btn-outline {
  font-family: var(--font-body); font-size: .82rem; font-weight: 600;
  padding: .4rem .9rem; border-radius: var(--radius-sm);
  background: transparent; border: 1px solid rgba(245,222,141,.35);
  color: #f5de8d; cursor: pointer; text-decoration: none;
  transition: background var(--transition);
}
.nav-btn-outline:hover {
  background: rgba(245,222,141,.1); text-decoration: none;
}
.nav-cta {
  font-family: var(--font-body); font-size: .85rem; font-weight: 700;
  padding: .45rem 1.1rem; border-radius: var(--radius-sm);
  background: #f5de8d; color: var(--color-accent); border: none;
  cursor: pointer; letter-spacing: .01em; text-decoration: none;
  transition: background var(--transition), transform var(--transition);
}
.nav-cta:hover {
  background: #fff8e8; transform: translateY(-1px); text-decoration: none;
}
</style>
